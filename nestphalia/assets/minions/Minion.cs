using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class MinionTemplate
{
    public string ID; // Minions don't really need this since they're defined as a part of their nests, which already have IDs
    public string Name;
    public string Description;
    public Texture2D Texture;
    public double MaxHealth;
    public double Armor;
    public double Damage;
    public ProjectileTemplate Projectile;
    public double AttackDuration;
    public double Speed;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats
    public int WalkAnimDelay;

    public MinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackDuration = 1, int walkAnimDelay = 2)
    {
        ID = id;
        Name = name;
        Description = description;
        Texture = texture;
        MaxHealth = maxHealth;
        Armor = armor;
        Damage = damage;
        Projectile = new ProjectileTemplate($"{id}_attack", Resources.GetTextureByName("minion_bullet"), damage, 400);
        AttackDuration = attackDuration;
        Speed = speed;
        PhysicsRadius = physicsRadius;
        WalkAnimDelay = walkAnimDelay;
    }

    // Implementations of Instantiate() must call Register!
    public virtual void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new Minion(this, team, position, navPath));
    }
    private protected void Register(Minion minion)
    {
        World.Minions.Add(minion);
        World.Sprites.Add(minion);
    }
    
    public virtual string GetStats()
    {
        return
            $"{Name}\n" +
            $"HP: {MaxHealth}\n" +
            (Armor == 0 ? "" : $"Armor: {Armor}\n") +
            $"Speed: {Speed}\n" +
            $"Damage: {Projectile.Damage} ({Projectile.Damage / AttackDuration}/s)\n" +
            $"Size: {PhysicsRadius * 2}\n" +
            $"{Description}";
    }

    // Override this to return false for bugs that handle their own pathfinding on spawn
    public virtual bool PathFromNest()
    {
        return true;
    }
}

public partial class Minion : ISprite
{
    // Great and mighty State
    protected BaseState State;
    
    // Components
    public MinionTemplate Template;
    public Team Team;
    protected NavPath NavPath;
    
    // Public State
    public Vector2 Position;
    public double Health;
    public bool IsFlying;
    public Int2D OriginTile;
    protected Vector2 NextPos; // This is the world space position the minion is currently trying to reach
    
    // Status Effects
    public bool Glued;
    public bool Frenzy;
    
    // Private State - Try to encapsulate these into state classes
    private Vector2 _collisionOffset;
    private double _timeOfLastAction;
    private Color? _tintOverride;
    
    public double Z { get; set; }
    
    public Minion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath)
    {
        Template = template;
        Team = team;
        Position = position;
        NextPos = position;
        OriginTile = World.PosToTilePos(position);
        Health = Template.MaxHealth;
        _timeOfLastAction = Time.Scaled;
        // Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Pink, Color.Orange, Color.White, Color.Beige, Color.Black, Color.Brown, Color.DarkBlue, Color.Lime, Color.Magenta, Color.SkyBlue, Color.Violet, Color.Maroon, Color.Gold };
        // _tintOverride = colors[Random.Shared.Next(colors.Length)];
        
        State = new Move(this);

        if (navPath != null)
        {
            NavPath = navPath;
        }
        else
        {
            NavPath = new NavPath(template.Name, Team);
            // NavPath.Destination = GetNewTarget();
            // PathFinder.RequestPath(NavPath);
            SetTarget(GetNewTarget());
        }
        Z = Position.Y;
    }

    #region Events
    
    // Overrides must call base, or update the state machine themselves!
    public virtual void Update()
    {
        State.Update();
        Z = Position.Y + (IsFlying ? 240 : 0);
    }

    protected virtual void OnTargetReached()
    {
        SetTarget(GetNewTarget());
    }

    protected virtual void OnAttack()
    {
        Template.Projectile.Instantiate(World.GetTileAtPos(NextPos), this, Position);
    }
    
    public virtual void Draw()
    {
        DrawBug(State.GetAnimFrame());
        DrawDecorators();
        DrawDebug();
    }
    #endregion
    
    #region Protected Functions
    
    protected void UpdateNextPos()
    {
        Vector2 pos = World.GetTileCenter(NavPath.NextTile(Position));
        if (pos != NextPos)
        {
            NextPos = pos;
            _timeOfLastAction = Time.Scaled;
        }
    }

    protected virtual bool CheckIfLost()
    {
        if (Time.Scaled - _timeOfLastAction > 5)
        {
            Console.WriteLine($"{Template.Name} got lost");
            _timeOfLastAction = Time.Scaled;
            return true;
        }
        return false;
    }

    protected bool CanAttack()
    {
        // Guard against out of range attacks
        if (Vector2.Distance(Position, NextPos) > 24 + Template.PhysicsRadius) return false;
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position) != NavPath.Destination)) return false;
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid()) return false; 

        return true;
    }
        
    // Returns move speed adjusted for glue, mine anxiety, etc.
    protected double AdjustedSpeed()
    {
        double adjustedSpeed = Template.Speed;
        Structure? structure = World.GetTileAtPos(Position);
        if (Glued || (!IsFlying && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
        if (Frenzy) adjustedSpeed *= 2;
        return adjustedSpeed;
    }
    
    public void ApplyPush()
    {
        // Cap push force to 12 px/frame, hopefully this will prevent bugs from getting embedded in walls by mega crowd crush events
        if (_collisionOffset.Length() > 6)
        {
            Position += _collisionOffset.Normalized() * 6;
        }
        else
        {
            Position += _collisionOffset;
        }
        _collisionOffset = Vector2.Zero;
    }

    protected Int2D GetNewTarget()
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? s = World.GetTile(x, y);
                if (s != null && s.Team != Team && s is not Rubble)
                {
                    targets.Add(new Sortable<Int2D>(-Vector2.Distance(Position, World.GetTileCenter(x,y)), new Int2D(x,y)));
                }
            }
        }
        
        targets = targets.OrderByDescending(x=>x.Order).ToList();
        
        if (targets.Count > 32)
        { 
            targets.RemoveRange(32, targets.Count-32);
        }

        foreach (Sortable<Int2D> t in targets)
        {
            t.Order += Team.GetHateFor(t.Value);
        }
        targets = targets.OrderByDescending(x=>x.Order).ToList();

        if (targets.Count == 0)
        {
            Console.WriteLine($"{Template.Name} wants to retarget but can't see any valid targets!");
            return new Int2D(0,0);
        }

        // NavPath.Reset(Position);
        int i = Math.Min(targets.Count, 16);
        i = Math.Min(World.RandomInt(i), World.RandomInt(i));
        // i = World.Random.WeightedRandom(i);
        return targets[i].Value;
    }

    protected void DrawBug(int frame)
    {
        int size = Template.Texture.Height / 2;
        Vector2 pos = new Vector2(Position.X - size / 2f, Position.Y - size / 2f);
        bool flip = NextPos.X > Position.X;
        Rectangle source = new Rectangle(flip ? size : 0, 0, flip ? size : -size, size);
        source.X = size * frame;
        Raylib.DrawTextureRec(Template.Texture, source, pos, Color.White);
        source.Y += size;
        Raylib.DrawTextureRec(Template.Texture, source, pos, _tintOverride ?? Team.UnitTint);
    }
    
    protected void DrawDecorators()
    {
        // Health Bar
        if (Health < Template.MaxHealth)
        {
            Vector2 start = Position - new Vector2(Template.PhysicsRadius, Template.PhysicsRadius + 2);
            Vector2 end = start + new Vector2((float)(2 * Template.PhysicsRadius * (Health / Template.MaxHealth)), 0);
            Raylib.DrawLineEx(start, end, 1, new Color(32, 192, 32, 255));
        }
        
        // State Decorators
        State.DrawDecorators();
    }

    protected void DrawDebug()
    {
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, 2*Template.PhysicsRadius))
        {
            if (World.DrawDebugInfo) Raylib.DrawCircleV(NextPos, 2, Color.Green);
            
            Vector2 path = Position;
            foreach (Int2D i in NavPath.Waypoints)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
                path = v;
            }

            if (NavPath.Waypoints.Count == 0)
            {
                Vector2 v = World.GetTileCenter(NavPath.Destination);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
            }
            
            if (World.DrawDebugInfo) GUI.DrawTextLeft((int)Position.X, (int)Position.Y, State.ToString(), guiSpace: false);
        }
    }
    #endregion

    #region Public Functions
    
    public void Push(Vector2 distance)
    {
        _collisionOffset += distance;
    }
    
    public void Hurt(double damage, Projectile? damageSource = null)
    {
        // guard against second bullet in same frame.
        if (Health <= 0) return;

        Health -= Math.Max(1, damage - Template.Armor);
        if (Health <= 0)
        {
            Team.AddFearOf(Template.MaxHealth/10, World.PosToTilePos(Position));
            //Console.WriteLine($"{Template.Name} killed by {damageSource.Source.GetType().Name} which is " + (damageSource.Source is Structure ? "" : "not ") + "a structure, assigning hate...");
            if (damageSource?.Source is Structure s)
            {
                //Console.WriteLine($"{Template.Name} killed by {s.Template.Name}, assigning hate...");
                Int2D pos = s.GetTilePos();
                Team.AddHateFor(Template.MaxHealth/10, pos.X, pos.Y);
            }
            Die();
        }
    }

    public virtual void Die()
    {
        World.MinionsToRemove.Add(this);
    }

    public Int2D GetTargetTile()
    {
        return NavPath.Destination;
    }

    public virtual void SetTarget(Int2D target, double thinkDuration = 0.5)
    {
        NavPath.Reset(Position);
        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Destination = target;
        Team.RequestPath(NavPath);
        
        // Wait a bit, then force pathfinding if it hasn't happened yet.
        State = new Wait(this, thinkDuration, () =>
        {
            if (!NavPath.Found)
            {
                Team.DemandPath(NavPath);
            }
            State = new Move(this);
        }, Resources.GetTextureByName("particle_confused"));
    }
    #endregion
}
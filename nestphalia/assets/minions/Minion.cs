using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public abstract class MinionTemplate
{
    public string ID; // Minions don't really need this since they're defined as a part of their nests, which already have IDs
    public string Name;
    public string Description;
    public Texture2D Texture;
    public double MaxHealth;
    public double Armor;
    public double Damage;
    public ProjectileTemplate Projectile;
    public double AttackCooldown;
    public double Speed;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats

    public MinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1)
    {
        ID = id;
        Name = name;
        Description = description;
        Texture = texture;
        MaxHealth = maxHealth;
        Armor = armor;
        Damage = damage;
        Projectile = new ProjectileTemplate($"{id}_attack", Resources.GetTextureByName("minion_bullet"), damage, 400);
        AttackCooldown = attackCooldown;
        Speed = speed;
        PhysicsRadius = physicsRadius;
    }

    // Implementations of Instantiate() must call Register!
    public abstract void Instantiate(Team team, Vector2 position, NavPath? navPath);
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
            $"Damage: {Projectile.Damage} ({Projectile.Damage / AttackCooldown}/s)\n" +
            $"Size: {PhysicsRadius * 2}\n" +
            $"{Description}";
    }

    // Override this to return false for bugs that handle their own pathfinding on spawn
    public virtual bool PathFromNest()
    {
        return true;
    }
}

public abstract class Minion : ISprite
{
    public MinionTemplate Template;
    public Team Team;
    protected NavPath NavPath;
    public Vector2 Position;
    public double Health;
    private double _timeSinceLastAction;
    protected Vector2 NextPos; // This is the world space position the minion is currently trying to reach
    private bool _attacking;
    private double _attackStartedTime;
    // private double _lastFiredTime;
    private Vector2 _collisionOffset;
    public Int2D OriginTile;
    public bool Glued;
    public bool IsFlying;
    public bool Frenzy;
    
    public enum MinionState
    {
        Waiting,
        Moving,
        Attacking
    }
    
    public double Z { get; set; }
    
    public Minion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath)
    {
        Template = template;
        Team = team;
        Position = position;
        NextPos = position;
        OriginTile = World.PosToTilePos(position);
        Health = Template.MaxHealth;
        _timeSinceLastAction = Time.Scaled;

        if (navPath != null)
        {
            NavPath = navPath;
        }
        else
        {
            NavPath = new NavPath(template.Name, Team);
            Retarget();
            PathFinder.RequestPath(NavPath);
        }
        Z = Position.Y;
    }

    public abstract void Update();
    
    protected void UpdateNextPos()
    {
        Vector2 pos = World.GetTileCenter(NavPath.NextTile(Position));
        if (pos != NextPos)
        {
            NextPos = pos;
            _timeSinceLastAction = Time.Scaled;
            return;
        }
        
        if (Time.Scaled - _timeSinceLastAction > 5)
        {
            Console.WriteLine($"{Template.Name} got lost");
            NavPath.Reset(Position);
            PathFinder.RequestPath(NavPath);
            _timeSinceLastAction = Time.Scaled;
        }
    }

    // Attempts to attack, returns true if attack target is valid 
    protected bool TryAttack()
    {
        // Guard against out of range attacks
        if (Vector2.Distance(Position, NextPos) > 24 + Template.PhysicsRadius) { _attacking = false; return false;}
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position) != NavPath.Destination)) { _attacking = false; return false;}
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid()) { _attacking = false; return false; }
        _timeSinceLastAction = Time.Scaled;
        if (!_attacking)
        {
            _attacking = true;
            _attackStartedTime = Time.Scaled;
        }
        if (Time.Scaled - _attackStartedTime >= (Frenzy ? Template.AttackCooldown/2 : Template.AttackCooldown))
        {
            Template.Projectile.Instantiate(t, this, Position);
            _attackStartedTime = Time.Scaled;
        }
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
        Position += _collisionOffset;
        _collisionOffset = Vector2.Zero;
    }

    protected void Retarget()
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
            return;
        }

        NavPath.Reset(Position);
        int i = Math.Min(targets.Count, 16);
        i = Math.Min(World.Random.Next(i), World.Random.Next(i));
        i = World.Random.WeightedRandom(i);
        NavPath.Destination = targets[i].Value;
    }

    public virtual void Draw()
    {
        Z = Position.Y + (IsFlying ? 240 : 0);

        Vector2 pos = new Vector2((int)Position.X - Template.Texture.Width / 2f, (int)Position.Y - Template.Texture.Width / 2f);
        bool flip = NextPos.X > (int)Position.X;
        Rectangle source = new Rectangle(flip ? Template.Texture.Width : 0, 0, flip ? Template.Texture.Width : -Template.Texture.Width, Template.Texture.Height);
        Raylib.DrawTextureRec(Template.Texture, source, pos, Team.UnitTint);

        DrawHealthBar();
        DrawDebug();
    }

    protected void DrawHealthBar()
    {
        if (Health < Template.MaxHealth)
        {
            Vector2 start = Position - new Vector2(Template.Texture.Width / 2f, Template.Texture.Height / 2f + 2);
            Vector2 end = start + new Vector2((float)(Template.Texture.Width * (Health / Template.MaxHealth)), 0);
            Raylib.DrawLineEx(start, end, 1, new Color(32, 192, 32, 255));
        }
    }

    protected void DrawDebug()
    {
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, 2*Template.PhysicsRadius))
        {
            Raylib.DrawCircleV(NextPos, 2, Color.Green);
            
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
        }
    }

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

    protected virtual void Die()
    {
        World.MinionsToRemove.Add(this);
    }

    public Int2D GetTargetTile()
    {
        return NavPath.Destination;
    }

    public virtual void SetTarget(Int2D target)
    {
        NavPath.Reset(Position);
        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Destination = target;
        PathFinder.RequestPath(NavPath);
    }
}
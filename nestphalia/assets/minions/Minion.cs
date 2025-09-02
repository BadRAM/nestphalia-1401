using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MinionTemplate : JsonAsset
{
    public string Name;
    public string Description;
    public Texture2D Texture;
    public Texture2D ShadowTexture;
    public double MaxHealth;
    public double Armor;
    public double Damage;
    public ProjectileTemplate Projectile;
    public double AttackDuration;
    public double Speed;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats
    public int WalkAnimDelay;

    public MinionTemplate(JObject jObject) : base(jObject)
    {
        Name = jObject.Value<string?>("Name") ?? throw new ArgumentNullException();
        Description = jObject.Value<string?>("Description") ?? "";
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        ShadowTexture = Resources.GetTextureByName("shadow");
        MaxHealth = jObject.Value<double?>("MaxHealth") ?? throw new ArgumentNullException();
        Armor = jObject.Value<double?>("Armor") ?? 0;
        Damage = jObject.Value<double?>("Damage") ?? 0;
        AttackDuration = jObject.Value<double?>("AttackDuration") ?? 1;
        Speed = jObject.Value<double?>("Speed") ?? 0;
        PhysicsRadius = jObject.Value<int?>("PhysicsRadius") ?? throw new ArgumentNullException();
        WalkAnimDelay = jObject.Value<int?>("WalkAnimDelay") ?? 2;

        string j = $@"{{""ID"": ""{ID}_attack"", ""Texture"": ""minion_bullet"", ""Damage"": {Damage}, ""Speed"": 400}}";
        Projectile = new ProjectileTemplate(JObject.Parse(j));
    }

    // Implementations of Instantiate() must call Register!
    public virtual Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        Minion m = new Minion(this, team, position, navPath);
        World.RegisterMinion(m);
        return m;
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

    public virtual void DrawPreview(int x, int y, Color teamColor)
    {
        int size = Texture.Height / 2;
        Vector2 pos = new Vector2(x - size / 2f, y - size / 2f);
        Rectangle source = new Rectangle(0, 0, size, size);
        Raylib.DrawTextureRec(Texture, source, pos, Color.White);
        source.Y += size;
        Raylib.DrawTextureRec(Texture, source, pos, teamColor);
    }
    
    // This is called by nests to get the initial path, and also by minions to repath
    public virtual void RequestPath(Int2D startPos, Int2D targetPos, NavPath navPath, Team team, Minion minion = null)
    {
        navPath.Reset(startPos);
        navPath.Destination = targetPos;
        team.RequestPath(navPath);
    }
}

public partial class Minion : ISprite
{
    // Great and mighty State
    protected BaseState State;
    
    // Components
    public MinionTemplate Template;
    public Team Team;
    public NavPath NavPath;
    
    // Public State
    public Vector3 Position;
    public Vector3 DrawOffset;
    public double Health;
    public double Armor;
    public bool IsFlying;
    public bool IsOnTopOfStructure;
    public Int2D OriginTile;
    protected Vector2 NextPos; // This is the world space position the minion is currently trying to reach
    
    // Status Effects
    public bool StandardBearer;
    public bool Glued;
    public bool Frenzy;
    
    // Private State - Try to encapsulate these into state classes
    private Vector2 _collisionOffset;
    private double _timeOfLastAction;
    private Color? _tintOverride;
    
    // public double Z { get; set; }
    
    public Minion(MinionTemplate template, Team team, Vector3 position, NavPath? navPath)
    {
        Template = template;
        Team = team;
        Position = position;
        NextPos = position.XY();
        OriginTile = World.PosToTilePos(position);
        Health = Template.MaxHealth;
        Armor = Template.Armor;
        _timeOfLastAction = Time.Scaled;
        // // Party mode, randomly colors all minions
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
            SetTarget(GetNewTarget());
        }
    }

    #region Events
    
    // Overrides must call base, or update the state machine themselves!
    public virtual void Update()
    {
        State.Update();
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
        if (IsFlying && NavPath.Found)
        {
            NextPos = World.GetTileCenter(NavPath.Destination);
            _timeOfLastAction = Time.Scaled;
            return;
        }
        Vector2 pos = World.GetTileCenter(NavPath.NextTile(Position.XY()));
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
            // Console.WriteLine($"{Template.Name} got lost");
            _timeOfLastAction = Time.Scaled;
            return true;
        }
        return false;
    }

    protected virtual bool CanAttack()
    {
        // Don't attack while we're on top of structures
        if (IsOnTopOfStructure) return false;
        // Guard against out of range attacks
        if (Vector2.Distance(Position.XY(), NextPos) > 24 + Template.PhysicsRadius) return false;
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position.XY()) != NavPath.Destination)) return false;
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid(this)) return false; 

        return true;
    }
        
    // Returns move speed adjusted for glue, mine anxiety, etc.
    protected virtual double AdjustedSpeed()
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
        if (_collisionOffset.Length() > 12)
        {
            Position += (_collisionOffset.Normalized() * 12).XYZ();
        }
        else
        {
            Position += (_collisionOffset).XYZ();
        }
        _collisionOffset = Vector2.Zero;
    }

    protected Int2D GetNewTarget()
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        for (int y = 0; y < World.BoardHeight; ++y)
        {
            Structure? s = World.GetTile(x, y);
            if (s != null && s.Team != Team && s is not Rubble)
            {
                targets.Add(new Sortable<Int2D>(-Vector2.Distance(Position.XY(), World.GetTileCenter(x,y)), new Int2D(x,y)));
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
            GameConsole.WriteLine($"{Template.Name} wants to retarget but can't see any valid targets!");
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
        Vector2 pos = new Vector2(Position.X - size / 2f, Position.Y - size / 2f - Position.Z) + DrawOffset.XYZ2D();
        if (!IsFlying && IsOnTopOfStructure) pos.Y -= 8;
        bool flip = NextPos.X > Position.X;
        if (StandardBearer) Raylib.DrawTextureV(Team.BattleStandard, pos - new Vector2(Team.BattleStandard.Width/2-size/2, Team.BattleStandard.Height-size/2), Team.Color);
        Rectangle source = new Rectangle(flip ? size : 0, 0, flip ? size : -size, size);
        source.X = size * frame;
        Raylib.DrawTextureRec(Template.Texture, source, pos, Color.White);
        source.Y += size;
        Raylib.DrawTextureRec(Template.Texture, source, pos, _tintOverride ?? Team.Color);
        if (Position.Z + DrawOffset.Z > 0)
        {
            pos = new Vector2(Position.X - Template.ShadowTexture.Width / 2f, Position.Y - Template.ShadowTexture.Height / 2f) + DrawOffset.XY();
            if (IsOnTopOfStructure) pos.Y -= 8;
            Raylib.DrawTextureV(Template.ShadowTexture, pos, Raylib.ColorAlpha(Color.White, Math.Min((Position.Z + DrawOffset.Z)/12, 1)));
        }
    }
    
    protected void DrawDecorators()
    {
        // Health Bar
        if (Health < Template.MaxHealth)
        {
            Vector2 start = Position.XY() - new Vector2(Template.PhysicsRadius, Template.PhysicsRadius + 2 + Position.Z);
            Vector2 end = start + new Vector2((float)(2 * Template.PhysicsRadius * (Health / Template.MaxHealth)), 0);
            Raylib.DrawLineEx(start, end, 1, new Color(32, 192, 32, 255));
        }
        
        // State Decorators
        State.DrawDecorators();
    }

    protected void DrawDebug()
    {
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position.XYZ2D(), 2*Template.PhysicsRadius))
        {
            if (Screen.DebugMode) Raylib.DrawCircleV(NextPos, 2, Color.Green);

            if (Position.Z > 0)
            {
                Raylib.DrawLineV(Position.XYZ2D(), Position.XY(), Color.SkyBlue);
            }
            
            Vector2 path = Position.XY();
            foreach (Int2D i in NavPath.Points)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
                path = v;
            }

            if (NavPath.Points.Count == 0)
            {
                Vector2 v = World.GetTileCenter(NavPath.Destination);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
            }
            
            Raylib.DrawCircleV(World.GetTileCenter(NavPath.Destination), 3, Color.Red);

            if (Screen.DebugMode)
            {
                Raylib.DrawCircleLinesV(Position.XY(), Template.PhysicsRadius, Color.Orange);
                Raylib.DrawLineV(Position.XY(), Position.XY() + DrawOffset.XYZ2D(), Color.Orange);
                GUI.DrawTextLeft((int)Position.X, (int)Position.Y, State.ToString(), anchor: Screen.TopLeft);
            }
        }
    }
    #endregion

    #region Public Functions
    
    public void Push(Vector2 distance)
    {
        _collisionOffset += distance;
    }
    
    public virtual void Hurt(double damage, Projectile? damageSource = null)
    {
        // guard against second bullet in same frame.
        if (Health <= 0) return;

        Health -= Math.Max(1, damage - Armor);
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
        Template.RequestPath(World.PosToTilePos(Position), target, NavPath, Team, this);

        WaitForPath(thinkDuration);
    }
    
    public virtual NavPath WaitForPath(double thinkDuration = 0.5)
    {
        if (State is Jump) return NavPath;

        // Wait a bit, then force pathfinding if it hasn't happened yet.
        SetState(new Wait(this, thinkDuration, () =>
            {
                if (!NavPath.Found)
                {
                    Team.DemandPath(NavPath);
                }
                SetState(new Move(this));
            }, 
            Resources.GetTextureByName("particle_confused")));
        return NavPath;
    }
    
    public double GetDrawOrder()
    {
        return Position.Y;
    }

    public bool SetState(BaseState state)
    {
        return State.Exit(state);
    }

    public void SetStateFlee(List<Int2D> escapePoints)
    {
        NavPath.Reset(Position);
        NavPath.Points.AddRange(escapePoints);
        Team.RequestPath(NavPath);

        SetState(new Wait(this, 0.1, () =>
            {
                if (!NavPath.Found)
                {
                    Team.DemandPath(NavPath);
                }
                SetState(new Flee(this));
            }, 
            Resources.GetTextureByName("particle_confused")));
    }
    
    public bool IsOrigin(int x, int y)
    {
        return OriginTile.X == x && OriginTile.Y == y;
    }
    
    #endregion
}
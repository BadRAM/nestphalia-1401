using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MinionTemplate : JsonAsset
{
    public string Name;
    public string Description;
    public double MaxHealth;
    public double Armor;
    public double Damage;
    public AttackTemplate Attack;
    public double AttackDuration;
    public double Speed;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats
    public Minion.StateType DefaultState;
    public Minion.StateType AttackType;
    public int WalkAnimDelay;
    public Texture2D Texture;
    public Texture2D ShadowTexture;
    public Vector2 SpriteSize;
    public Dictionary<Minion.AnimationState, List<Vector2>> SpriteFrames;

    public MinionTemplate(JObject jObject) : base(jObject)
    {
        Name = jObject.Value<string?>("Name") ?? throw new ArgumentNullException();
        Description = jObject.Value<string?>("Description") ?? "";
        MaxHealth = jObject.Value<double?>("MaxHealth") ?? throw new ArgumentNullException();
        Armor = jObject.Value<double?>("Armor") ?? 0;
        Damage = jObject.Value<double?>("Damage") ?? 0;
        AttackDuration = jObject.Value<double?>("AttackDuration") ?? 1;
        Speed = jObject.Value<double?>("Speed") ?? 0;
        PhysicsRadius = jObject.Value<int?>("PhysicsRadius") ?? throw new ArgumentNullException();
        DefaultState = Enum.Parse<Minion.StateType>(jObject.Value<string?>("DefaultState") ?? "Move");
        AttackType = Enum.Parse<Minion.StateType>(jObject.Value<string?>("AttackType") ?? "MeleeAttack");
        WalkAnimDelay = jObject.Value<int?>("WalkAnimDelay") ?? 2;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        ShadowTexture = Resources.GetTextureByName(jObject.Value<string?>("ShadowTexture") ?? "shadow");
        SpriteSize = jObject.Value<JObject?>("SpriteSize")?.ToObject<Vector2>() ?? Vector2.One * Texture.Height/2;
        SpriteFrames = jObject.Value<JObject?>("SpriteFrames")?.ToObject<Dictionary<Minion.AnimationState, List<Vector2>>>() ?? new Dictionary<Minion.AnimationState, List<Vector2>>();
        if (!SpriteFrames.ContainsKey(Minion.AnimationState.Base))
        {
            SpriteFrames.Add(Minion.AnimationState.Base, new List<Vector2>([Vector2.Zero]));
        }

        string j = $@"{{""ID"": ""{ID}_attack"", ""Texture"": ""minion_bullet"", ""Damage"": {Damage}, ""Speed"": 400}}";
        Attack = new MeleeAttackTemplate(JObject.Parse(j));
    }

    // Implementations of Instantiate() must call Register!
    public virtual Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        return Register(new Minion(this, team, position, navPath));
    }
    
    public static Minion Register(Minion minion)
    {
        World.Minions.Add(minion);
        World.MinionsByID.Add(minion.ID, minion);
        World.Sprites.Add(minion);
        return minion;
    }
    
    public virtual string GetStats()
    {
        return
            $"{Name}\n" +
            $"HP: {MaxHealth}\n" +
            (Armor == 0 ? "" : $"Armor: {Armor}\n") +
            $"Speed: {Speed}\n" +
            $"Damage: {Attack.Damage} ({Attack.Damage / AttackDuration}/s)\n" +
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

    public Rectangle GetAnimationFrame(Minion.AnimationState animation, int frame)
    {
        if (!SpriteFrames.ContainsKey(animation))
        {
            return new Rectangle(SpriteFrames[Minion.AnimationState.Base][0], SpriteSize);
        }
        List<Vector2> frames = SpriteFrames[animation];
        return new Rectangle(frames[frame % frames.Count], SpriteSize);
    }

    public int GetAnimationFrameCount(Minion.AnimationState animation)
    {
        if (!SpriteFrames.ContainsKey(animation)) return 0;
        return SpriteFrames[animation].Count;
    }
}

public partial class Minion : ISprite, IMortal
{
    public readonly int ID;
    
    // Components
    protected BaseState State; 
    public MinionTemplate Template;
    public Team Team { get; }
    public NavPath NavPath;
    public StatusCollect Status;
    
    // Public State
    public Vector3 Position;
    public Vector3 DrawOffset;
    public double Health { get; set; }
    public double Armor;
    public bool IsFlying;
    public bool IsOnTopOfStructure;
    public Int2D Origin { get; }
    protected Vector2 NextPos; // This is the world space position the minion is currently trying to reach
    
    // Status Effects
    
    // Private State - Try to encapsulate these into state classes
    private Vector2 _collisionOffset;
    private double _timeOfLastAction;
    private Color? _tintOverride;
    
    // public double Z { get; set; }
    
    public Minion(MinionTemplate template, Team team, Vector3 position, NavPath? navPath)
    {
        ID = World.GetMinionId();
        Template = template;
        Team = team;
        Status = new StatusCollect(this);
        Position = position;
        NextPos = position.XY();
        Origin = World.PosToTilePos(position);
        Health = Template.MaxHealth;
        Armor = Template.Armor;
        _timeOfLastAction = Time.Scaled;
        // // Party mode, randomly colors all minions
        // Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Pink, Color.Orange, Color.White, Color.Beige, Color.Black, Color.Brown, Color.DarkBlue, Color.Lime, Color.Magenta, Color.SkyBlue, Color.Violet, Color.Maroon, Color.Gold };
        // _tintOverride = colors[Random.Shared.Next(colors.Length)];
        
        ResetState(Template.DefaultState);

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
        Status.Update();
    }

    protected virtual void OnTargetReached()
    {
        SetTarget(GetNewTarget());
    }

    protected virtual void DoAttack()
    {
        Structure target = World.GetTileAtPos(NextPos);
        if (target == null)
        {
            GameConsole.WriteLine($"{Template.ID} attacked an empty tile");
            Template.Attack.Instantiate(NextPos.XYZ(), this, Position);
            return;
        }
        Template.Attack.Instantiate(target, this, Position);
    }
    
    public virtual void Draw()
    {
        DrawBug(State.GetAnimFrame());
        DrawDecorators();
        Status.Draw();
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
    protected virtual double AdjustSpeed(double BaseSpeed)
    {
        BaseSpeed = Template.Speed;
        Structure? structure = World.GetTileAtPos(Position);
        if (Status.Has("Glued") || (!IsFlying && structure?.Team == Team && structure is Minefield)) BaseSpeed *= 0.5;
        if (Status.Has("Frenzy")) BaseSpeed *= 2;
        return BaseSpeed;
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

        int i = Math.Min(targets.Count, 16);
        i = Math.Min(World.RandomInt(i), World.RandomInt(i));
        // i = World.WeightedRandom(i);
        return targets[i].Value;
    }

    protected void DrawBug(Rectangle frame)
    {
        Vector2 size = Template.SpriteSize;
        Vector2 pos = Position.XYZ2D() - size/2 + DrawOffset.XYZ2D();
        if (!IsFlying && IsOnTopOfStructure) pos.Y -= 8;
        if (NextPos.X < Position.X) frame = frame.FlipX();
        if (Status.Has("StandardBearer")) Raylib.DrawTextureV(Team.BattleStandard, pos - new Vector2(Team.BattleStandard.Width/2-size.X/2, Team.BattleStandard.Height-size.Y/2), _tintOverride ?? Team.Color);
        Color baseTint = Color.White;
        if (Status.Has("Burning")) baseTint = Color.Orange;
        Raylib.DrawTextureRec(Template.Texture, frame, pos, baseTint);
        frame.Y += size.Y;
        Raylib.DrawTextureRec(Template.Texture, frame, pos, _tintOverride ?? Team.Color);
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

    public void DrawDebug(bool overrideHover = false)
    {
        if (overrideHover || Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position.XYZ2D(), 2*Template.PhysicsRadius))
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
    
    public virtual void Hurt(double damage, Attack? damageSource = null, bool ignoreArmor = false, bool minDamage = true)
    {
        // guard against dying twice one frame.
        if (Health <= 0) return;

        if (!ignoreArmor) damage -= Armor;
        damage = Math.Max(minDamage ? 1 : 0, damage);
        if (damage <= 0) return;
        
        Team.AddFearOf(damage/10, World.PosToTilePos(Position));
        if (damageSource?.Source?.Origin != null)
        {
            Team.AddHateFor(damage/10, damageSource.Source.Origin);
        }
        
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    // This is for IMortal
    public Vector3 GetPos()
    {
        return Position;
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
    
    public NavPath WaitForPath(double thinkDuration = 0.5)
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
    
    // This is used to transition between 'generic' states that take no arguments, so that they can be modularized as data in minionTemplates
    public void ResetState(StateType stateType)
    {
        switch (stateType)
        {
            case StateType.Move:
                State = new Move(this);
                break;
            case StateType.SwoopAttack:
                State = new SwoopAttack(this);
                break;
            case StateType.MeleeAttack:
                State = new MeleeAttack(this);
                break;
            case StateType.RangedAttack:
                State = new RangedAttack(this);
                break;
            case StateType.Cheer:
                State = new Cheer(this);
                break;
            case StateType.Flee:
                State = new Flee(this);
                break;
            case StateType.Wait:
            case StateType.Jump:
                throw new Exception($"State: {stateType.ToString()} needs arguments!");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
        }
    }

    // This one is used for direct state changes. Returns false if state change rejected.
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
        return Origin.X == x && Origin.Y == y;
    }
    
    public string GetStateString()
    {
        return State.ToString();
    }
    
    #endregion
}
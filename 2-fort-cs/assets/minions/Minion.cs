using System.Numerics;
using CommunityToolkit.HighPerformance.Helpers;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class MinionTemplate
{
    public string ID; // Minions don't really need this since they're defined as a part of their nests, which already have IDs
    public string Name;
    public string Description;
    public Texture Texture;
    public double MaxHealth;
    public double Armor;
    public double Damage;
    public ProjectileTemplate Projectile;
    public double AttackCooldown;
    public double Speed;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats

    public MinionTemplate(string id, string name, string description, Texture texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1)
    {
        ID = id;
        Name = name;
        Description = description;
        Texture = texture;
        MaxHealth = maxHealth;
        Armor = armor;
        Damage = damage;
        Projectile = new ProjectileTemplate(Resources.GetTextureByName("minion_bullet"), damage, 400);
        AttackCooldown = attackCooldown;
        Speed = speed;
        PhysicsRadius = physicsRadius;
    }

    public virtual void Instantiate(Vector2 position, Team team, NavPath? navPath)
    {
        Minion m = new Minion(this, team, position, navPath);
        World.Minions.Add(m);
        World.Sprites.Add(m);
    }
    
    public virtual string GetStats()
    {
        return "Function under construction";
    }
}

public class Minion : ISprite
{
    public MinionTemplate Template;
    public Team Team;
    protected NavPath NavPath;
    public Vector2 Position;
    public double Health;
    public bool IsAlive;
    protected Vector2 Target;
    private bool _attacking;
    private double _attackStartedTime;
    // private double _lastFiredTime;
    private Vector2 _collisionOffset;
    public bool Glued;
    public bool IsFlying;
    public bool Frenzy;
    
    public double Z { get; set; }

    public Minion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath)
    {
        Template = template;
        Position = position;
        Target = position;
        //_targetTile = targetTile;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;

        if (navPath != null)
        {
            NavPath = navPath;
        }
        else
        {
            NavPath = new NavPath(Team);
            Retarget();
        }
        //_path = new PathFinder(this);
        //_path.FindPath(_targetTile);
        Z = Position.Y;
    }
    
    public virtual void Update()
    {
        // if the next tile in our path is adjacent and solid, then attack it
        Target = World.GetTileCenter(NavPath.NextTile(Position));
        if (!TryAttack())
        {
            // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
            if (NavPath.Found && NavPath.TargetReached(Position))
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
            // else, move towards next tile on path.
            Position = Position.MoveTowards(Target, AdjustedSpeed() * Time.DeltaTime);
        }
        
        Frenzy = false;
    }

    // Attempts to attack, returns true if attack target is valid 
    protected virtual bool TryAttack()
    {
        if (Vector2.Distance(Position, Target) > 24 + Template.PhysicsRadius) { _attacking = false; return false;}
        Structure? t = World.GetTileAtPos(Target);
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position) != NavPath.Destination)) { _attacking = false; return false;}
        if (t.Team == Team && !t.PhysSolid(Team)) { _attacking = false; return false;}
        if (!_attacking)
        {
            _attacking = true;
            _attackStartedTime = Time.Scaled;
        }
        if (Time.Scaled - _attackStartedTime >= (Frenzy ? Template.AttackCooldown/2 : Template.AttackCooldown))
        {
            Template.Projectile.Instantiate(t, this);
            _attackStartedTime = Time.Scaled;
        }
        return true;
    }
        
    // Returns move speed adjusted for glue, mine anxiety, etc.
    public double AdjustedSpeed()
    {
        double adjustedSpeed = Template.Speed;
        Structure? structure = World.GetTileAtPos(Position);
        if (Glued || (!IsFlying && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
        if (Frenzy) adjustedSpeed *= 2;
        return adjustedSpeed;
    }
    
    // Should this be here, or in World? maybe somewhere else entirely, like a physics functions class?
    public void PlanCollision(int index)
    {
        for (int i = index+1; i < World.Minions.Count; i++)
        {
            // Abort loop if we're outside of our X band
            if (World.Minions[i].Position.X - Position.X > Template.PhysicsRadius + 12) break;
            if (World.Minions[i].IsFlying != IsFlying) continue;
            if (!Raylib.CheckCollisionCircles(Position, Template.PhysicsRadius, World.Minions[i].Position, World.Minions[i].Template.PhysicsRadius)) continue;
            if (Position == World.Minions[i].Position)
            {
                                 _collisionOffset += new Vector2((float)(Random.Shared.NextDouble() - 0.5), (float)(Random.Shared.NextDouble() - 0.5));
                World.Minions[i]._collisionOffset += new Vector2((float)(Random.Shared.NextDouble() - 0.5), (float)(Random.Shared.NextDouble() - 0.5));
                continue;
            }
            
            Vector2 delta = Position - World.Minions[i].Position;
                             _collisionOffset += delta.Normalized() * Math.Min((Template.PhysicsRadius + World.Minions[i].Template.PhysicsRadius - delta.Length())/2, 0.5f);
            World.Minions[i]._collisionOffset -= delta.Normalized() * Math.Min((Template.PhysicsRadius + World.Minions[i].Template.PhysicsRadius - delta.Length())/2, 0.5f);
        }

        if (IsFlying) return;
        Int2D tilePos = World.PosToTilePos(Position);
        
        for (int x = tilePos.X-1; x < tilePos.X+3; ++x)
        {
            for (int y = tilePos.Y-1; y < tilePos.Y+3; ++y)
            {
                // guard against out of bounds, non-solid tiles, and friendly hives
                if (x < 0 || 
                    x >= World.BoardWidth || 
                    y < 0 || 
                    y >= World.BoardHeight || 
                    !(World.GetTile(x,y)?.PhysSolid(Team) ?? false) ||
                    (World.GetTile(x,y) is Spawner && World.GetTile(x,y).Team == Team)
                    ) continue;
                Vector2 c = World.GetTileCenter(x, y);
                Rectangle b = World.GetTileBounds(x, y);
                if (!Raylib.CheckCollisionCircleRec(Position, Template.PhysicsRadius, b)) continue;
                {
                    if (Position.X > b.X && Position.X < b.X + b.Width) // circle center is in tile X band
                    {
                        // Find desired Y for above or below cases
                        double desiredY = Position.Y > c.Y ? b.Y + b.Height + Template.PhysicsRadius : b.Y - Template.PhysicsRadius;
                        _collisionOffset.Y = (float)desiredY - Position.Y;
                    }
                    else if (Position.Y > b.Y && Position.Y < b.Y + b.Height) // Circle Center is in tile Y band 
                    {
                        double desiredX = Position.X > c.X ? b.X + b.Width + Template.PhysicsRadius : b.X - Template.PhysicsRadius;
                        _collisionOffset.X = (float)desiredX - Position.X;
                    }
                    // else // Circle Center is in tile corner region
                    // {
                    //     Vector2 corner = new Vector2
                    //     (
                    //         Position.X > c.X ? b.X : b.X + b.Width,
                    //         Position.Y > c.Y ? b.Y : b.Y + b.Height
                    //     );
                    //     Vector2 delta = Position - corner;
                    //     _collisionOffset += delta.Normalized() * (Template.PhysicsRadius - delta.Length());
                    // }
                }
            }
        }
    }
    
    public void LateUpdate()
    {
        Position += _collisionOffset;
        _collisionOffset = Vector2.Zero;
    }

    protected void Retarget()
    {
        //SortedList<double, Int2D> targets = new SortedList<double, Int2D>();
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? s = World.GetTile(x, y);
                if (s != null && s.Team != Team && s is not Rubble)
                {
                    targets.Add(new Sortable<Int2D>(-Vector2.Distance(Position, World.GetTileCenter(x,y)), new Int2D(x,y)));
                    // targets.Add(new PotentialTarget(-Vector2.Distance(Position, World.GetTileCenter(x,y)), new Int2D(x,y)));
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

        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Reset();
        int i = Math.Min(targets.Count, 16);
        i = Math.Min(Random.Shared.Next(i), Random.Shared.Next(i));
        i = Utils.WeightedRandom(i);
        NavPath.Destination = targets[i].Value;
        // _navPath.Destination = targets[0].Value;
    }

    public virtual void Draw()
    {
        Z = Position.Y + (IsFlying ? 240 : 0);

        Vector2 pos = new Vector2((int)Position.X - Template.Texture.width / 2, (int)Position.Y - Template.Texture.width / 2);
        bool flip = Target.X > pos.X;
        Rectangle source = new Rectangle(flip ? Template.Texture.width : 0, 0, flip ? Template.Texture.width : -Template.Texture.width, Template.Texture.height);
        //Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        Raylib.DrawTextureRec(Template.Texture, source, pos, Team.UnitTint);
        
        // Debug, shows path
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, Template.PhysicsRadius))
        {
            Vector2 path = Position;
            foreach (Int2D i in NavPath.Waypoints)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
                path = v;
            }

            if (NavPath.Waypoints.Count == 0)
            {
                Vector2 v = World.GetTileCenter(NavPath.Destination);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
            }
        }
    }
    
    public virtual void Hurt(Projectile damageSource)
    {
        // guard against second bullet in same frame.
        if (Health <= 0) return;

        Health -= Math.Max(1, damageSource.Template.Damage - Template.Armor);
        if (Health <= 0)
        {
            Team.AddFearOf(Template.MaxHealth, World.PosToTilePos(Position));
            //Console.WriteLine($"{Template.Name} killed by {damageSource.Source.GetType().Name} which is " + (damageSource.Source is Structure ? "" : "not ") + "a structure, assigning hate...");
            if (damageSource.Source is Structure s)
            {
                //Console.WriteLine($"{Template.Name} killed by {s.Template.Name}, assigning hate...");
                Int2D pos = s.GetTilePos();
                Team.AddHateFor(Template.MaxHealth/10, pos.X, pos.Y);
            }
            // TODO: Add hate to origin structure when killed by minion
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

    public virtual void SetTarget(Int2D target)
    {
        NavPath.Reset();
        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Destination = target;
        PathFinder.RequestPath(NavPath);
    }
}
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

public class Minion : ISprite
{
    public MinionTemplate Template;
    public Team Team;
    protected NavPath NavPath;
    public Vector2 Position;
    public double Health;
    public bool IsAlive;
    private double _timeSinceLastAction;
    protected Vector2 NextPos;
    private bool _attacking;
    private double _attackStartedTime;
    // private double _lastFiredTime;
    private Vector2 _collisionOffset;
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
        Position = position;
        NextPos = position;
        //_targetTile = targetTile;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;
        _timeSinceLastAction = Time.Scaled;

        if (navPath != null)
        {
            NavPath = navPath;
        }
        else
        {
            NavPath = new NavPath(Team);
            Retarget();
            PathFinder.RequestPath(NavPath);
        }
        //_path = new PathFinder(this);
        //_path.FindPath(_targetTile);
        Z = Position.Y;
    }
    
    public virtual void Update()
    {
        UpdateNextPos();
        
        // if the next tile in our path is adjacent and solid, then attack it
        if (!TryAttack() && NavPath.Found)
        {
            // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
            if (NavPath.TargetReached(Position))
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
            // else, move towards next tile on path.
            Position = Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
        }
        
        Frenzy = false;
    }
    
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
    protected virtual bool TryAttack()
    {
        // Guard against out of range attacks
        if (Vector2.Distance(Position, NextPos) > 24 + Template.PhysicsRadius) { _attacking = false; return false;}
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position) != NavPath.Destination)) { _attacking = false; return false;}
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid(Team)) { _attacking = false; return false;}
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
    public double AdjustedSpeed()
    {
        double adjustedSpeed = Template.Speed;
        Structure? structure = World.GetTileAtPos(Position);
        if (Glued || (!IsFlying && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
        if (Frenzy) adjustedSpeed *= 2;
        return adjustedSpeed;
    }
    
    // Should this be here, or in World? maybe somewhere else entirely, like a physics functions class?
    public void CollideMinion(Minion other)
    {
        // if (other == this) 
        // {
        //     Console.WriteLine("Selfcolliding!");
        //     return;
        // }
        if (other.IsFlying != IsFlying) return;
        if (!Raylib.CheckCollisionCircles(Position, Template.PhysicsRadius, other.Position, other.Template.PhysicsRadius)) return;
        if (Position == other.Position) // jostle randomly if both minions are in the exact same position
        {
                  _collisionOffset += new Vector2((float)(World.Random.NextDouble() - 0.5), (float)(World.Random.NextDouble() - 0.5));
            other._collisionOffset += new Vector2((float)(World.Random.NextDouble() - 0.5), (float)(World.Random.NextDouble() - 0.5));
            return;
        }
        
        Vector2 delta = Position - other.Position;
        float weightRatio = Template.PhysicsRadius / (Template.PhysicsRadius + other.Template.PhysicsRadius);
        float penDepth = (Template.PhysicsRadius + other.Template.PhysicsRadius - delta.Length());
              _collisionOffset += delta.Normalized() * Math.Min(penDepth * (1f-weightRatio), 0.5f);
        other._collisionOffset -= delta.Normalized() * Math.Min(penDepth * (weightRatio),    0.5f);
    }
    
    public void CollideTerrain()
    {
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

        NavPath.Reset(Position);
        int i = Math.Min(targets.Count, 16);
        i = Math.Min(World.Random.Next(i), World.Random.Next(i));
        i = World.Random.WeightedRandom(i);
        NavPath.Destination = targets[i].Value;
        // _navPath.Destination = targets[0].Value;
    }

    public virtual void Draw()
    {
        Z = Position.Y + (IsFlying ? 240 : 0);

        Vector2 pos = new Vector2((int)Position.X - Template.Texture.Width / 2, (int)Position.Y - Template.Texture.Width / 2);
        bool flip = NextPos.X > pos.X;
        Rectangle source = new Rectangle(flip ? Template.Texture.Width : 0, 0, flip ? Template.Texture.Width : -Template.Texture.Width, Template.Texture.Height);
        //Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        Raylib.DrawTextureRec(Template.Texture, source, pos, Team.UnitTint);

        if (Health < Template.MaxHealth)
        {
            Vector2 start = Position - new Vector2(Template.Texture.Width / 2, Template.Texture.Height / 2 + 2);
            Vector2 end = start + new Vector2((float)(Template.Texture.Width * (Health / Template.MaxHealth)), 0);
            Raylib.DrawLineEx(start, end, 1, new Color(32, 192, 32, 255));
        }
        
        // Debug, shows path
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
    
    public virtual void Hurt(Projectile damageSource, double damage)
    {
        // guard against second bullet in same frame.
        if (Health <= 0) return;

        Health -= Math.Max(1, damage - Template.Armor);
        if (Health <= 0)
        {
            Team.AddFearOf(Template.MaxHealth/10, World.PosToTilePos(Position));
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
        NavPath.Reset(Position);
        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Destination = target;
        PathFinder.RequestPath(NavPath);
    }
}
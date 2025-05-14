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
    public Rigidbody Rigidbody;
    public Team Team;
    protected NavPath NavPath;
    // public Vector2 Position;
    public double Health;
    public bool IsAlive;
    private double _timeSinceLastAction;
    protected Vector2 NextPos; // This is the world space position the minion is currently trying to reach
    private bool _attacking;
    private double _attackStartedTime;
    private double _lastFiredTime;
    // private Vector2 _collisionOffset;
    public bool Glued;
    // public bool IsFlying;
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
        Rigidbody = World.CreateRigidbody(this, position, template.PhysicsRadius, false);
        Template = template;
        NextPos = position;
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
            NavPath = new NavPath(template.Name, Team);
            Retarget();
            PathFinder.RequestPath(NavPath);
        }
        //_path = new PathFinder(this);
        //_path.FindPath(_targetTile);
        Z = Rigidbody.Position.Y;
    }
    
    public virtual void Update()
    {
        UpdateNextPos();
        
        // if the next tile in our path is adjacent and solid, then attack it
        if (!TryAttack() && NavPath.Found)
        {
            // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
            if (NavPath.TargetReached(Rigidbody.Position))
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
            // else, move towards next tile on path.
            Rigidbody.Position = Rigidbody.Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
        }
        
        Frenzy = false;
    }
    
    protected void UpdateNextPos()
    {
        Vector2 pos = World.GetTileCenter(NavPath.NextTile(Rigidbody.Position));
        if (pos != NextPos)
        {
            NextPos = pos;
            _timeSinceLastAction = Time.Scaled;
            return;
        }
        
        if (Time.Scaled - _timeSinceLastAction > 5)
        {
            Console.WriteLine($"{Template.Name} got lost");
            NavPath.Reset(Rigidbody.Position);
            PathFinder.RequestPath(NavPath);
            _timeSinceLastAction = Time.Scaled;
        }
    }

    // Attempts to attack, returns true if attack target is valid 
    protected virtual bool TryAttack()
    {
        // Guard against out of range attacks
        if (Vector2.Distance(Rigidbody.Position, NextPos) > 24 + Template.PhysicsRadius) { _attacking = false; return false;}
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Rigidbody.Position) != NavPath.Destination)) { _attacking = false; return false;}
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid()) { _attacking = false; return false;}
        _timeSinceLastAction = Time.Scaled;
        if (!_attacking)
        {
            _attacking = true;
            _attackStartedTime = Time.Scaled;
        }
        if (Time.Scaled - _attackStartedTime >= (Frenzy ? Template.AttackCooldown/2 : Template.AttackCooldown))
        {
            Template.Projectile.Instantiate(t, this, Rigidbody.Position);
            _attackStartedTime = Time.Scaled;
        }
        return true;
    }
        
    // Returns move speed adjusted for glue, mine anxiety, etc.
    public double AdjustedSpeed()
    {
        double adjustedSpeed = Template.Speed;
        Structure? structure = World.GetTileAtPos(Rigidbody.Position);
        if (Glued || (!Rigidbody.IsFlying && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
        if (Frenzy) adjustedSpeed *= 2;
        return adjustedSpeed;
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
                    targets.Add(new Sortable<Int2D>(-Vector2.Distance(Rigidbody.Position, World.GetTileCenter(x,y)), new Int2D(x,y)));
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

        NavPath.Reset(Rigidbody.Position);
        int i = Math.Min(targets.Count, 16);
        i = Math.Min(World.Random.Next(i), World.Random.Next(i));
        i = World.Random.WeightedRandom(i);
        NavPath.Destination = targets[i].Value;
        // _navPath.Destination = targets[0].Value;
    }

    public virtual void Draw()
    {
        Z = Rigidbody.Position.Y + (Rigidbody.IsFlying ? 240 : 0);

        Vector2 pos = new Vector2((int)Rigidbody.Position.X - Template.Texture.Width / 2, (int)Rigidbody.Position.Y - Template.Texture.Width / 2);
        bool flip = NextPos.X > pos.X;
        Rectangle source = new Rectangle(flip ? Template.Texture.Width : 0, 0, flip ? Template.Texture.Width : -Template.Texture.Width, Template.Texture.Height);
        //Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        Raylib.DrawTextureRec(Template.Texture, source, pos, Team.UnitTint);

        if (Health < Template.MaxHealth)
        {
            Vector2 start = Rigidbody.Position - new Vector2(Template.Texture.Width / 2, Template.Texture.Height / 2 + 2);
            Vector2 end = start + new Vector2((float)(Template.Texture.Width * (Health / Template.MaxHealth)), 0);
            Raylib.DrawLineEx(start, end, 1, new Color(32, 192, 32, 255));
        }
        
        // Debug, shows path
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Rigidbody.Position, 2*Template.PhysicsRadius))
        {
            Raylib.DrawCircleV(NextPos, 2, Color.Green);
            
            Vector2 path = Rigidbody.Position;
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
            Team.AddFearOf(Template.MaxHealth/10, World.PosToTilePos(Rigidbody.Position));
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
        Rigidbody.Owner = null;
    }

    public Int2D GetTargetTile()
    {
        return NavPath.Destination;
    }

    public virtual void SetTarget(Int2D target)
    {
        NavPath.Reset(Rigidbody.Position);
        NavPath.Start = World.PosToTilePos(Rigidbody.Position);
        NavPath.Destination = target;
        PathFinder.RequestPath(NavPath);
    }
}
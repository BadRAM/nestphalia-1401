using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class MinionTemplate
{
    public string Name;
    public Texture Texture;
    public double MaxHealth;
    public double Armor;
    //public float Damage;
    public ProjectileTemplate Projectile;
    public double Range;
    public double RateOfFire;
    public double Speed;
    public bool IsFlying;
    public float PhysicsRadius; // This is a float because Raylib.CheckCircleOverlap() wants floats

    public MinionTemplate(string name, Texture texture, double maxHealth, double armor, ProjectileTemplate projectile, /*float damage,*/ double range, double rateOfFire, double speed, bool isFlying, float physicsRadius)
    {
        Name = name;
        Texture = texture;
        MaxHealth = maxHealth;
        Armor = armor;
        //Damage = damage;
        Projectile = projectile;
        Range = range;
        RateOfFire = rateOfFire;
        Speed = speed;
        IsFlying = isFlying;
        PhysicsRadius = physicsRadius;
    }

    public void Instantiate(Vector2 position, TeamName team, Path path)
    {
        World.Minions.Add(new Minion(this, position, team, path));
    }
}

public class Minion
{
    public MinionTemplate Template;
    public Vector2 Position;
    public double Health;
    public bool IsAlive;
    public TeamName Team;
    private Vector2 _target;
    //protected Int2D _targetTile;
    private double _lastFiredTime;
    public Vector2 CollisionOffset;
    private Path _path;

    public Minion(MinionTemplate template, Vector2 position, TeamName team, Path path)
    {
        Template = template;
        Position = position;
        _target = position;
        //_targetTile = targetTile;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;
        _path = path;
        //_path = new PathFinder(this);
        //_path.FindPath(_targetTile);
    }
    
    public virtual void Update()
    {
        // if the next tile in our path is adjacent and solid, then attack it
        
        // else, move towards next tile on path.
        
        // if we're at our final destination, ask for a new path.
        
        
        
        
        Structure? t = World.GetTileAtPos(Position.MoveTowards(_target, Template.Range)); // TODO: make this respect minion range
        if (Template.IsFlying && t != World.GetTile(_path.Destination)) t = null; // cheeky intercept so flyers ignore all but their target.

        if (t != null && t.IsSolid() && t.GetTeam() != Team)
        {
            if (Time.Scaled - _lastFiredTime > 60/Template.RateOfFire)
            {
                World.Projectiles.Add(new Projectile(Template.Projectile, Position, Position.MoveTowards(_target, Template.Range)));
                _lastFiredTime = Time.Scaled;
            }
        }
        else if (Template.IsFlying)
        {
            _target = World.GetTileCenter(_path.Destination);
            Position = Position.MoveTowards(_target, Template.Speed * Time.DeltaTime);
            if (Position == _target)
            {
                Retarget();
            }
        }
        else
        {
            if (_path.Found && _path.TargetReached(Position))
            {
                Retarget();
            }
            
            _target = World.GetTileCenter(_path.NextTile(Position));
            Position = Position.MoveTowards(_target, Template.Speed * Time.DeltaTime);
        }
    }
    
    // Should this be here, or in World? maybe somewhere else entirely, like a physics functions class?
    public void PlanCollision(int index)
    {
        for (int i = index+1; i < World.Minions.Count; i++)
        {
            // Abort loop if we're outside of our X band
            if (World.Minions[i].Position.X - Position.X > Template.PhysicsRadius + 12) break;
            if (World.Minions[i].Template.IsFlying != Template.IsFlying) continue;
            if (!Raylib.CheckCollisionCircles(Position, Template.PhysicsRadius, World.Minions[i].Position, World.Minions[i].Template.PhysicsRadius)) continue;
        
            Vector2 delta = Position - World.Minions[i].Position;
                             CollisionOffset += delta.Normalized() * Math.Min((Template.PhysicsRadius + World.Minions[i].Template.PhysicsRadius - delta.Length())/2, 0.5f);
            World.Minions[i].CollisionOffset -= delta.Normalized() * Math.Min((Template.PhysicsRadius + World.Minions[i].Template.PhysicsRadius - delta.Length())/2, 0.5f);
        }

        if (Template.IsFlying) return;
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
                    !(World.GetTile(x,y)?.IsSolid() ?? false) ||
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
                        CollisionOffset.Y = (float)desiredY - Position.Y;
                    }
                    else if (Position.Y > b.Y && Position.Y < b.Y + b.Height) // Circle Center is in tile Y band 
                    {
                        double desiredX = Position.X > c.X ? b.X + b.Width + Template.PhysicsRadius : b.X - Template.PhysicsRadius;
                        CollisionOffset.X = (float)desiredX - Position.X;
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
        Position += CollisionOffset;
        CollisionOffset = Vector2.Zero;
    }

    private void Retarget()
    {
        List<Int2D> targets = new List<Int2D>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                if (World.GetTile(x,y) != null && World.GetTile(x,y).Team != Team)
                {
                    targets.Add(new Int2D(x,y));
                }
            }
        }

        if (targets.Count == 0)
        {
            return;
        }

        _path.Found = false;
        _path.Waypoints.Clear();
        _path.Start = World.PosToTilePos(Position);
        _path.Destination = targets[Random.Shared.Next(targets.Count)];

        if (Template.IsFlying)
        {
            _path.Found = true;
        }
        else
        {
            PathFinder.RequestPath(_path);
        }
    }

    public virtual void Draw()
    {
        Color tint = Raylib.WHITE;
        if (Team == TeamName.Player) tint = Raylib.BLUE;
        if (Team == TeamName.Enemy) tint = Raylib.RED;
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        
        // Debug, shows path
        if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), Position, Template.PhysicsRadius))
        {
            Vector2 path = Position;
            foreach (Int2D i in _path.Waypoints)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
                path = v;
            }

            if (_path.Waypoints.Count == 0)
            {
                Vector2 v = World.GetTileCenter(_path.Destination);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
            }
        }
    }

    public virtual void Hurt(double damage)
    {
        Health -= Math.Max(1, damage - Template.Armor);
        if (Health <= 0)
        {
            World.MinionsToRemove.Add(this);
        }
    }
}
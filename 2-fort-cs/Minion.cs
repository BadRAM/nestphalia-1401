using System.Numerics;
using Raylib_cs;

namespace _2_fort_cs;

public class MinionTemplate
{
    public string Name;
    public Texture2D Texture;
    public float MaxHealth;
    public float Armor;
    //public float Damage;
    public ProjectileTemplate Projectile;
    public float Range;
    public float RateOfFire;
    public float Speed;
    public bool IsFlying;
    public float PhysicsRadius;

    public MinionTemplate(string name, Texture2D texture, float maxHealth, float armor, ProjectileTemplate projectile, /*float damage,*/ float range, float rateOfFire, float speed, bool isFlying, float physicsRadius)
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

    public void Instantiate(Vector2 position, TeamName team)
    {
        World.Minions.Add(new Minion(this, position, team));
    }
}

public class Minion
{
    public MinionTemplate Template;
    public Vector2 Position;
    public float Health;
    public bool IsAlive;
    public TeamName Team;
    private Vector2 _target;
    private float _lastFiredTime;
    private Vector2 _collisionOffset;
    private PathFinder _pathFinder;

    public Minion(MinionTemplate template, Vector2 position, TeamName team)
    {
        Template = template;
        Position = position;
        _target = position;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;
        _pathFinder = new PathFinder(this);
    }

    public virtual void Update()
    {
        Tile t = World.GetTileAtPos(Position.MoveTowards(_target, Template.Range));
        if (t.IsSolid() && t.GetTeam() != Team)
        {
            if (Raylib.GetTime() - _lastFiredTime > 60/Template.RateOfFire)
            {
                World.Projectiles.Add(new Projectile(Template.Projectile, Position, Position.MoveTowards(_target, Template.Range)));
                _lastFiredTime = (float)Raylib.GetTime();
            }
        }
        else
        {
            if (_pathFinder.TargetReached())
            {
                if (Team == TeamName.Player)
                {
                    _pathFinder.FindPath(new Int2D(Random.Shared.Next(27, 47), Random.Shared.Next(1, 21)));
                }
                else if (Team == TeamName.Enemy)
                {
                    _pathFinder.FindPath(new Int2D(Random.Shared.Next(1, 21), Random.Shared.Next(1, 21)));
                }
                else
                {
                    _pathFinder.FindPath(new Int2D(Random.Shared.Next(1, 47), Random.Shared.Next(1, 21)));
                }
            }
            
            _target = World.GetTileCenter(_pathFinder.NextTile());
            Position = Position.MoveTowards(_target, Template.Speed / 60f);
        }
        
        PlanCollision();

    }

    private void PlanCollision()
    {
        foreach (Minion m in World.Minions)
        {
            if (m == this) continue;
            if (m.Template.IsFlying != Template.IsFlying) continue;
            if (!Raylib.CheckCollisionCircles(Position, Template.PhysicsRadius, m.Position, m.Template.PhysicsRadius)) continue;
        
            Vector2 delta = Position - m.Position;
            _collisionOffset += delta.Normalized() * Math.Min((Template.PhysicsRadius + m.Template.PhysicsRadius - delta.Length())/2, 1f);
        }

        if (Template.IsFlying) return;
        Int2D tilePos = World.PosToTilePos(Position);
        
        for (int x = tilePos.X-1; x < tilePos.X+3; ++x)
        {
            for (int y = tilePos.Y-1; y < tilePos.Y+3; ++y)
            {
                // guard against out of bounds and non solid tiles
                if (x >= 0 && x < World.BoardWidth && y >= 0 && y < World.BoardHeight && !World.GetTile(x,y).IsSolid()) continue;
                Vector2 c = World.GetTileCenter(x, y);
                Rectangle b = World.GetTileBounds(x, y);
                if (!Raylib.CheckCollisionCircleRec(Position, Template.PhysicsRadius, b)) continue;
                {
                    if (Position.X > b.X && Position.X < b.X + b.Width) // circle center is in tile X band
                    {
                        // Find desired Y for above or below cases
                        float desiredY = Position.Y > c.Y ? b.Y + b.Height + Template.PhysicsRadius : b.Y - Template.PhysicsRadius;
                        _collisionOffset.Y = desiredY - Position.Y;
                    }
                    else if (Position.Y > b.Y && Position.Y < b.Y + b.Height) // Circle Center is in tile Y band 
                    {
                        float desiredX = Position.X > c.X ? b.X + b.Width + Template.PhysicsRadius : b.X - Template.PhysicsRadius;
                        _collisionOffset.X = desiredX - Position.X;
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

    public virtual void Draw()
    {
        Color tint = Color.White;
        if (Team == TeamName.Player) tint = Color.Blue;
        if (Team == TeamName.Enemy) tint = Color.Red;
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.Width/2, (int)Position.Y - Template.Texture.Width/2, tint);
        // Debug, shows path
        Vector2 path = Position;
        foreach (Int2D i in _pathFinder.Path)
        {
            Vector2 v = World.GetTileCenter(i);
            Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
            path = v;
        }
    }

    public virtual void Hurt(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            World.MinionsToRemove.Add(this);
        }
    }

    public void SetTarget(Vector2 target)
    {
        _target = target;
    }
}
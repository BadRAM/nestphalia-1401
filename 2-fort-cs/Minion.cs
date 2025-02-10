using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class MinionTemplate
{
    public string Name;
    public Texture Texture;
    public float MaxHealth;
    public float Armor;
    //public float Damage;
    public ProjectileTemplate Projectile;
    public float Range;
    public float RateOfFire;
    public float Speed;
    public bool IsFlying;
    public float PhysicsRadius;

    public MinionTemplate(string name, Texture texture, float maxHealth, float armor, ProjectileTemplate projectile, /*float damage,*/ float range, float rateOfFire, float speed, bool isFlying, float physicsRadius)
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

    public void Instantiate(Vector2 position, TeamName team, Int2D targetTile)
    {
        World.Minions.Add(new Minion(this, position, team, targetTile));
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
    protected Int2D _targetTile;
    private float _lastFiredTime;
    private Vector2 _collisionOffset;
    private PathFinder _pathFinder;

    public Minion(MinionTemplate template, Vector2 position, TeamName team, Int2D targetTile)
    {
        Template = template;
        Position = position;
        _target = position;
        _targetTile = targetTile;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;
        _pathFinder = new PathFinder(this);
        _pathFinder.FindPath(_targetTile);
    }

    public virtual void Update()
    {
        Structure? t = World.GetTileAtPos(Position.MoveTowards(_target, Template.Range)); // TODO: make this respect minion range
        if (Template.IsFlying && t != World.GetTile(_targetTile)) t = null; // cheeky intercept so flyers ignore all but their target.

        if (t != null && t.IsSolid() && t.GetTeam() != Team)
        {
            if (Raylib.GetTime() - _lastFiredTime > 60/Template.RateOfFire)
            {
                World.Projectiles.Add(new Projectile(Template.Projectile, Position, Position.MoveTowards(_target, Template.Range)));
                _lastFiredTime = (float)Raylib.GetTime();
            }
        }
        else if (Template.IsFlying)
        {
            _target = World.GetTileCenter(_targetTile);
            Position = Position.MoveTowards(_target, Template.Speed / 60f);
            if (Position == _target)
            {
                Retarget();
            }
        }
        else
        {
            if (_pathFinder.TargetReached())
            {
                Retarget();
            }
            
            _target = World.GetTileCenter(_pathFinder.NextTile());
            Position = Position.MoveTowards(_target, Template.Speed / 60f);
        }
        
        PlanCollision();
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

        _targetTile = targets[Random.Shared.Next(targets.Count)];
        _pathFinder.FindPath(_targetTile);
    }

    private void PlanCollision()
    {
        foreach (Minion m in World.Minions)
        {
            if (m == this) continue;
            if (m.Template.IsFlying != Template.IsFlying) continue;
            if (!Raylib.CheckCollisionCircles(Position, Template.PhysicsRadius, m.Position, m.Template.PhysicsRadius)) continue;
        
            Vector2 delta = Position - m.Position;
            _collisionOffset += delta.Normalized() * Math.Min((Template.PhysicsRadius + m.Template.PhysicsRadius - delta.Length())/2, 0.5f);
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
        Color tint = Raylib.WHITE;
        if (Team == TeamName.Player) tint = Raylib.BLUE;
        if (Team == TeamName.Enemy) tint = Raylib.RED;
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        
        // Debug, shows path
        if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), Position, Template.PhysicsRadius))
        {
            Vector2 path = Position;
            foreach (Int2D i in _pathFinder.Path)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
                path = v;
            }
        }
    }

    public virtual void Hurt(float damage)
    {
        Health -= Math.Max(1, damage - Template.Armor);
        if (Health <= 0)
        {
            World.MinionsToRemove.Add(this);
        }
    }
}
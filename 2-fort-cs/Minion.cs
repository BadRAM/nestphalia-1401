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

    public MinionTemplate(string name, Texture2D texture, float maxHealth, float armor, ProjectileTemplate projectile, /*float damage,*/ float range, float rateOfFire, float speed, bool isFlying)
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
    }

    public void Instantiate(Vector2 position)
    {
        World.Minions.Add(new Minion(this, position, 0));
    }
}

public class Minion
{
    public MinionTemplate Template;
    public Vector2 Position;
    public float Health;
    public bool IsAlive;
    public int Team;
    private Vector2 _target;
    private float _lastFiredTime;

    public Minion(MinionTemplate template, Vector2 position, int team)
    {
        Template = template;
        Position = position;
        _target = position;
        Team = team;
        Health = Template.MaxHealth;
        IsAlive = true;
    }

    public virtual void Update()
    {
        if (World.GetTileAtPos(Position.MoveTowards(_target, Template.Range)).IsSolid())
        {
            if (Raylib.GetTime() - _lastFiredTime > 60/Template.RateOfFire)
            {
                World.Projectiles.Add(new Projectile(Template.Projectile, Position, Position.MoveTowards(_target, Template.Range)));
                _lastFiredTime = (float)Raylib.GetTime();
            }
        }
        else
        {
            Position = Position.MoveTowards(_target, Template.Speed / 60f);
            if (Position == _target)
            {
                _target = new Vector2(Random.Shared.Next(10, 1000), Random.Shared.Next(10, 500));
            }
        }
    }

    public virtual void Draw()
    {
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.Width/2, (int)Position.Y - Template.Texture.Width/2, Color.White);
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
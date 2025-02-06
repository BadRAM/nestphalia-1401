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

    public Minion(MinionTemplate template, Vector2 position, TeamName team)
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
            Position = Position.MoveTowards(_target, Template.Speed / 60f);
            if (Position == _target)
            {
                if (Team == TeamName.Player)
                {
                    _target = new Vector2(Random.Shared.Next(576, 1152), Random.Shared.Next(8, 528));
                }
                else if (Team == TeamName.Enemy)
                {
                    _target = new Vector2(Random.Shared.Next(0, 576), Random.Shared.Next(8, 528));
                }
                else
                {
                    _target = new Vector2(Random.Shared.Next(0, 1152), Random.Shared.Next(8, 528));
                }
            }
        }
    }

    public virtual void Draw()
    {
        Color tint = Color.White;
        if (Team == TeamName.Player) tint = Color.Blue;
        if (Team == TeamName.Enemy) tint = Color.Red;
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.Width/2, (int)Position.Y - Template.Texture.Width/2, tint);
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
using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class ProjectileTemplate
{
    public Texture Texture;
    public double Damage;
    public double Speed;

    public ProjectileTemplate(Texture texture, double damage, double speed)
    {
        Damage = damage;
        Speed = speed;
        Texture = texture;
    }

    public virtual void Instantiate(object target, object source)
    {
        Vector2 pos = Vector2.Zero;
        if (source is Minion m) pos = m.Position;
        if (source is Structure s) pos = s.GetCenter();
        Projectile p = new Projectile(this, pos, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class Projectile : ISprite
{
    public ProjectileTemplate Template;
    public Vector2 Position;
    public Object Target;
    public Object Source;
    public double Z { get; set; }

    public Projectile(ProjectileTemplate template, Vector2 position, Object target, Object source)
    {
        Template = template;
        Position = position;
        Target = target;
        Source = source;
    }

    public virtual void Update()
    {
        if (Target is Minion minion)
        {
            Position = Position.MoveTowards(minion.Position, Template.Speed * Time.DeltaTime);
            if (Position == minion.Position)
            {
                minion.Hurt(this);
                World.ProjectilesToRemove.Add(this);
            }
        }
        else if (Target is Structure structure)
        {
            Position = Position.MoveTowards(structure.GetCenter(), Template.Speed * Time.DeltaTime);
            if (Position == structure.GetCenter())
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage);
                Destroy();
            }
        }
        else if (Target is Vector2 vec)
        {
            Position = Position.MoveTowards(vec, Template.Speed * Time.DeltaTime);
            if (Position == vec)
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage);
                Destroy();
            }
        }

        Z = Position.Y + 24;
    }

    public virtual void Destroy()
    {
        World.ProjectilesToRemove.Add(this);
    }

    public virtual void Draw()
    {
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, Raylib.WHITE);
    }
}
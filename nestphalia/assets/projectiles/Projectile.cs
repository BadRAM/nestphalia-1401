using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class ProjectileTemplate
{
    public string ID;
    public Texture2D Texture;
    public double Damage;
    public double Speed;

    public ProjectileTemplate(string id, Texture2D texture, double damage, double speed)
    {
        ID = id;
        Damage = damage;
        Speed = speed;
        Texture = texture;
    }

    public virtual void Instantiate(object target, object source, Vector2 position)
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
                minion.Hurt(Template.Damage*1.5 - Template.Damage*World.Random.NextDouble(), this);
                World.ProjectilesToRemove.Add(this);
            }
        }
        else if (Target is Structure structure)
        {
            Position = Position.MoveTowards(structure.GetCenter(), Template.Speed * Time.DeltaTime);
            if (Position == structure.GetCenter())
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage*1.5 - Template.Damage*World.Random.NextDouble());
                Destroy();
            }
        }
        else if (Target is Vector2 vec)
        {
            Position = Position.MoveTowards(vec, Template.Speed * Time.DeltaTime);
            if (Position == vec)
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage*1.5 - Template.Damage*World.Random.NextDouble());
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
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.Width/2, (int)Position.Y - Template.Texture.Width/2, Color.White);
    }
}
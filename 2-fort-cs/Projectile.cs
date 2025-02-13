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
}

public class Projectile
{
    public ProjectileTemplate Template;
    public Vector2 Position;
    public bool MinionTargetted; // if true, go towards minion, if false go towards pos
    public Minion? TargetMinion;
    public Vector2 TargetPos;
    public bool Destroy;

    public Projectile(ProjectileTemplate template, Vector2 position, Minion targetMinion)
    {
        Template = template;
        Position = position;
        TargetMinion = targetMinion;
        MinionTargetted = true;
    }

    public Projectile(ProjectileTemplate template, Vector2 position, Vector2 target)
    {
        Template = template;
        Position = position;
        TargetPos = target;
        MinionTargetted = false;
        
    }

    public virtual void Update()
    {
        if (MinionTargetted)
        {
            Position = Position.MoveTowards(TargetMinion.Position, Template.Speed * Time.DeltaTime);
            if (Position == TargetMinion.Position)
            {
                TargetMinion.Hurt(Template.Damage);
                World.ProjectilesToRemove.Add(this);
            }
        }
        else
        {
            Position = Position.MoveTowards(TargetPos, Template.Speed * Time.DeltaTime);
            if (Position == TargetPos)
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage);
                World.ProjectilesToRemove.Add(this);
            }
        }
    }

    public virtual void Draw()
    {
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, Raylib.WHITE);
    }
}
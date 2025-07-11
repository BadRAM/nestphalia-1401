using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ProjectileTemplate : JsonAsset
{
    [JsonProperty(Order = -3)]
    public Texture2D Texture;
    public double Damage;
    public double Speed;

    // public ProjectileTemplate(string id, Texture2D texture, double damage, double speed) : base(new JObject())
    // {
    //     ID = id;
    //     Damage = damage;
    //     Speed = speed;
    //     Texture = texture;
    // }
    
    public ProjectileTemplate(JObject jObject) : base(jObject)
    {
        Damage = jObject.Value<double?>("Damage") ?? 0;
        Speed = jObject.Value<double?>("Speed") ?? 0;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }

    public virtual void Instantiate(object target, object source, Vector3 position)
    {
        Projectile p = new Projectile(this, position, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class Projectile : ISprite
{
    public ProjectileTemplate Template;
    public Vector3 Position;
    public Object Target;
    public Object Source;

    public Projectile(ProjectileTemplate template, Vector3 position, Object target, Object source)
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
                minion.Hurt(Template.Damage*1.5 - Template.Damage*World.RandomDouble(), this);
                World.ProjectilesToRemove.Add(this);
            }
        }
        else if (Target is Structure structure)
        {
            Position = Position.MoveTowards(structure.GetCenter().XYZ(), Template.Speed * Time.DeltaTime);
            if (Position == structure.GetCenter().XYZ())
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage*1.5 - Template.Damage*World.RandomDouble());
                Destroy();
            }
        }
        else if (Target is Vector3 vec)
        {
            Position = Position.MoveTowards(vec, Template.Speed * Time.DeltaTime);
            if (Position == vec)
            {
                World.GetTileAtPos(Position)?.Hurt(Template.Damage*1.5 - Template.Damage*World.RandomDouble());
                Destroy();
            }
        }
    }

    public virtual void Destroy()
    {
        World.ProjectilesToRemove.Add(this);
    }

    public virtual void Draw()
    {
        Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.Width/2, (int)(Position.Y - Template.Texture.Width/2 - Position.Z), Color.White);
    }

    public virtual double GetDrawOrder()
    {
        return Position.Y;
    }
}
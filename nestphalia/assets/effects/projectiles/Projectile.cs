using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ProjectileTemplate : EffectTemplate
{
    public Texture2D Texture;
    public double Damage;
    public double Speed;
    
    public ProjectileTemplate(JObject jObject) : base(jObject)
    {
        Damage = jObject.Value<double?>("Damage") ?? 0;
        Speed = jObject.Value<double?>("Speed") ?? 0;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }

    public virtual void Instantiate(object target, object source, Vector3 position)
    {
        Projectile p = new Projectile(this, position, target, source);
        World.Effects.Add(p);
        World.Sprites.Add(p);
    }
}

public class Projectile : Effect
{
    private ProjectileTemplate _template;
    public Object Target;
    public Object Source;

    public Projectile(ProjectileTemplate template, Vector3 position, Object target, Object source) : base(template, position)
    {
        _template = template;
        Target = target;
        Source = source;
    }

    public override void Update()
    {
        if (Target is Minion minion)
        {
            Position = Position.MoveTowards(minion.Position, _template.Speed * Time.DeltaTime);
            if (Position == minion.Position)
            {
                minion.Hurt(_template.Damage*1.5 - _template.Damage*World.RandomDouble(), this);
                World.EffectsToRemove.Add(this);
            }
        }
        else if (Target is Structure structure)
        {
            Position = Position.MoveTowards(structure.GetCenter().XYZ(), _template.Speed * Time.DeltaTime);
            if (Position == structure.GetCenter().XYZ())
            {
                World.GetTileAtPos(Position)?.Hurt(_template.Damage*1.5 - _template.Damage*World.RandomDouble());
                Destroy();
            }
        }
        else if (Target is Vector3 vec)
        {
            Position = Position.MoveTowards(vec, _template.Speed * Time.DeltaTime);
            if (Position == vec)
            {
                World.GetTileAtPos(Position)?.Hurt(_template.Damage*1.5 - _template.Damage*World.RandomDouble());
                Destroy();
            }
        }
    }

    public override void Draw()
    {
        Raylib.DrawTexture(_template.Texture, (int)Position.X - _template.Texture.Width/2, (int)(Position.Y - _template.Texture.Width/2 - Position.Z), Color.White);
    }
}
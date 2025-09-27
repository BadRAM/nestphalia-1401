using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ProjectileTemplate : AttackTemplate
{
    public Texture2D Texture;
    public double Speed;
    
    public ProjectileTemplate(JObject jObject) : base(jObject)
    {
        Speed = jObject.Value<double?>("Speed") ?? 0;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }
    
    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new Projectile(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new Projectile(this, position, target, source));
    }
}

public class Projectile : Attack
{
    private ProjectileTemplate _template;

    public Projectile(ProjectileTemplate template, Vector3 position, IMortal targetEntity, IMortal source) : base(template, position, targetEntity, source)
    {
        _template = template;
    }
    
    public Projectile(ProjectileTemplate template, Vector3 position, Vector3 targetPos, IMortal source) : base(template, position, targetPos, source)
    {
        _template = template;
    }

    public override void Update()
    {
        if (TargetEntity != null)
        {
            TargetPos = TargetEntity.GetPos();
        }
        
        Position = Position.MoveTowards(TargetPos, _template.Speed * Time.DeltaTime);
        if (Position == TargetPos)
        {
            TargetEntity?.Hurt(GetDamageModified(), this);
            World.EffectsToRemove.Add(this);
        }
    }

    public override void Draw()
    {
        Raylib.DrawTexture(_template.Texture, (int)Position.X - _template.Texture.Width/2, (int)(Position.Y - _template.Texture.Width/2 - Position.Z), Color.White);
    }
}
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ProjectileTemplate : AttackTemplate
{
    public double Speed;
    public bool FaceForward;
    public Texture2D Texture;
    public SubAsset<AttackTemplate>? HitEffect;
    
    public ProjectileTemplate(JObject jObject) : base(jObject)
    {
        Speed = jObject.Value<double?>("Speed") ?? throw new ArgumentNullException();
        FaceForward = jObject.Value<bool?>("FaceForward") ?? false;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        if (jObject.ContainsKey("HitEffect")) HitEffect = new SubAsset<AttackTemplate>(jObject.GetValue("HitEffect")!);
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
            if (_template.HitEffect != null)
            {
                if (TargetEntity != null)
                {
                    _template.HitEffect.Asset.Instantiate(TargetEntity, Source, Position);
                }
                else
                {
                    _template.HitEffect.Asset.Instantiate(TargetPos, Source, Position);
                }
            }
            TargetEntity?.Hurt(GetDamageModified(), this);
            World.EffectsToRemove.Add(this);
        }
    }

    public override void Draw()
    {
        float rot = _template.FaceForward ? (float)((TargetPos - Position).XYZ2D().Angle() * (180/Math.PI)) : 0;
        Raylib.DrawTexturePro(
            _template.Texture, 
            _template.Texture.Rect(), 
            new Rectangle(Position.XYZ2D(), 
            _template.Texture.Size()), 
            _template.Texture.Size()/2, 
            rot, 
            Color.White);
    }
}
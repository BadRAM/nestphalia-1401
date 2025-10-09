using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ArcProjectileTemplate : AttackTemplate
{
    public double ArcDuration;
    public double ArcHeight;
    public bool FaceForward;
    public Texture2D Texture;
    public SubAsset<AttackTemplate>? HitEffect;
        
    public ArcProjectileTemplate(JObject jObject) : base(jObject)
    {
        ArcDuration = jObject.Value<double?>("ArcDuration") ?? throw new ArgumentNullException();
        ArcHeight = jObject.Value<double?>("ArcHeight") ?? throw new ArgumentNullException();
        FaceForward = jObject.Value<bool?>("FaceForward") ?? false;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        if (jObject.ContainsKey("HitEffect")) HitEffect = new SubAsset<AttackTemplate>(jObject.GetValue("HitEffect")!);
    }
    
    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new ArcProjectile(this, position, target, source));
    }
    
    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new ArcProjectile(this, position, target, source));
    }
}

public class ArcProjectile : Attack
{
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private Vector3 _lastPos;
    private double _timeFired;
    private ArcProjectileTemplate _template;
    
    public ArcProjectile(ArcProjectileTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    { 
        _template = template;
        Init();
    }
    
    public ArcProjectile(ArcProjectileTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {

        _template = template;
        Init();
    }

    private void Init()
    {
        _startPos = Position;
        _targetPos = TargetPos;
        _timeFired = Time.Scaled;
    }
    
    public override void Update()
    {
        _lastPos = Position;
        
        double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
        double arcOffset = Math.Sin(t * Math.PI) * _template.ArcHeight;
        
        Position = Vector3.Lerp(_startPos, _targetPos, (float)t);
        Position.Z += (float)arcOffset;

        if (t >= 1)
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
            TargetEntity?.Hurt(_template.Damage);
            World.EffectsToRemove.Add(this);
        }
    }

    public override void Draw()
    {
        float rot = _template.FaceForward ? (float)((Position - _lastPos).XYZ2D().Angle() * (180/Math.PI)) : 0;
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
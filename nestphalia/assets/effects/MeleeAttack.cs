using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MeleeAttackTemplate : ProjectileTemplate
{
    public MeleeAttackTemplate(JObject jObject) : base(jObject)
    {
    }

    public override void Instantiate(object target, object source, Vector3 position)
    {
        Projectile p = new MeleeAttack(this, position, target, source);
        World.Effects.Add(p);
        World.Sprites.Add(p);
    }
}

public class MeleeAttack : Projectile
{
    private MeleeAttackTemplate _template;
    private double _duration = 0.8;
    private double _startTime;
    private Rectangle _particleSrc = new Rectangle(0, 22, 2, 2);
    private Texture2D _texture;
    
    
    public MeleeAttack(MeleeAttackTemplate template, Vector3 position, object target, object source) : base(template, position, target, source)
    {
        _template = template;
        
        if (target is Minion minion)
        {
            minion.Hurt(_template.Damage*1.5 - _template.Damage*World.RandomDouble(), this);
            Position = minion.Position;
        }
        if (target is Structure structure)
        {
            structure.Hurt(_template.Damage*1.5 - _template.Damage*World.RandomDouble());
            Position = structure.GetCenter().XYZ();
            Position += new Vector3(-10, -10, 0);
            Position += new Vector3(Random.Shared.Next(20), Random.Shared.Next(20), 0);
            _texture = structure.Template.Texture;
            _particleSrc.Width = (float)Math.Sqrt(_template.Damage);
            _particleSrc.Height = _particleSrc.Width;
            _particleSrc = new Rectangle(Random.Shared.Next(24 - (int)_particleSrc.Width), Random.Shared.Next(24 - (int)_particleSrc.Height) + _texture.Height - 22, 2, 2);
        }

        _startTime = Time.Scaled;
    }

    public override void Update()
    {
        float t = (float)((Time.Scaled - _startTime) / _duration);
        Position.Z = (float)(Easings.Bounce(t) * 12);
        if (t >= 1)
        {
            Destroy();
        }
    }

    public override void Draw()
    {
        Raylib.DrawTextureRec(_texture, _particleSrc, Position.XYZ2D() - Vector2.One, Color.Gray);
        // Raylib.DrawCircleV(Position.XYZ2D(), 2, Color.Brown);
    }
}
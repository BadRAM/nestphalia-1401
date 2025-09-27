using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MeleeAttackTemplate(JObject jObject) : AttackTemplate(jObject)
{
    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new MeleeAttack(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new MeleeAttack(this, position, target, source));
    }
}

public class MeleeAttack : Attack
{
    private MeleeAttackTemplate _template;
    private double _duration = 0.8;
    private double _startTime;
    private Vector2 _move;
    private Rectangle _particleSrc = new Rectangle(0, 22, 2, 2);
    private Texture2D _texture;
    
    public MeleeAttack(MeleeAttackTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    {
        targetEntity.Hurt(GetDamageModified(), this);
        _template = template;
        
        if (targetEntity is Structure structure && source is Minion srcMinion)
        {
            Position = Vector3.Lerp(srcMinion.Position, structure.Position, Random.Shared.NextSingle());
            _texture = structure.Template.Texture;
            _particleSrc.Width = (float)Math.Sqrt(_template.Damage);
            _particleSrc.Height = _particleSrc.Width;
            _particleSrc = new Rectangle(Random.Shared.Next(24 - (int)_particleSrc.Width), Random.Shared.Next(24 - (int)_particleSrc.Height) + _texture.Height - 22, 2, 2);
        }
        
        _startTime = Time.Scaled;
        _move = Random.Shared.UnitCircle() * 8;
    }
    
    public MeleeAttack(MeleeAttackTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {
        _template = template;
        _duration = 0;
        _startTime = Time.Scaled;
        GameConsole.WriteLine($"Something tried to meleeAttack the ground.");
    }

    public override void Update()
    {
        float t = (float)((Time.Scaled - _startTime) / _duration);
        Position += (_move * (float)(Time.DeltaTime / _duration)).XYZ();
        Position.Z = (float)(Easings.FullBounce(t) * 12);
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
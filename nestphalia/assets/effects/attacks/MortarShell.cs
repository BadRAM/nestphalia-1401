using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MortarShellTemplate : AttackTemplate
{
    public double ArcDuration;
    public double ArcHeight;
    public float BlastRadius;
    public bool DamageBuildings;
    public Texture2D Texture;
        
    public MortarShellTemplate(JObject jObject) : base(jObject)
    {
        ArcDuration = jObject.Value<double?>("ArcDuration") ?? throw new ArgumentNullException();
        ArcHeight = jObject.Value<double?>("ArcHeight") ?? throw new ArgumentNullException();
        BlastRadius = jObject.Value<float?>("BlastRadius") ?? 0;
        DamageBuildings = jObject.Value<bool?>("DamageBuildings") ?? false;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }

    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new MortarShell(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new MortarShell(this, position, target, source));
    }
}

public class MortarShell : Attack
{
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private double _timeFired;
    private MortarShellTemplate _template;
    private Team? _team;
    private Texture2D _explosion;
    private double _timeExploded = 0;
    private SoundResource _soundEffect;
    
    public MortarShell(MortarShellTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    { 
        _template = template;
        _soundEffect = Resources.GetSoundByName("explosion");
        Init();
    }
    
    public MortarShell(MortarShellTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {

        _template = template;
        _soundEffect = Resources.GetSoundByName("explosion");
        Init();
    }

    private void Init()
    {
        _team = Source?.Team;
        _startPos = Position;
        _targetPos = TargetPos;
        _timeFired = Time.Scaled;
        _explosion = Resources.GetTextureByName("explosion32");
    }
    
    public override void Update()
    {
        if (_timeExploded != 0)
        {
            if (Time.Scaled - _timeExploded > 1)
            {
                World.EffectsToRemove.Add(this);
            }
            return;
        }
        
        double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
        double arcOffset = Math.Sin(t * Math.PI) * _template.ArcHeight;
        
        Position = Vector3.Lerp(_startPos, _targetPos, (float)t);
        Position.Z += (float)arcOffset;

        if (t >= 1)
        {
            _soundEffect.PlayRandomPitch(SoundResource.WorldToPan(Position.X));
            if (_template.BlastRadius > 0) Explode();
            else TargetEntity?.Hurt(_template.Damage);
            _timeExploded = Time.Scaled;
        }
    }

    private void Explode()
    {
        foreach (Minion minion in World.GetMinionsInRegion(World.PosToTilePos(_targetPos), (int)(1 + _template.BlastRadius / 24)))
        {
            if (minion.Team == _team) continue;
            float dist = Vector3.Distance(_targetPos, minion.Position);
            if (dist < _template.BlastRadius)
            {
                double falloff = Math.Clamp(2 * (_template.BlastRadius - dist) / _template.BlastRadius, 0, 1);
                minion.Hurt(_template.Damage * falloff, this);
                // Knockback if max damage dealt
                if (falloff >= 0.99)
                {
                    double blastFactor = 3 * _template.Damage * (_template.BlastRadius - dist / _template.BlastRadius) / (minion.Template.MaxHealth + 40) ;
                    if (blastFactor > 12)
                    {
                        Vector2 target = minion.Position.XY() + (minion.Position.XY() - Position.XY()).Normalized() * (float)blastFactor;
                        minion.SetState(new Minion.Jump(minion, target, 0, blastFactor / 80, 0, blastFactor/2));
                        // GameConsole.WriteLine($"Launched a {minion.Template.ID} with force {blastFactor:N3}");
                    }
                }
            }
        }

        if (_template.DamageBuildings)
        {
            Int2D center = World.PosToTilePos(Position);
            int radius = (int)(_template.BlastRadius / 24) + 2;
            List<Minion> minions = new List<Minion>();
            for (int x = Math.Max(center.X - radius, 0); x <= Math.Min(center.X + radius, World.BoardHeight-1); x++)
            for (int y = Math.Max(center.Y - radius, 0); y <= Math.Min(center.Y + radius, World.BoardHeight-1); y++)
            {
                double dist = Vector2.Distance(World.GetTileCenter(x, y), Position.XY());
                if (dist > _template.BlastRadius) continue;
                Structure? struc = World.GetTile(x, y);
                if (struc == null || struc.Team == _team) continue;
                
                double falloff = Math.Clamp(2 * (_template.BlastRadius - dist) / _template.BlastRadius, 0, 1);
                struc.Hurt(_template.Damage * falloff, this);
            }
        }
    }

    public override void Draw()
    {
        if (_timeExploded == 0)
        {
            Raylib.DrawTexture(_template.Texture, (int)Position.X - _template.Texture.Width/2, (int)(Position.Y - _template.Texture.Width/2 - Position.Z), Color.White);
        }
        else
        {
            int frame = (int)Math.Floor((Time.Scaled - _timeExploded) / 0.1);
            Rectangle src = new Rectangle(frame * 32, 0, 32, 32);
            if (src.X >= _explosion.Width)
            {
                return;
            }
            Vector2 pos = Position.XYZ2D() - new Vector2(16, 16);
            Raylib.DrawTextureRec(_explosion, src, pos, Color.White);
        }
    }
}
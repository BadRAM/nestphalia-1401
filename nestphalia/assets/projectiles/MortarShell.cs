using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MortarShellTemplate : ProjectileTemplate
{
    public double ArcDuration;
    public double ArcHeight;
    public float BlastRadius;
        
    public MortarShellTemplate(JObject jObject) : base(jObject)
    {
        ArcDuration = jObject.Value<double?>("arcDuration") ?? throw new ArgumentNullException();
        ArcHeight = jObject.Value<double?>("arcHeight") ?? throw new ArgumentNullException();
        BlastRadius = jObject.Value<float?>("blastRadius") ?? 0;
    }

    public override void Instantiate(object target, object source, Vector3 position)
    {
        MortarShell p = new MortarShell(this, position, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class MortarShell : Projectile
{
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private double _timeFired;
    private MortarShellTemplate _template;
    private Team _team;
    private Texture2D _explosion;
    private double _timeExploded = 0;
    private SoundResource _soundEffect;
    
    public MortarShell(MortarShellTemplate template, Vector3 position, object target, object source) : base(template, position, target, source)
    {
        _template = template;
        _timeFired = Time.Scaled;
        _startPos = position;
        _explosion = Resources.GetTextureByName("explosion32");
        _soundEffect = Resources.GetSoundByName("explosion");
        if (target is Minion minion)
        {
            _targetPos = minion.Position;
        }
        if (target is Structure structure)
        {
            _targetPos = structure.GetCenter().XYZ();
        }
        if (target is Vector3 vec)
        {
            _targetPos = vec;
        }
        if (source is Structure sourceStructure)
        {
            _team = sourceStructure.Team;
        }
    }
    
    public override void Update()
    {
        if (_timeExploded != 0)
        {
            if (Time.Scaled - _timeExploded > 1)
            {
                World.ProjectilesToRemove.Add(this);
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
            if (Target is Structure s)
            {
                s.Hurt(_template.Damage);
            }
            else
            {
                foreach (Minion minion in World.GetMinionsInRegion(World.PosToTilePos(_targetPos), (int)(1 + _template.BlastRadius / 24)))
                {
                    if (minion.Team == _team) continue;
                    float dist = Vector3.Distance(_targetPos, minion.Position);
                    if (dist < _template.BlastRadius)
                    {
                        double falloff = Math.Clamp(2 * (_template.BlastRadius - dist) / _template.BlastRadius, 0, 1);
                        minion.Hurt(_template.Damage * falloff, this);
                    }
                }
            }
            _timeExploded = Time.Scaled;
        }
    }

    public override void Draw()
    {
        if (_timeExploded == 0)
        {
            base.Draw();
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
        
        // //Raylib.DrawLine(_startPos.X,);
        // Raylib.DrawLineEx(_startPos, _targetPos, 1, Raylib.RED);
        //
        // double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
        //
        // Vector2 groundPoint = Vector2.Lerp(_startPos, _targetPos, (float)t);
        // Raylib.DrawLineEx(groundPoint, Position, 1, Raylib.GREEN);
    }
}
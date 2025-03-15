using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class MortarShellTemplate : ProjectileTemplate
{
    public double ArcDuration;
    public double ArcHeight;
    public float BlastRadius;
        
    public MortarShellTemplate(Texture2D texture, double damage, double arcDuration, double arcHeight, float blastRadius) : base(texture, damage, 0)
    {
        ArcDuration = arcDuration;
        ArcHeight = arcHeight;
        BlastRadius = blastRadius;
    }

    public override void Instantiate(object target, object source, Vector2 position)
    {
        MortarShell p = new MortarShell(this, position, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class MortarShell : Projectile
{
    private Vector2 _startPos;
    private Vector2 _targetPos;
    private double _timeFired;
    private MortarShellTemplate _template;
    private Team _team;
    private Texture2D _explosion;
    private double _timeExploded = 0;
    private SoundResource _soundEffect;
    
    public MortarShell(MortarShellTemplate template, Vector2 position, object target, object source) : base(template, position, target, source)
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
            _targetPos = structure.GetCenter();
        }
        if (target is Vector2 vec)
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
        
        Position = Vector2.Lerp(_startPos, _targetPos, (float)t);
        Z = Position.Y;
        Position.Y -= (float)arcOffset;

        if (t >= 1)
        {
            _soundEffect.PlayRandomPitch(SoundResource.WorldToPan(Position.X));
            if (Target is Structure s)
            {
                s.Hurt(_template.Damage);
            }
            else
            {
                for (int index = 0; index < World.Minions.Count; index++)
                {
                    Minion minion = World.Minions[index];
                    if (!minion.IsFlying && minion.Team != _team &&
                        Vector2.Distance(_targetPos, minion.Position) < _template.BlastRadius)
                    {
                        double damage = Math.Clamp(2 * (_template.BlastRadius - Vector2.Distance(_targetPos, minion.Position)) / _template.BlastRadius, 0, 1) * _template.Damage;
                        minion.Hurt(this, damage);
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
            Vector2 pos = Position - new Vector2(16, 16);
            Raylib.DrawTextureRec(_explosion, src, pos, Color.White);
        }
        
        // base.Draw();
        // //Raylib.DrawLine(_startPos.X,);
        // Raylib.DrawLineEx(_startPos, _targetPos, 1, Raylib.RED);
        //
        // double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
        //
        // Vector2 groundPoint = Vector2.Lerp(_startPos, _targetPos, (float)t);
        // Raylib.DrawLineEx(groundPoint, Position, 1, Raylib.GREEN);
    }
}
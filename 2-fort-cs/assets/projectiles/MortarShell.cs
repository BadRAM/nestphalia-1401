using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class MortarShellTemplate : ProjectileTemplate
{
    public double ArcDuration;
    public double ArcHeight;
    public float BlastRadius;
        
    public MortarShellTemplate(Texture texture, double damage, double arcDuration, double arcHeight, float blastRadius) : base(texture, damage, 0)
    {
        ArcDuration = arcDuration;
        ArcHeight = arcHeight;
        BlastRadius = blastRadius;
    }

    public override void Instantiate(object target, object source)
    {
        Vector2 pos = Vector2.Zero;
        if (source is Minion m) pos = m.Position;
        if (source is Structure s) pos = s.GetCenter();
        MortarShell p = new MortarShell(this, pos, target, source);
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
    private Texture _explosion;
    private double _timeExploded = 0;
    
    public MortarShell(MortarShellTemplate template, Vector2 position, object target, object source) : base(template, position, target, source)
    {
        _template = template;
        _timeFired = Time.Scaled;
        _startPos = position;
        _explosion = Resources.GetTextureByName("explosion32");
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
                        Raylib.CheckCollisionCircles(_targetPos, _template.BlastRadius, minion.Position,
                            minion.Template.PhysicsRadius))
                    {
                        minion.Hurt(this);
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
            if (src.X >= _explosion.width)
            {
                return;
            }
            Vector2 pos = Position - new Vector2(16, 16);
            Raylib.DrawTextureRec(_explosion, src, pos, Raylib.WHITE);
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
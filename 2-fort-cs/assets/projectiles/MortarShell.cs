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
    
    public MortarShell(MortarShellTemplate template, Vector2 position, object target, object source) : base(template, position, target, source)
    {
        _template = template;
        _timeFired = Time.Scaled;
        _startPos = position;
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
        double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
        double arcOffset = Math.Sin(t * Math.PI) * _template.ArcHeight;
        
        Position = Vector2.Lerp(_startPos, _targetPos, (float)t);
        Z = Position.Y;
        Position.Y -= (float)arcOffset;

        if (t >= 1)
        {
            foreach (Minion minion in World.Minions)
            {
                if (!minion.Template.IsFlying && minion.Team != _team &&
                    Raylib.CheckCollisionCircles(_targetPos, _template.BlastRadius, minion.Position, minion.Template.PhysicsRadius))
                {
                    minion.Hurt(this);
                }
            }
            World.ProjectilesToRemove.Add(this);
        }
    }

    // public override void Draw()
    // {
    //     base.Draw();
    //     //Raylib.DrawLine(_startPos.X,);
    //     Raylib.DrawLineEx(_startPos, _targetPos, 1, Raylib.RED);
    //     
    //     double t = (Time.Scaled - _timeFired) / _template.ArcDuration;
    //
    //     Vector2 groundPoint = Vector2.Lerp(_startPos, _targetPos, (float)t);
    //     Raylib.DrawLineEx(groundPoint, Position, 1, Raylib.GREEN);
    // }
}
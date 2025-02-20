using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class MinefieldTemplate : StructureTemplate
{
    public int MaxCharges;
    public ProjectileTemplate Bomb;
    public double Range;
    public double Cooldown;
    
    public MinefieldTemplate(string name, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, int maxCharges, ProjectileTemplate bomb, double range, double cooldown) : base(name, texture, maxHealth, price, levelRequirement, baseHate)
    {
        MaxCharges = maxCharges;
        Bomb = bomb;
        Range = range;
        Cooldown = cooldown;
        Class = StructureClass.Tower;
    }

    public override Minefield Instantiate(Team team, int x, int y)
    {
        return new Minefield(this, team, x, y);
    }
}


public class Minefield : Structure
{
    private MinefieldTemplate _template;
    private int _chargesLeft;
    private double _timeLastTriggered;
    private Vector2[] _drawPoints = new []{new Vector2(6,6), new Vector2(-6,-6), new Vector2(-6,6), new Vector2(6,-6)};
    
    public Minefield(MinefieldTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _chargesLeft = template.MaxCharges;
        for (int i = 0; i < _drawPoints.Length; i++)
        {
            _drawPoints[i] += new Vector2(Random.Shared.Next(-2, 2), Random.Shared.Next(-2, 2));
        }

        Z = position.Y - 24;
    }

    public override void Update()
    {
        base.Update();
        
        if (Time.Scaled - _timeLastTriggered < _template.Cooldown) return;
        foreach (Minion minion in World.Minions)
        {
            if (minion.Team != Team && !minion.Template.IsFlying &&
                Raylib.CheckCollisionCircles(
                    position, (float)_template.Range, 
                    minion.Position,minion.Template.PhysicsRadius))
            {
                _chargesLeft--;
                position += _drawPoints[_chargesLeft];
                _template.Bomb.Instantiate(position, this);
                position = World.GetTileCenter(X, Y);
                _timeLastTriggered = Time.Scaled;
                if (_chargesLeft <= 0) Destroy();
                Health = Math.Max(_chargesLeft, Health);
                break;
            }
        }
    }
    
    public override void Draw()
    {
        for (int i = 0; i < Math.Min(_chargesLeft, _drawPoints.Length); i++)
        {
            int x = (int)(_drawPoints[i].X + position.X - _template.Bomb.Texture.width/2);
            int y = (int)(_drawPoints[i].Y + position.Y - _template.Bomb.Texture.height/2);
            Raylib.DrawTexture(_template.Bomb.Texture, x, y, Raylib.WHITE);
        }
    }

    public override bool NavSolid(Team team)
    {
        return team == Team;
    }

    public override bool PhysSolid(Team team)
    {
        return false;
    }
}
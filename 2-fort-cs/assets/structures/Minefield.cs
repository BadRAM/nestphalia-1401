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
    
    public Minefield(MinefieldTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _chargesLeft = template.MaxCharges;
    }

    public override void Update()
    {
        base.Update();
        
        if (Time.Scaled - _timeLastTriggered < _template.Cooldown) return;
        foreach (Minion minion in World.Minions)
        {
            if (minion.Team != Team && 
                Raylib.CheckCollisionCircles(
                    position, (float)_template.Range, 
                    minion.Position,minion.Template.PhysicsRadius))
            {
                _template.Bomb.Instantiate(position, this);
                _chargesLeft--;
                Health = Math.Max(_chargesLeft, Health);
                break;
            }
        }
    }
    
    public override bool IsSolid()
    {
        return false;
    }
}
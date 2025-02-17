using System.Diagnostics;
using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class TurretTemplate : StructureTemplate
{
    public enum TargetSelector
    {
        Nearest,
        Random,
        RandomFocus,
        Strongest,
        Weakest,
        Flyer
    }
    
    public double Range;
    public ProjectileTemplate Projectile;
    // public float Damage;
    public double RateOfFire;
    // public TurretTargetSelector TargetSelector;
    public TargetSelector TargetMode;
    public bool CanHitFlying;
    public bool CanHitGround;

    public TurretTemplate(string name, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, double range, ProjectileTemplate projectile, double rateOfFire, TargetSelector targetMode, bool canHitGround, bool canHitFlying) : base(name, texture, maxHealth, price, levelRequirement, baseHate)
    {
        Range = range;
        Projectile = projectile;
        //Damage = damage;
        RateOfFire = rateOfFire;
        TargetMode = targetMode;
        CanHitGround = canHitGround;
        CanHitFlying = canHitFlying;
    }
    
    public override Turret Instantiate(Team team, int x, int y)
    {
        return new Turret(this, team, x, y);
    }
    
    public override string GetDescription()
    {
        return $"{Name}\n" +
               $"${Price}\n" +
               $"HP: {MaxHealth}\n" +
               $"Damage: {Projectile.Damage} ({Projectile.Damage / (RateOfFire / 60)}/s)\n" +
               $"Range: {Range}";
    }
}

public class Turret : Structure
{
    private double _timeLastFired;
    private TurretTemplate _template;
    private Minion? _target;
    
    public Turret(TurretTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (Time.Scaled - _timeLastFired > 60/_template.RateOfFire)
        {
            switch (_template.TargetMode)
            {
                case TurretTemplate.TargetSelector.Nearest:
                    _target = FindTargetNearest();
                    break;
                case TurretTemplate.TargetSelector.Random:
                    _target = FindTargetRandom();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            if (_target != null && _target.Health <= 0)
            {
                _target = null;
            }

            if (_target != null)
            {
                _template.Projectile.Instantiate(_target, this);
                _timeLastFired = Time.Scaled;
            }
        }
    }

    private Minion? FindTargetNearest()
    {
        Minion? nearest = null;
        double minDist = double.MaxValue;
        for (int i = 0; i < World.Minions.Count; i++)
        {
            Minion m = World.Minions[i];
            if (m.Team == Team) continue;
            if ((m.Template.IsFlying && !_template.CanHitFlying) || (!m.Template.IsFlying && !_template.CanHitGround)) continue;
            double d = Vector2.Distance(World.Minions[i].Position, position);
            if (d < _template.Range && d < minDist)
            {
                minDist = d;
                nearest = m;
            }
        }

        return nearest;
    }

    private Minion? FindTargetRandom()
    {
        Minion? random = null;
        List<Minion> ValidTargets = new List<Minion>();
        foreach (Minion m in World.Minions)
        {
            if ((m.Template.IsFlying && !_template.CanHitFlying) || (!m.Template.IsFlying && !_template.CanHitGround)) continue;
            if (m.Team != Team && Vector2.Distance(m.Position, position) < _template.Range)
            {
                ValidTargets.Add(m);
            }
        }

        if (ValidTargets.Count > 0)
        {
            random = ValidTargets[Random.Shared.Next(ValidTargets.Count)];
        }
        
        return random;
    }

    private Minion? FindTargetRandomFocus()
    {
        Minion? random = _target;
        if (random != null && random.Health > 0) return random;
        return FindTargetRandom();
    }
}
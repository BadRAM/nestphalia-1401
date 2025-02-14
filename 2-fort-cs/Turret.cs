using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class TurretTemplate : StructureTemplate
{
    public enum TargetSelector
    {
        Nearest,
        Random,
        Highest,
        Lowest,
        Flyer
    }
    
    public double Range;
    public ProjectileTemplate Projectile;
    // public float Damage;
    public double RateOfFire;
    public TargetSelector TargetMode;

    public TurretTemplate(string name, Texture texture, double maxHealth, double price, int levelRequirement, double range, ProjectileTemplate projectile, double rateOfFire, TargetSelector targetMode = TargetSelector.Nearest) : base(name, texture, maxHealth, price, levelRequirement)
    {
        Range = range;
        Projectile = projectile;
        //Damage = damage;
        RateOfFire = rateOfFire;
        TargetMode = targetMode;
    }
    
    public override Turret Instantiate(int x, int y)
    {
        return new Turret(this, x, y);
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
    private double lastFireTime;
    private TurretTemplate _template;
    
    public Turret(TurretTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        base.Update();
        if (Time.Scaled - lastFireTime > 60/_template.RateOfFire)
        {
            if (_template.TargetMode == TurretTemplate.TargetSelector.Nearest)
            {
                Minion? nearest = null;
                double minDist = double.MaxValue;
                for (int i = 0; i < World.Minions.Count; i++)
                {
                    if (World.Minions[i].Team == Team) continue;
                    double d = Vector2.Distance(World.Minions[i].Position, position);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = World.Minions[i];
                    }
                }

                if (minDist < _template.Range && nearest != null)
                {
                    World.Projectiles.Add(new Projectile(_template.Projectile, position, nearest));
                    lastFireTime = Time.Scaled;
                }
            }
            else if (_template.TargetMode == TurretTemplate.TargetSelector.Random)
            {
                List<Minion> ValidTargets = new List<Minion>();
                foreach (Minion m in World.Minions)
                {
                    if (m.Team != Team && Vector2.Distance(m.Position, position) < _template.Range)
                    {
                        ValidTargets.Add(m);
                    }
                }

                if (ValidTargets.Count > 0)
                {
                    World.Projectiles.Add(new Projectile(_template.Projectile, position, ValidTargets[Random.Shared.Next(ValidTargets.Count)]));
                    lastFireTime = Time.Scaled;
                }
            }
        }
    }
}
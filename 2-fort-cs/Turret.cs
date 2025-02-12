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
    
    public float Range;
    public ProjectileTemplate Projectile;
    // public float Damage;
    public float RateOfFire;
    public TargetSelector TargetMode;

    public TurretTemplate(string name, Texture texture, float maxHealth, float price, int levelRequirement, float range, ProjectileTemplate projectile, float rateOfFire, TargetSelector targetMode = TargetSelector.Nearest) : base(name, texture, maxHealth, price, levelRequirement)
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
}

public class Turret : Structure
{
    private float lastFireTime;
    private TurretTemplate _template;
    
    public Turret(TurretTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        base.Update();
        if (Raylib.GetTime() - lastFireTime > 60f/_template.RateOfFire)
        {
            if (_template.TargetMode == TurretTemplate.TargetSelector.Nearest)
            {
                Minion? nearest = null;
                float minDist = float.MaxValue;
                for (int i = 0; i < World.Minions.Count; i++)
                {
                    if (World.Minions[i].Team == Team) continue;
                    float d = Vector2.Distance(World.Minions[i].Position, position);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = World.Minions[i];
                    }
                }

                if (minDist < _template.Range && nearest != null)
                {
                    World.Projectiles.Add(new Projectile(_template.Projectile, position, nearest));
                    lastFireTime = (float)Raylib.GetTime();
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
                    lastFireTime = (float)Raylib.GetTime();
                }
            }
        }
    }
}
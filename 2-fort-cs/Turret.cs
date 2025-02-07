using System.Numerics;
using Raylib_cs;

namespace _2_fort_cs;

public class TurretTemplate : StructureTemplate
{
    public float Range;
    public ProjectileTemplate Projectile;
    // public float Damage;
    public float RateOfFire;

    public TurretTemplate(string name, Texture2D texture, float maxHealth, float range, ProjectileTemplate projectile, float rateOfFire) : base(name, texture, maxHealth)
    {
        Range = range;
        Projectile = projectile;
        //Damage = damage;
        RateOfFire = rateOfFire;
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
    }
    
    public virtual void Fire()
    {
        
    }
}
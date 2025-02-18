using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class BroodMinionTemplate : MinionTemplate
{
    
    public BroodMinionTemplate(string name, Texture texture, double maxHealth, double armor, ProjectileTemplate projectile, double range, double rateOfFire, double speed, bool isFlying, float physicsRadius) : base(name, texture, maxHealth, armor, projectile, range, rateOfFire, speed, isFlying, physicsRadius)
    {
    }
}
    
public class BroodMinion : Minion
{
    
    public BroodMinion(MinionTemplate template, Team team, Vector2 position, NavPath navPath) : base(template, team, position, navPath)
    {
    }
}
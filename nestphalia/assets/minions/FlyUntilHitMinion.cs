using System.Numerics;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public class FlyUntilHitMinionTemplate : FlyingMinionTemplate
{
    public double LandSpeed;
    
    public FlyUntilHitMinionTemplate(JObject jObject) : base(jObject)
    {
        LandSpeed = jObject.Value<double?>("LandSpeed") ?? throw new ArgumentNullException();
    }
    
    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        Minion m = new FlyUntilHitMinion(this, team, position, navPath);
        World.RegisterMinion(m);
        return m;
    }
}
    
public class FlyUntilHitMinion : FlyingMinion
{
    private FlyUntilHitMinionTemplate _template;
    
    public FlyUntilHitMinion(FlyUntilHitMinionTemplate template, Team team, Vector3 position, NavPath navPath) : base(template, team, position, navPath)
    {
        _template = template;
        Armor = 0;
    }

    public override void Update()
    {
        bool wasFlying = IsFlying;
        
        base.Update();

        // On the update where we land, increase armor and repath
        if (wasFlying && !IsFlying)
        {
            SetTarget(NavPath.Destination);
            Armor = Template.Armor;
            // GameConsole.WriteLine($"{Template.ID} landed safely.");
        }
    }

    public override void Hurt(double damage, Projectile? damageSource = null, bool ignoreArmor = false, bool minDamage = true)
    {
        base.Hurt(damage, damageSource, ignoreArmor, minDamage);

        if (WantToFly)
        {
            WantToFly = false;
        }
    }
    
    protected override double AdjustSpeed(double BaseSpeed)
    {
        if (!IsFlying) BaseSpeed = _template.LandSpeed;
        return base.AdjustSpeed(BaseSpeed);
    }
}
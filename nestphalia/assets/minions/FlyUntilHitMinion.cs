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
    
    public override void Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        World.RegisterMinion(new FlyUntilHitMinion(this, team, position, navPath));
    }
}
    
public class FlyUntilHitMinion : FlyingMinion
{
    private FlyUntilHitMinionTemplate _template;
    
    public FlyUntilHitMinion(FlyUntilHitMinionTemplate template, Team team, Vector3 position, NavPath navPath) : base(template, team, position, navPath)
    {
        _template = template;
        Armor = 0;
        FlyAnimIndex = 5;
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

    public override void Hurt(double damage, Projectile? damageSource = null)
    {
        base.Hurt(damage, damageSource);

        if (WantToFly)
        {
            WantToFly = false;
        }
    }
    
    protected override double AdjustedSpeed()
    {
        double adjustedSpeed = Template.Speed;
        if (!IsFlying) adjustedSpeed = _template.LandSpeed;
        Structure? structure = World.GetTileAtPos(Position);
        if (Glued || (!IsFlying && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
        if (Frenzy) adjustedSpeed *= 2;
        return adjustedSpeed;
    }
}
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public class AttackBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public SubAsset<AttackTemplate> Attack;
    
    public AttackBeaconTemplate(JObject jObject) : base(jObject)
    {
        Attack = new SubAsset<AttackTemplate>(jObject.GetValue("Attack") ?? "");
    }

    public override Structure Instantiate(Team team, int x, int y)
    {
        return new AttackBeacon(this, team, x, y);
    }
}

public class AttackBeacon : ActiveAbilityBeacon
{
    private AttackBeaconTemplate _template;
    
    public AttackBeacon(AttackBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
    }

    public override void Activate(Vector2 targetPosition)
    {
        base.Activate(targetPosition);
        _template.Attack.Asset.Instantiate(targetPosition.XYZ(), this, Position);
    }

    // TODO: Implement SelectPosition properly
    public override Vector2? SelectPosition(double minimumValue)
    { 
        return null;
    }
}
using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class BuffBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public double Radius;
    public string Effect;
    public double Power;
    public double Duration;
    
    public BuffBeaconTemplate(JObject jObject) : base(jObject)
    {
        Radius = jObject.Value<double?>("Radius") ?? 48;
        Effect = jObject.Value<string?>("Effect") ?? throw new ArgumentNullException();
        Power = jObject.Value<double?>("Power") ?? 1;
        Duration = jObject.Value<double?>("Duration") ?? 5;
    }

    public override Structure Instantiate(Team team, int x, int y)
    {
        return new BuffBeacon(this, team, x, y);
    }
}

public class BuffBeacon : ActiveAbilityBeacon
{
    private BuffBeaconTemplate _template;
    
    public BuffBeacon(BuffBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
    }

    public override void Activate(Vector2 targetPosition)
    {
        base.Activate(targetPosition);
        StatusEffect statusTemplate;
        switch (_template.Effect)
        {
            case "Burning":
                statusTemplate = new BurningStatus(_template.Power, _template.Duration);
                break;
            case "Frenzy":
                statusTemplate = new StatusEffect(_template.Effect, _template.Effect, _template.Duration, Color.Red);
                break;
            default:
                statusTemplate = new StatusEffect(_template.Effect, _template.Effect, _template.Duration, Color.Green);
                break;
        }
        foreach (Minion minion in World.GetMinionsInRadius(targetPosition, (float)_template.Radius))
        {
            minion.Status.Add(statusTemplate.Clone());
        }
    }

    // TODO: Implement SelectPosition properly for buff
    public override Vector2? SelectPosition(double minimumValue)
    {
        return null;
    }
}
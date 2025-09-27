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
        foreach (Minion minion in World.GetMinionsInRegion(World.PosToTilePos(targetPosition), (int)(_template.Radius / 24) + 1))
        {
            if (Vector2.Distance(minion.Position.XY(), targetPosition) > _template.Radius) continue;
            switch (_template.Effect)
            {
                case "Burning":
                    minion.Status.Add(new BurningStatus(_template.Power, _template.Duration));
                    break;
                case "Frenzy":
                    minion.Status.Add(new StatusEffect(_template.Effect, _template.Effect, _template.Duration, Color.Red));
                    break;
                default:
                    minion.Status.Add(new StatusEffect(_template.Effect, _template.Effect, _template.Duration, Color.Green));
                    break;
            }
        }
    }

    // TODO: Implement SelectPosition properly for buff
    public override Vector2? SelectPosition(double minimumValue)
    {
        // Vector2 bestPos = Vector2.Zero;
        // double bestValue = Double.MaxValue;
        // foreach (Minion minion in World.Minions)
        // {
        //     if (minion.Team != Team) continue;
        //     double d = Vector2.Distance(minion.Position.XY(), World.GetTileCenter(minion.GetTargetTile()));
        //     if (d <= minimumValue)
        //     {
        //         bestPos = minion.Position.XY();
        //         bestValue = d;
        //     }
        // }
        //
        // return bestValue > minimumValue ? bestPos : null;
        return null;
    }
}
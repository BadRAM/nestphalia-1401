using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class RallyBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public RallyBeaconTemplate(JObject jObject) : base(jObject)
    {
    }

    public override RallyBeacon Instantiate(Team team, int x, int y)
    {
        return new RallyBeacon(this, team, x, y);
    }
}

public class RallyBeacon : ActiveAbilityBeacon
{
    public RallyBeacon(ActiveAbilityBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }

    public override void Activate(Vector2 targetPosition)
    {
        base.Activate(targetPosition);
        List<NavPath> paths = new List<NavPath>();
        foreach (Minion minion in World.Minions)
        {
            if (minion.Team == Team)
            {
                NavPath n = minion.WaitForPath(World.RandomDouble() * 0.4 + 0.1);
                n.Reset(minion.Position);
                n.Destination = World.PosToTilePos(targetPosition);
                paths.Add(n);
            }
        }
        Team.PathFinder.FindPathsBatched(paths);
    }

    public override Vector2? SelectPosition(double minimumValue)
    {
        double value = 0;
        double mult = 0;
        List<Int2D> targets = new List<Int2D>();
        foreach (Minion minion in World.Minions)
        {
            if (minion.Team != Team) continue;
            value += minion.Health;
            if (targets.Contains(minion.GetTargetTile())) continue;
            targets.Add(minion.GetTargetTile());
            mult++;
        }

        if (value * mult > minimumValue * 10)
        {
            return World.GetTileCenter(targets[World.RandomInt(0, targets.Count)]);
        }
        return null;
    }
}
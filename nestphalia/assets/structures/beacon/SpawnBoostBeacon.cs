using System.Numerics;
using ZeroElectric.Vinculum;

namespace nestphalia;

public class SpawnBoostBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public SpawnBoostBeaconTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, double cooldown, double cooldownReduction, Texture abilityIcon) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate, cooldown, cooldownReduction, abilityIcon)
    {
    }
    
    public override SpawnBoostBeacon Instantiate(Team team, int x, int y)
    {
        return new SpawnBoostBeacon(this, team, x, y);
    }
}

public class SpawnBoostBeacon : ActiveAbilityBeacon
{
    public SpawnBoostBeacon(ActiveAbilityBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }

    public override void Activate(Vector2 targetPosition)
    {
        Int2D pos = new Int2D(X, Y);
        Int2D target = World.PosToTilePos(targetPosition);
        
        SpawnBoost(pos + Int2D.Up, target);
        SpawnBoost(pos + Int2D.Down, target);
        SpawnBoost(pos + Int2D.Left, target);
        SpawnBoost(pos + Int2D.Right, target);
        
        base.Activate(targetPosition);
    }

    private void SpawnBoost(Int2D pos, Int2D target)
    {
        Structure? s = World.GetTile(pos);
        if (s is Spawner spawner)
        {
            spawner.SetTarget(target);
            spawner.WaveEffect();
        }
    }


    public override Vector2? SelectPosition(double minimumValue)
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? s = World.GetTile(x, y);
                if (s != null && s.Team != Team && s is not Rubble)
                {
                    if (Team.GetHateFor(x,y) > 0)
                    {
                        targets.Add(new Sortable<Int2D>(Team.GetHateFor(x,y), new Int2D(x,y)));
                    }
                }
            }
        }
        
        targets = targets.OrderByDescending(o => o.Order).ToList();
        
        int i = Utils.WeightedRandom(targets.Count);
        Console.WriteLine($"Picked target {i}");
        return World.GetTileCenter(targets[i].Value);
    }
}
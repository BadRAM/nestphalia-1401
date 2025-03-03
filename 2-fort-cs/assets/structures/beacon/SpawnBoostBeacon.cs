using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

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
        // Structure s = World.GetTileAtPos(targetPosition);
        // if (s is Spawner spawner)
        // {
        //     spawner.WaveEffect();
        //     base.Activate(targetPosition);
        // }
        
        Structure? s = World.GetTile(X+1, Y);
        if (s is Spawner spawner1)
        {
            spawner1.WaveEffect();
            base.Activate(targetPosition);
        }
        
        s = World.GetTile(X-1, Y);
        if (s is Spawner spawner2)
        {
            spawner2.WaveEffect();
            base.Activate(targetPosition);
        }
        
        s = World.GetTile(X, Y+1);
        if (s is Spawner spawner3)
        {
            spawner3.WaveEffect();
            base.Activate(targetPosition);
        }
        
        s = World.GetTile(X, Y-1);
        if (s is Spawner spawner4)
        {
            spawner4.WaveEffect();
            base.Activate(targetPosition);
        }
    }


    public override Vector2? SelectPosition(double minimumValue)
    {
        // List<Spawner> spawners = new List<Spawner>();
        //
        // for (int x = 0; x < World.BoardWidth; x++)
        // {
        //     for (int y = 0; y < World.BoardHeight; y++)
        //     {
        //         Structure? structure = World.GetTile(x,y);
        //         if (structure?.Team == Team && structure is Spawner nest)
        //         {
        //             spawners.Add(nest);
        //         }
        //     }
        // }
        //
        // if (spawners.Count == 0) return null;
        // return spawners[Random.Shared.Next(0, spawners.Count)].GetCenter();
        return position;
    }
}
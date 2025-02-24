using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class RepairBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public RepairBeaconTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, double cooldown, double cooldownReduction, Texture abilityIcon) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate, cooldown, cooldownReduction, abilityIcon)
    {
    }
    
    public override RepairBeacon Instantiate(Team team, int x, int y)
    {
        return new RepairBeacon(this, team, x, y);
    }
}

public class RepairBeacon : ActiveAbilityBeacon
{
    public RepairBeacon(ActiveAbilityBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }

    public override void Activate(Vector2 targetPosition)
    {
        Structure? structure = World.GetTileAtPos(targetPosition);
        if (structure != null && structure.Team == Team)
        {
            if (structure is Rubble rubble)
            {
                World.SetTile(rubble.DestroyedStructure, Team, World.PosToTilePos(targetPosition));
                base.Activate(targetPosition);
            }
            else if (structure.Health < structure.Template.MaxHealth)
            {
                structure.Health = structure.Template.MaxHealth;
                base.Activate(targetPosition);
            }
        }
    }

    public override Vector2? SelectPosition(double minimumValue = 150)
    {
        Vector2 bestPos = Vector2.Zero;
        double bestScore = double.MinValue;
        
        for (int x = 0; x < World.BoardWidth; x++)
        {
            for (int y = 0; y < World.BoardHeight; y++)
            {
                Structure? structure = World.GetTile(x,y);
                if (structure?.Team == Team && structure is Rubble rubble)
                {
                    if (rubble.DestroyedStructure.Price > bestScore)
                    {
                        bestScore = rubble.DestroyedStructure.Price;
                        bestPos = World.GetTileCenter(x, y);
                    }
                }
            }
        }
        
        return bestScore > minimumValue ? bestPos : null;
    }
}
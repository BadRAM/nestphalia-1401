using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class RepairBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public RepairBeaconTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double price, int levelRequirement, double baseHate, double cooldown, double cooldownReduction, Texture2D abilityIcon) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate, cooldown, cooldownReduction, abilityIcon)
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
        Int2D pos = World.PosToTilePos(targetPosition);
        
        Repair(pos, true);
        Repair(pos + Int2D.Up, false);
        Repair(pos + Int2D.Down, false);
        Repair(pos + Int2D.Left, false);
        Repair(pos + Int2D.Right, false);
        base.Activate(targetPosition);
    }

    private void Repair(Int2D targetPosition, bool repairNonWalls)
    {
        Structure? structure = World.GetTile(targetPosition);
        if (structure != null && structure.Team == Team && (repairNonWalls || structure.GetType() == typeof(Structure) || (structure is Rubble r && r.DestroyedStructure.GetType() == typeof(StructureTemplate))))
        {
            if (structure is Rubble rubble)
            {
                World.SetTile(rubble.DestroyedStructure, Team, targetPosition);
                structure = World.GetTile(targetPosition); 
                if (structure is ActiveAbilityBeacon beacon)
                {
                    Team.AddBeacon(beacon);
                }
            }
            else if (structure.Health < structure.Template.MaxHealth)
            {
                structure.Health = structure.Template.MaxHealth;
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
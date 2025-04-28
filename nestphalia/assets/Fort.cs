using System.Text.Json.Serialization;

namespace nestphalia;

public class Fort
{
    [JsonInclude] public string Name = "Your Fort";
    [JsonInclude] public string Comment = "It's a fort!";
    [JsonInclude] public string[] Board = new string[20 * 20];
    [JsonIgnore] public string Path = "";
    [JsonIgnore] public double TotalCost;
    
    public void LoadToBoard(bool rightSide)
    {
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                if (rightSide)
                {
                    World.SetTile(Assets.GetTileByID(Board[x+y*20]), World.RightTeam, 46-x,y+1);
                }
                else
                {
                    World.SetTile(Assets.GetTileByID(Board[x+y*20]), World.LeftTeam, x+1,y+1);
                }
            }
        }
    }

    public string FortSummary()
    {
        int structureCount = 0;
        int turretCount = 0;
        int utilityCount = 0;
        int nestCount = 0;
        double totalCost = 0;
        
        for (int x = 0; x < 20; ++x)
        {
            for (int y = 0; y < 20; ++y)
            {
                StructureTemplate? t = Assets.GetTileByID(Board[x+y*20]);
                if (t == null) continue;
                structureCount++;
                totalCost += t.Price;
                if (t.Class == StructureTemplate.StructureClass.Utility) utilityCount++;
                if (t.Class == StructureTemplate.StructureClass.Tower) turretCount++;
                if (t.Class == StructureTemplate.StructureClass.Nest) nestCount++;
            }
        }

        return $"{Name}\n" +
               $"${totalCost}\n" +
               $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               $"{nestCount} Nests\n" +
               $"{structureCount} Total";
    }

    public string IsValid(Campaign campaign)
    {
        int nestCount = 0;
        int stratagemCount = 0;
        double totalCost = 0;
        bool illegalBuilding = false;
        
        for (int x = 0; x < 20; ++x)
        {
            for (int y = 0; y < 20; ++y)
            {
                StructureTemplate? t = Assets.GetTileByID(Board[x+y*20]);
                if (t == null) continue;
                totalCost += t.Price;
                if (t is SpawnerTemplate) nestCount++;
                if (t is ActiveAbilityBeaconTemplate) stratagemCount++;
                if (t.LevelRequirement > campaign.Level) illegalBuilding = true;
            }
        }

        string reason = "";
        if (nestCount <= 0) reason = "Fort has no nests!";
        else if (nestCount > campaign.GetNestCap()) reason = "Fort has too many nests!";
        else if (stratagemCount > 4) reason = "Fort has too many stratagems!";
        else if (totalCost > campaign.Money) reason = "Fort is to expensive!";
        else if (illegalBuilding) reason = "Fort contains locked structures!";

        return reason;
    }

    // Updates this fort object to reflect the current player side fort in the world.
    public void SaveBoard()
    {
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? "";
            }
        }
    }

    public void UpdateCost()
    {
        double totalCost = 0;
        
        for (int x = 0; x < 20; ++x)
        {
            for (int y = 0; y < 20; ++y)
            {
                StructureTemplate? t = Assets.GetTileByID(Board[x+y*20]);
                if (t == null) continue;
                totalCost += t.Price;
            }
        } 
        
        TotalCost = totalCost;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Fort))]
[JsonSerializable(typeof(Campaign))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
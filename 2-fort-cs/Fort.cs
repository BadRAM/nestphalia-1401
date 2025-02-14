using System.Text.Json.Serialization;

namespace _2_fort_cs;

public class Fort
{
    [JsonInclude] public string Name = "Fort";
    [JsonInclude] public string Comment = "It's a fort!";
    [JsonInclude] public string[] Board = new string[20 * 20];
    
    public void LoadToBoard()
    {
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                World.SetTile(Assets.GetTileByName(Board[x+y*20]), x+1,y+1);
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
                StructureTemplate? t = Assets.GetTileByName(Board[x+y*20]);
                if (t == null) continue;
                structureCount++;
                totalCost += t.Price;
                if (t is TurretTemplate) turretCount++;
                if (t is DoorTemplate) utilityCount++;
                if (t is SpawnerTemplate) nestCount++;
            }
        }

        return $"{Name}\n" +
               $"${totalCost}\n" +
               $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               $"{nestCount} Nests\n" +
               $"{structureCount} Total";
    }

    // Updates this fort object to reflect the current player side fort in the world.
    public void SaveBoard()
    {
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.Name ?? "";
            }
        }
    }
}
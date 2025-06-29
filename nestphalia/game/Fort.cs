using System.Drawing;
using System.Text.Json.Serialization;

namespace nestphalia;

public class Fort
{
    [JsonInclude] public string Name = "Your Fort";
    [JsonInclude] public string Comment = "It's a fort!";
    [JsonInclude] public string[] Board = new string[20 * 20]; // TODO: Make this a dimensional array, currently blocked by serialization strategy. New serializer could fix.
    [JsonIgnore] public string Path = "";
    [JsonIgnore] public double TotalCost;
    
    public Fort(string name, string path)
    {
        Name = name;
        Path = path;
    }

    // This constructor is for the json deserializer, please use the other one for creating new forts.
    public Fort() { }
    
    public void LoadToBoard(Int2D position, bool flip)
    {
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
        {
            if (flip)
            {
                World.SetTile(Assets.GetStructureByID(Board[x+y*20]), World.RightTeam, (20 + position.X) - x,y + position.Y);
            }
            else
            {
                World.SetTile(Assets.GetStructureByID(Board[x+y*20]), World.LeftTeam, x + position.X,y + position.Y);
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
        for (int y = 0; y < 20; ++y)
        {
            StructureTemplate? t = Assets.GetStructureByID(Board[x+y*20]);
            if (t == null) continue;
            structureCount++;
            totalCost += t.Price;
            if (t.Class == StructureTemplate.StructureClass.Utility) utilityCount++;
            if (t.Class == StructureTemplate.StructureClass.Tower) turretCount++;
            if (t.Class == StructureTemplate.StructureClass.Nest) nestCount++;
        }

        return $"{Name}\n" +
               $"${totalCost}\n" +
               $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               $"{nestCount} Nests\n" +
               $"{structureCount} Total";
    }

    public string IsValid(CampaignSaveData campaign)
    {
        int nestCount = 0;
        int stratagemCount = 0;
        double totalCost = 0;
        bool illegalBuilding = false;
        
        for (int x = 0; x < 20; ++x)
        for (int y = 0; y < 20; ++y)
        {
            StructureTemplate? t = Assets.GetStructureByID(Board[x+y*20]);
            if (t == null) continue;
            totalCost += t.Price;
            if (t is SpawnerTemplate) nestCount++;
            if (t is ActiveAbilityBeaconTemplate) stratagemCount++;
            if (t.LevelRequirement > campaign.Level) illegalBuilding = true;
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
        for (int y = 0; y < 20; y++)
        {
            Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? "";
        }
    }

    public void UpdateCost()
    {
        double totalCost = 0;
        
        for (int x = 0; x < 20; ++x)
        for (int y = 0; y < 20; ++y)
        {
            StructureTemplate? t = Assets.GetStructureByID(Board[x+y*20]);
            if (t == null) continue;
            totalCost += t.Price;
        }
        
        TotalCost = totalCost;
    }
}

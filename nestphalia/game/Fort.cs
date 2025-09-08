using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public class Fort
{
    public string Name;
    public string Comment = "It's a fort!";
    public string[] Board = new string[20 * 20]; // TODO: Make this a dimensional array, currently blocked by serialization strategy. New serializer could fix.
    [JsonIgnore] public string Path = "";
    [JsonIgnore] public double TotalCost;
    
    // This constructor is for making new forts from scratch
    public Fort(string path)
    {
        Name = "fort";
        Path = path;
    }

    // This constructor is for the json deserializer.
    public Fort(JObject j, string path)
    {
        Name = j.Value<string?>("Name") ?? throw new ArgumentNullException();
        Comment = j.Value<string?>("Comment") ?? throw new ArgumentNullException();
        Board = j.Value<JArray>("Board")?.ToObject<string[]>() ?? throw new ArgumentNullException();
        if (Board.Length != 20 * 20) throw new Exception("Invalid board size");
        Path = path;
    }
    
    public void LoadToBoard(Level.FortSpawnZone spawnZone)
    {
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
        {
            if (spawnZone.Flip)
            {
                if (!Assets.Exists<StructureTemplate>(Board[x+y*20])) continue;
                World.SetTile(Assets.Get<StructureTemplate>(Board[x+y*20]), World.RightTeam, (19 + spawnZone.X) - x,y + spawnZone.Y);
            }
            else
            {
                if (!Assets.Exists<StructureTemplate>(Board[x+y*20])) continue;
                World.SetTile(Assets.Get<StructureTemplate>(Board[x+y*20]), World.LeftTeam, x + spawnZone.X,y + spawnZone.Y);
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
            if (!Assets.Exists<StructureTemplate>(Board[x+y*20])) continue;
            StructureTemplate t = Assets.Get<StructureTemplate>(Board[x+y*20]);
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
            if (!Assets.Exists<StructureTemplate>(Board[x+y*20])) continue;
            StructureTemplate t = Assets.Get<StructureTemplate>(Board[x+y*20]);
            totalCost += t.Price;
            if (t is SpawnerTemplate) nestCount++;
            if (t is ActiveAbilityBeaconTemplate) stratagemCount++;
            if (!campaign.UnlockedStructures.Contains(t.ID)) illegalBuilding = true;
        }

        string reason = "";
        if (illegalBuilding) reason = "Fort contains locked structures!";
        else if (nestCount <= 0) reason = "Fort has no nests!";
        else if (nestCount > campaign.GetNestCap()) reason = "Fort has too many nests!";
        else if (stratagemCount > 4) reason = "Fort has too many stratagems!";
        else if (totalCost > campaign.Money) reason = "Fort is too expensive!";

        return reason;
    }

    // Updates this fort object to reflect the current player side fort in the world.
    public void SaveBoard()
    {
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
        {
            Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? ""; // TODO: respect level's player offset?
        }
    }

    public void UpdateCost()
    {
        double totalCost = 0;
        
        for (int x = 0; x < 20; ++x)
        for (int y = 0; y < 20; ++y)
        {
            if (!Assets.Exists<StructureTemplate>(Board[x+y*20])) continue;
            StructureTemplate t = Assets.Get<StructureTemplate>(Board[x+y*20]);
            totalCost += t.Price;
        }
        
        TotalCost = totalCost;
    }
    
    public void SaveToDisc()
    {
        if (System.IO.Path.GetFileName(Path) == "" || System.IO.Path.EndsInDirectorySeparator(Path) || !System.IO.Path.HasExtension(Path))
        {
            Path = GetUnusedFileName(Name, Path);
        }
        string jsonString = JObject.FromObject(this).ToString();
        File.WriteAllText(Path, jsonString);
    }

    public static Fort? LoadFromDisc(string filepath)
    {
        filepath = filepath.MakePathAbsolute();
        
        if (!System.IO.Path.Exists(filepath))
        {
            GameConsole.WriteLine($"Failed to find fort at {filepath}");
            return null;
        }
        
        string jsonString = File.ReadAllText(filepath);
        Fort fort = new Fort(JObject.Parse(jsonString), filepath);
        fort.UpdateCost();
        GameConsole.WriteLine($"Loaded {fort.Name}, path: {filepath}");
        return fort;
    }

    // Two use cases: Making a new fort, and saving in a directory that may already have a fort with that filename
    public static string GetUnusedFileName(string name, string path)
    {
        name = string.Concat(name.Where(c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-')) ?? "");
        name = name.Replace(' ', '-');
        if (name == "") name = "fort";

        if (File.Exists($"{path}/{name}.fort")) // Collision!
        {
            int number = 2;
            while (File.Exists($"{path}/{name}{number}.fort"))
            {
                number++;
                if (number > 9999) throw new Exception("Can't find a valid fort name!");
            }
            name = $"{path}/{name}{number}.fort";
        }
        else
        {
            name = $"{path}/{name}.fort";
        }

        return name;
    }
}
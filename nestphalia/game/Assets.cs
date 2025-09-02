using System.Reflection;
using Newtonsoft.Json.Linq;

namespace nestphalia;

// This class is for loading game content from disc into memory as objects.
// In the past, all the content data itself was hard coded into this file, but now that it's been shrunkled it might be
// more consistent to access these functions through Resources.
public static class Assets
{
    private static Dictionary<string, FloorTileTemplate> _floorTiles = new Dictionary<string, FloorTileTemplate>();
    public static FloorTileTemplate BlankFloor;
    private static Dictionary<string, StructureTemplate> _structures = new Dictionary<string, StructureTemplate>();
    private static Dictionary<string, Level> _levels = new Dictionary<string, Level>();


    // The lookup table must be used instead of reflection, so that static analysis knows not to trim the JsonAsset
    // constructor when publishing with trimmed assemblies. it also provides serializer stability if type names change internally.
    private static readonly Dictionary<string, ConstructorInfo> JsonAssetTypes = new Dictionary<string, ConstructorInfo>()
    {
        { "Level",                     typeof(Level).GetConstructor([typeof(JObject)])! },
        { "StructureTemplate",         typeof(StructureTemplate).GetConstructor([typeof(JObject)])! },
        { "SpawnerTemplate",           typeof(SpawnerTemplate).GetConstructor([typeof(JObject)])! },
        { "DoorTemplate",              typeof(DoorTemplate).GetConstructor([typeof(JObject)])! },
        { "SpringBoardTemplate",       typeof(SpringBoardTemplate).GetConstructor([typeof(JObject)])! },
        { "GluePaperTemplate",         typeof(GluePaperTemplate).GetConstructor([typeof(JObject)])! },
        { "HazardSignTemplate",        typeof(HazardSignTemplate).GetConstructor([typeof(JObject)])! },
        { "MinefieldTemplate",         typeof(MinefieldTemplate).GetConstructor([typeof(JObject)])! },
        { "TowerTemplate",             typeof(TowerTemplate).GetConstructor([typeof(JObject)])! },
        { "FrenzyBeaconTemplate",      typeof(FrenzyBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "RallyBeaconTemplate",       typeof(RallyBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "RepairBeaconTemplate",      typeof(RepairBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "SpawnBoostBeaconTemplate",  typeof(SpawnBoostBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "LightningBoltTemplate",     typeof(LightningBoltTemplate).GetConstructor([typeof(JObject)])! },
        { "MortarShellTemplate",       typeof(MortarShellTemplate).GetConstructor([typeof(JObject)])! },
        { "ProjectileTemplate",        typeof(ProjectileTemplate).GetConstructor([typeof(JObject)])! },
        { "MinionTemplate",            typeof(MinionTemplate).GetConstructor([typeof(JObject)])! },
        { "BroodMinionTemplate",       typeof(BroodMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "FlyingMinionTemplate",      typeof(FlyingMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "FlyUntilHitMinionTemplate", typeof(FlyUntilHitMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "HeroMinionTemplate",        typeof(HeroMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "HopperMinionTemplate",      typeof(HopperMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "RangedMinionTemplate",      typeof(RangedMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "SapperMinionTemplate",      typeof(SapperMinionTemplate).GetConstructor([typeof(JObject)])! },
    };
    
    public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    {
        if (!jObject.ContainsKey("Type"))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
            return null;
        }
        
        if (!JsonAssetTypes.ContainsKey(jObject.Value<string>("Type")))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
            return null;
        }
        
        return JsonAssetTypes[jObject.Value<string>("Type")].Invoke([jObject]) as T;
    }
    
    public static void Load()
    {
        _floorTiles.Clear();
        _structures.Clear();
        _levels.Clear();
        
        _floorTiles.Add("floor_1", new FloorTileTemplate("floor_1", Resources.GetTextureByName("floor1")));
        _floorTiles.Add("floor_2", new FloorTileTemplate("floor_2", Resources.GetTextureByName("floor2")));
        _floorTiles.Add("floor_empty", new FloorTileTemplate("floor_empty", Resources.GetTextureByName("clear")));
        BlankFloor = _floorTiles["floor_empty"];
        
        foreach (string path in Directory.GetFiles(Resources.Dir + "/resources/content"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                JObject content = JObject.Parse(File.ReadAllText(path));
                foreach (JObject structure in content.Value<JArray>("Structures"))
                {
                    StructureTemplate t = LoadJsonAsset<StructureTemplate>(structure);
                    _structures.Add(t.ID, t);
                }
            }
        }
        
        foreach (string path in Directory.GetFiles(Resources.Dir + "/resources/levels"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                Level? level = LoadJsonAsset<Level>(JObject.Parse(File.ReadAllText(path)));
                if (level == null) continue;
                _levels.Add(level.ID, level);
            }
        }
    }

    public static StructureTemplate? GetStructureByID(string? id)
    {
        return _structures.GetValueOrDefault(id ?? "");
    }

    public static StructureTemplate[] GetAllStructures()
    {
        return _structures.Values.ToArray();
    }

    public static FloorTileTemplate? GetFloorTileByID(string id)
    {
        return _floorTiles.GetValueOrDefault(id ?? "");
    }
    
    public static FloorTileTemplate[] GetAllFloorTiles()
    {
        return _floorTiles.Values.ToArray();
    }

    public static Level GetLevelByID(string id)
    {
        return _levels.GetValueOrDefault(id ?? "", _levels["level_arena"]);
    }

    public static Level[] GetAllLevels()
    {
        return _levels.Values.ToArray();
    }

    public static void UpdateLevel(Level level)
    {
        _levels[level.ID] = level;
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace nestphalia;

// This class is for loading game content from disc into memory as objects.
// In the past, all the content data itself was hard coded into this file, but now that it's been shrunkled it might be
// more consistent to access these functions through Resources.
public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();

    // The lookup table must be used instead of reflection, so that static analysis knows not to trim the JsonAsset
    // constructor when publishing with trimmed assemblies. it also provides serializer stability if type names change internally.
    private static readonly Dictionary<string, ConstructorInfo> JsonAssetTypes = new Dictionary<string, ConstructorInfo>()
    {
        { "StructureTemplate",         typeof(StructureTemplate).GetConstructor([typeof(JObject)])! },
        { "SpawnerTemplate",           typeof(SpawnerTemplate).GetConstructor([typeof(JObject)])! },
        { "DoorTemplate",              typeof(DoorTemplate).GetConstructor([typeof(JObject)])! },
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
        { "SapperMinionTemplate",      typeof(SapperMinionTemplate).GetConstructor([typeof(JObject)])! },
    };
    
    public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    {
        if (!jObject.ContainsKey("type"))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
            return null;
        }
        
        if (!JsonAssetTypes.ContainsKey(jObject.Value<string>("type")))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
            return null;
        }
        
        return JsonAssetTypes[jObject.Value<string>("type")].Invoke([jObject]) as T;
    }
    
    public static void Load()
    {
        FloorTiles.Add(new FloorTileTemplate("Floor1", Resources.GetTextureByName("floor1")));
        FloorTiles.Add(new FloorTileTemplate("Floor2", Resources.GetTextureByName("floor2")));
        FloorTiles.Add(new FloorTileTemplate("Blank", Resources.GetTextureByName("clear")));
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/content"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                JObject content = JObject.Parse(File.ReadAllText(path));
                foreach (JObject structure in content.Value<JArray>("structures"))
                {
                    Structures.Add(LoadJsonAsset<StructureTemplate>(structure));
                }
            }
        }
    }

    public static StructureTemplate? GetStructureByID(string ID)
    {
        foreach (StructureTemplate structureTemplate in Structures)
        {
            if (structureTemplate.ID == ID)
            {
                return structureTemplate;
            }
        }
        return null;
    }
}
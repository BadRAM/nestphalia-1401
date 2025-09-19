using System.Reflection;
using Newtonsoft.Json.Linq;

namespace nestphalia;

// This class is for loading game content from disc into memory as objects.
// In the past, all the content data itself was hard coded into this file, but now that it's been shrunkled it might be
// more consistent to access these functions through Resources.
public static class Assets
{
    private static Dictionary<string, JsonAsset> _assets = new Dictionary<string, JsonAsset>();
    private static Dictionary<Type, List<JsonAsset>> _assetSets = new Dictionary<Type, List<JsonAsset>>();

    // The lookup table must be used instead of reflection, so that static analysis knows not to trim the JsonAsset
    // constructor when publishing with trimmed assemblies. it also provides deserializer stability if type names change internally.
    private static readonly Dictionary<string, ConstructorInfo> JsonAssetTypes = new Dictionary<string, ConstructorInfo>()
    {
        { "StretchyTexture",           typeof(StretchyTexture).GetConstructor([typeof(JObject)])! },
        { "Level",                     typeof(Level).GetConstructor([typeof(JObject)])! },
        { "FloorTileTemplate",         typeof(FloorTileTemplate).GetConstructor([typeof(JObject)])! },
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
    
    public static T LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    {
        if (!jObject.ContainsKey("Type"))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
        }
        
        if (!JsonAssetTypes.ContainsKey(jObject.Value<string>("Type") ?? ""))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
        }
        
        return JsonAssetTypes[jObject.Value<string>("Type")!].Invoke([jObject]) as T ?? throw new NullReferenceException();
    }
    
    public static void Load()
    {
        foreach (string path in Directory.GetFiles(Resources.Dir + "/resources/content"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                JArray content = JArray.Parse(File.ReadAllText(path));
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (JObject asset in content)
                {
                    JsonAsset t = LoadJsonAsset<JsonAsset>(asset);
                    _assets.Add(t.ID, t);
                }
            }
        }
        
        foreach (string path in Directory.GetFiles(Resources.Dir + "/resources/levels"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                Level level = LoadJsonAsset<Level>(JObject.Parse(File.ReadAllText(path)));
                _assets.Add(level.ID, level);
            }
        }
    }

    public static T Get<T>(string id) where T : JsonAsset
    {
        if (!_assets.ContainsKey(id)) throw new Exception($"Invalid Asset ID: {id}");
        JsonAsset ass = _assets[id];
        if (ass is T ret) return ret;
        else throw new Exception($"Asset {id} is not a {typeof(T)}");
    }

    public static List<T> GetAll<T>() where T : JsonAsset
    {
        if (_assetSets.ContainsKey(typeof(T)))
        {
            return new List<T>(_assetSets[typeof(T)].Cast<T>());
        }
        
        List<T> assets = new List<T>();
        foreach (KeyValuePair<string,JsonAsset> asset in _assets)
        {
            if (asset.Value is T a) assets.Add(a);
        }
        _assetSets.Add(typeof(T), assets.Cast<JsonAsset>().ToList());
        return assets;
    }

    public static bool Exists<T>(string? id) where T : JsonAsset
    {
        if (id == null || !_assets.ContainsKey(id)) return false;
        return _assets[id] is T;
    }

    public static bool Exists(string? id)
    {
        if (id == null) return false;
        return _assets.ContainsKey(id);
    }

    public static void UpdateAsset(JsonAsset asset)
    {
        if (!Exists(asset.ID)) throw new Exception($"Tried to update unknown asset {asset.ID}");
        _assets[asset.ID] = asset;
    }
}
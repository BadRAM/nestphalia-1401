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
    private static List<Action> _lateLoadActions = new List<Action>();

    // The lookup table must be used instead of reflection, so that static analysis knows not to trim the JsonAsset
    // constructor when publishing with trimmed assemblies. it also provides deserializer stability if type names change internally.
    private static readonly Dictionary<string, ConstructorInfo> JsonAssetTypes = new Dictionary<string, ConstructorInfo>()
    {
        { "StretchyTexture",           typeof(StretchyTexture).GetConstructor([typeof(JObject)])! },
        { "FloorScatterTexList",       typeof(FloorScatterTexList).GetConstructor([typeof(JObject)])! },
        { "Level",                     typeof(Level).GetConstructor([typeof(JObject)])! },
        { "FloorTile",                 typeof(FloorTileTemplate).GetConstructor([typeof(JObject)])! },
        { "Structure",                 typeof(StructureTemplate).GetConstructor([typeof(JObject)])! },
        { "Spawner",                   typeof(SpawnerTemplate).GetConstructor([typeof(JObject)])! },
        { "Door",                      typeof(DoorTemplate).GetConstructor([typeof(JObject)])! },
        { "SpringBoard",               typeof(SpringBoardTemplate).GetConstructor([typeof(JObject)])! },
        { "Trap",                      typeof(TrapTemplate).GetConstructor([typeof(JObject)])! },
        { "GluePaper",                 typeof(GluePaperTemplate).GetConstructor([typeof(JObject)])! },
        { "HazardSign",                typeof(HazardSignTemplate).GetConstructor([typeof(JObject)])! },
        { "Minefield",                 typeof(MinefieldTemplate).GetConstructor([typeof(JObject)])! },
        { "Tower",                     typeof(TowerTemplate).GetConstructor([typeof(JObject)])! },
        { "AttackBeacon",              typeof(AttackBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "BuffBeacon",                typeof(BuffBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "RallyBeacon",               typeof(RallyBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "RepairBeacon",              typeof(RepairBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "SpawnBoostBeacon",          typeof(SpawnBoostBeaconTemplate).GetConstructor([typeof(JObject)])! },
        { "LightningBolt",             typeof(LightningBoltTemplate).GetConstructor([typeof(JObject)])! },
        { "Explosion",                 typeof(ExplosionTemplate).GetConstructor([typeof(JObject)])! },
        { "MeleeAttack",               typeof(MeleeAttackTemplate).GetConstructor([typeof(JObject)])! },
        { "Boulder",                   typeof(BoulderTemplate).GetConstructor([typeof(JObject)])! },
        { "Projectile",                typeof(ProjectileTemplate).GetConstructor([typeof(JObject)])! },
        { "ArcProjectile",             typeof(ArcProjectileTemplate).GetConstructor([typeof(JObject)])! },
        { "Minion",                    typeof(MinionTemplate).GetConstructor([typeof(JObject)])! },
        { "BroodMinion",               typeof(BroodMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "FlyingMinion",              typeof(FlyingMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "FlyUntilHitMinion",         typeof(FlyUntilHitMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "HeroMinion",                typeof(HeroMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "HopperMinion",              typeof(HopperMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "RangedMinion",              typeof(RangedMinionTemplate).GetConstructor([typeof(JObject)])! },
        { "SapperMinion",              typeof(SapperMinionTemplate).GetConstructor([typeof(JObject)])! },
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

    public static void RegisterLateLoadAction(Action action)
    {
        _lateLoadActions.Add(action);
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
                    IndexAsset(t);
                }
            }
        }
        
        foreach (string path in Directory.GetFiles(Resources.Dir + "/resources/levels"))
        {
            if (Path.GetExtension(path).ToLower() == ".json")
            {
                Level level = LoadJsonAsset<Level>(JObject.Parse(File.ReadAllText(path)));
                IndexAsset(level);
            }
        }

        foreach (Action action in _lateLoadActions)
        {
            action.Invoke();
        }
        _lateLoadActions.Clear();
        _lateLoadActions.TrimExcess();
    }

    public static void IndexAsset(JsonAsset asset)
    {
        _assets.Add(asset.ID, asset);
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

public class SubAsset<T> where T : JsonAsset
{
    public T Asset;
    public string ID = "";

    public SubAsset(JToken jToken)
    {
        if (jToken.Type == JTokenType.String)
        {
            ID = jToken.Value<string>()!;
            Assets.RegisterLateLoadAction(Link);
        }

        if (jToken is JObject jObject)
        {
            Asset = Assets.LoadJsonAsset<T>(jObject);
            ID = Asset.ID;
        }
    }

    public SubAsset(T asset)
    {
        Asset = asset;
        ID = asset.ID;
    }

    private void Link()
    {
        Asset = Assets.Get<T>(ID);
    }
}
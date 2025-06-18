using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();

    public static Dictionary<string, ConstructorInfo> _jsonAssetTypes = new Dictionary<string, ConstructorInfo>()
    {
        { "StructureTemplate", typeof(StructureTemplate).GetConstructor([typeof(JObject)]) },
        { "SpawnerTemplate", typeof(SpawnerTemplate).GetConstructor([typeof(JObject)]) },
        { "DoorTemplate", typeof(DoorTemplate).GetConstructor([typeof(JObject)]) },
        { "GluePaperTemplate", typeof(GluePaperTemplate).GetConstructor([typeof(JObject)]) },
        { "HazardSignTemplate", typeof(HazardSignTemplate).GetConstructor([typeof(JObject)]) },
        { "MinefieldTemplate", typeof(MinefieldTemplate).GetConstructor([typeof(JObject)]) },
        { "TowerTemplate", typeof(TowerTemplate).GetConstructor([typeof(JObject)]) },
        { "FrenzyBeaconTemplate", typeof(FrenzyBeaconTemplate).GetConstructor([typeof(JObject)]) },
        { "RallyBeaconTemplate", typeof(RallyBeaconTemplate).GetConstructor([typeof(JObject)]) },
        { "RepairBeaconTemplate", typeof(RepairBeaconTemplate).GetConstructor([typeof(JObject)]) },
        { "SpawnBoostBeaconTemplate", typeof(SpawnBoostBeaconTemplate).GetConstructor([typeof(JObject)]) },
        { "LightningBoltTemplate", typeof(LightningBoltTemplate).GetConstructor([typeof(JObject)]) },
        { "MortarShellTemplate", typeof(MortarShellTemplate).GetConstructor([typeof(JObject)]) },
        { "ProjectileTemplate", typeof(ProjectileTemplate).GetConstructor([typeof(JObject)]) },
        { "MinionTemplate", typeof(MinionTemplate).GetConstructor([typeof(JObject)]) },
        { "BroodMinionTemplate", typeof(BroodMinionTemplate).GetConstructor([typeof(JObject)]) },
        { "FlyingMinionTemplate", typeof(FlyingMinionTemplate).GetConstructor([typeof(JObject)]) },
        { "FlyUntilHitMinionTemplate", typeof(FlyUntilHitMinionTemplate).GetConstructor([typeof(JObject)]) },
        { "HeroMinionTemplate", typeof(HeroMinionTemplate).GetConstructor([typeof(JObject)]) },
        { "HopperMinionTemplate", typeof(HopperMinionTemplate).GetConstructor([typeof(JObject)]) },
        { "SapperMinionTemplate", typeof(SapperMinionTemplate).GetConstructor([typeof(JObject)]) }
    };
    
    // // This version needs to be updated every time a jsonasset class is created or renamed, but is reflection free and trimmer safe.
    // // Maybe this could be automated with source generation?
    // public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    // {
    //     if (!jObject.ContainsKey("type"))
    //     {
    //         throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
    //         return null;
    //     }
    //
    //     Object o = null;
    //     switch (jObject.Value<string>("type"))
    //     {
    //         case "StructureTemplate":
    //             o = new StructureTemplate(jObject);
    //             break;
    //         case "SpawnerTemplate":
    //             o = new SpawnerTemplate(jObject);
    //             break;
    //         case "DoorTemplate":
    //             o = new DoorTemplate(jObject);
    //             break;
    //         case "GluePaperTemplate":
    //             o = new GluePaperTemplate(jObject);
    //             break;
    //         case "HazardSignTemplate":
    //             o = new HazardSignTemplate(jObject);
    //             break;
    //         case "MinefieldTemplate":
    //             o = new MinefieldTemplate(jObject);
    //             break;
    //         case "TowerTemplate":
    //             o = new TowerTemplate(jObject);
    //             break;
    //         case "FrenzyBeaconTemplate":
    //             o = new FrenzyBeaconTemplate(jObject);
    //             break;
    //         case "RallyBeaconTemplate":
    //             o = new RallyBeaconTemplate(jObject);
    //             break;
    //         case "RepairBeaconTemplate":
    //             o = new RepairBeaconTemplate(jObject);
    //             break;
    //         case "SpawnBoostBeaconTemplate":
    //             o = new SpawnBoostBeaconTemplate(jObject);
    //             break;
    //         case "LightningBoltTemplate":
    //             o = new LightningBoltTemplate(jObject);
    //             break;
    //         case "MortarShellTemplate":
    //             o = new MortarShellTemplate(jObject);
    //             break;
    //         case "ProjectileTemplate":
    //             o = new ProjectileTemplate(jObject);
    //             break;
    //         case "MinionTemplate":
    //             o = new MinionTemplate(jObject);
    //             break;
    //         case "BroodMinionTemplate":
    //             o = new BroodMinionTemplate(jObject);
    //             break;
    //         case "FlyingMinionTemplate":
    //             o = new FlyingMinionTemplate(jObject);
    //             break;
    //         case "FlyUntilHitMinionTemplate":
    //             o = new FlyUntilHitMinionTemplate(jObject);
    //             break;
    //         case "HeroMinionTemplate":
    //             o = new HeroMinionTemplate(jObject);
    //             break;
    //         case "HopperMinionTemplate":
    //             o = new HopperMinionTemplate(jObject);
    //             break;
    //         case "SapperMinionTemplate":
    //             o = new SapperMinionTemplate(jObject);
    //             break;
    //     }
    //     
    //     if (o is not T)
    //     {
    //         throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
    //         return null;
    //     }
    //     
    //     // return typeof(T).GetConstructor([typeof(JObject)]).Invoke([jObject]) as T;
    //     return o as T;
    // }
    
    // This version survives the trim!
    public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    {
        if (!jObject.ContainsKey("type"))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
            return null;
        }
        
        if (!_jsonAssetTypes.ContainsKey(jObject.Value<string>("type")))
        {
            throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
            return null;
        }
        
        return _jsonAssetTypes[jObject.Value<string>("type")].Invoke([jObject]) as T;
    }
    
    // // This version uses reflection and is incompatible with trimming
    // public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    // {
    //     if (!jObject.ContainsKey("type"))
    //     {
    //         throw new Exception($"Tried to load JsonAsset from JObject with no type. \n{jObject}");
    //         return null;
    //     }
    //
    //     Type? type = typeof(JsonAsset);
    //     type = Type.GetType($"nestphalia.{jObject.Value<string>("type")}");
    //     
    //     if (type == null || !type.IsAssignableTo(typeof(T)))
    //     {
    //         throw new Exception($"Tried to load JsonAsset from JObject with invalid type. \n{jObject}");
    //         return null;
    //     }
    //     
    //     // return typeof(T).GetConstructor([typeof(JObject)]).Invoke([jObject]) as T;
    //     return Activator.CreateInstance(type, jObject) as T;
    // }

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
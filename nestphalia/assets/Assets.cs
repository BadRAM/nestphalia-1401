using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();
    
    public static T? LoadJsonAsset<T>(JObject jObject) where T : JsonAsset
    {
        if (!jObject.ContainsKey("type"))
        {
            Console.WriteLine($"Tried to load JsonAsset from JObject with no type.\nStacktrace:\n{Environment.StackTrace}\nObject:\n{jObject}");
            throw new Exception();
            return null;
        }
        
        Type? type = Type.GetType($"nestphalia.{jObject.Value<string>("type")}");
        
        if (type == null || !type.IsAssignableTo(typeof(T)))
        {
            Console.WriteLine($"Tried to load JsonAsset from JObject with invalid type.\nStacktrace:\n{Environment.StackTrace}\nObject:\n{jObject}");
            throw new Exception();
            return null;
        }
        
        return Activator.CreateInstance(type, jObject) as T;
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
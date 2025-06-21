using System.Numerics;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class Level
{
    public string ID = "";
    public string Name = "";
    public string Description = "";
    public Int2D Location;
    public string[] LeadsTo = new string[0];
    public int Reward;
    public Int2D WorldSize;
    public string[,] FloorTiles = new string[0,0];
    public string[,] Structures = new string[0,0];
    public string Script = "";
    public Color EnemyColor;

    public Level() { }
    
    public Level(JObject jObject)
    {
        LoadValues(jObject);
    }

    public void LoadValues(JObject jObject)
    {
        ID = jObject.Value<string>("ID") ?? ID;
        Name = jObject.Value<string>("Name") ?? Name;
        Description = jObject.Value<string>("Description") ?? Description;
        Location = jObject.Value<JObject>("Location")?.ToObject<Int2D?>() ?? Location;
        LeadsTo = jObject.Value<JArray>("LeadsTo")?.ToObject<string[]>() ?? LeadsTo;
        Reward = jObject.Value<int?>("Reward") ?? Reward;
        WorldSize = jObject.Value<JObject>("WorldSize")?.ToObject<Int2D?>() ?? WorldSize;
        FloorTiles = jObject.Value<JArray>("FloorTiles")?.ToObject<string[,]>() ?? FloorTiles;
        Structures = jObject.Value<JArray>("Structures")?.ToObject<string[,]>() ?? Structures;
        Script = jObject.Value<string>("Script") ?? Script;
        EnemyColor = jObject.Value<JObject>("Location")?.ToObject<Color?>() ?? EnemyColor;
    }
    
    
    
    
}
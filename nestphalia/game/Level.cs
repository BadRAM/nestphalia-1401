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
    public Color EnemyColor = Color.Red;
    public Int2D Location;
    public string[] LeadsTo = [];
    public int MoneyReward;
    public string[] UnlockReward = [];
    public Int2D PlayerOffset = new Int2D(1,1); // The player fort is offset by this amount.
    public int PlayerRotation; // The player fort is rotated 90 degrees clockwise this many times
    public string Script = "";
    public Int2D WorldSize = new Int2D(World.BoardWidth,World.BoardHeight);
    public string[,] FloorTiles = new string[World.BoardWidth,World.BoardHeight];
    public string[,] Structures = new string[World.BoardWidth,World.BoardHeight];

    public Level()
    {
        for (int x = 0; x < WorldSize.X; x++)
        for (int y = 0; y < WorldSize.Y; y++)
        {
            FloorTiles[x, y] = "floor_1";
        }
    }

    public Level(string jsonString)
    {
        LoadValues(jsonString);
    }

    public void LoadValues(string jsonString)
    {
        JObject j = JObject.Parse(jsonString);
        
        ID = j.Value<string>("ID") ?? ID;
        Name = j.Value<string>("Name") ?? Name;
        Description = j.Value<string>("Description") ?? Description;
        EnemyColor = j.Value<JObject>("EnemyColor")?.ToObject<Color?>() ?? EnemyColor;
        Location = j.Value<JObject>("Location")?.ToObject<Int2D?>() ?? Location;
        LeadsTo = j.Value<JArray>("LeadsTo")?.ToObject<string[]>() ?? LeadsTo;
        MoneyReward = j.Value<int?>("MoneyReward") ?? MoneyReward;
        UnlockReward = j.Value<JArray>("UnlockReward")?.ToObject<string[]>() ?? UnlockReward;
        PlayerOffset = j.Value<JObject?>("PlayerOffset")?.ToObject<Int2D?>() ?? PlayerOffset;
        PlayerRotation = j.Value<int?>("PlayerRotation") ?? PlayerRotation;
        Script = j.Value<string>("Script") ?? Script;
        WorldSize = j.Value<JObject>("WorldSize")?.ToObject<Int2D?>() ?? WorldSize;
        FloorTiles = j.Value<JArray>("FloorTiles")?.ToObject<string[,]>() ?? FloorTiles;
        Structures = j.Value<JArray>("Structures")?.ToObject<string[,]>() ?? Structures;
    }

    public void LoadToBoard()
    {
        for (int x = 0; x < World.BoardWidth; x++)
        for (int y = 0; y < World.BoardHeight; y++)
        {
            World.SetFloorTile(Assets.GetFloorTileByID(FloorTiles[x,y]),x,y);
            World.SetTile(Assets.GetStructureByID(Structures[x,y]),World.RightTeam,x,y);
        }
    }
}
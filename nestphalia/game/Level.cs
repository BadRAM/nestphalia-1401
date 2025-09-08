using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class Level : JsonAsset
{
    public string Name = "";
    public string Description = "";
    public Color EnemyColor = Color.Red;
    public Color GradientTop = Color.Black;
    public Color GradientBottom = Color.Black;
    public Int2D Location;
    public Int2D SoilTexture;
    public string[] Prerequisites = [];
    public int MoneyReward;
    public string[] UnlockReward = [];
    public FortSpawnZone[] FortSpawnZones = [new FortSpawnZone(4, 4, 0, false)];
    public Int2D WorldSize = new Int2D(World.BoardWidth,World.BoardHeight);
    public string Music = "";
    public string Weather = "";
    public string Script = "";
    public string[,] FloorTiles;
    public string[,] Structures;

    public Level(JObject jObject) : base(jObject)
    {
        Type = "Level";
        Name = jObject.Value<string>("Name") ?? Name;
        Description = jObject.Value<string>("Description") ?? Description;
        EnemyColor = jObject.Value<JObject>("EnemyColor")?.ToObject<Color?>() ?? EnemyColor;
        GradientTop = jObject.Value<JObject>("GradientTop")?.ToObject<Color?>() ?? GradientTop;
        GradientBottom = jObject.Value<JObject>("GradientBottom")?.ToObject<Color?>() ?? GradientBottom;
        Location = jObject.Value<JObject>("Location")?.ToObject<Int2D?>() ?? Location;
        SoilTexture = jObject.Value<JObject>("Location")?.ToObject<Int2D?>() ?? SoilTexture;
        Prerequisites = jObject.Value<JArray>("Prerequisites")?.ToObject<string[]>() ?? Prerequisites;
        MoneyReward = jObject.Value<int?>("MoneyReward") ?? MoneyReward;
        UnlockReward = jObject.Value<JArray>("UnlockReward")?.ToObject<string[]>() ?? UnlockReward;
        FortSpawnZones = jObject.Value<JArray>("FortSpawnZones")?.ToObject<FortSpawnZone[]>() ?? FortSpawnZones;
        WorldSize = jObject.Value<JObject>("WorldSize")?.ToObject<Int2D?>() ?? WorldSize;
        Music = jObject.Value<string>("Music") ?? Music;
        Weather = jObject.Value<string>("Weather") ?? Weather;
        Script = jObject.Value<string>("Script") ?? Script;
        FloorTiles = jObject.Value<JArray>("FloorTiles")?.ToObject<string[,]>() ?? new string[WorldSize.X,WorldSize.Y];
        Structures = jObject.Value<JArray>("Structures")?.ToObject<string[,]>() ?? new string[WorldSize.X,WorldSize.Y];
        
        // Check that arrays match board size
        if (FloorTiles.GetLength(0) != WorldSize.X || FloorTiles.GetLength(1) != WorldSize.Y || 
            Structures.GetLength(0) != WorldSize.X || Structures.GetLength(1) != WorldSize.Y)
        {
            string[,] newFloors = new string[WorldSize.X, WorldSize.Y];
            string[,] newStruct = new string[WorldSize.X, WorldSize.Y];
            for (int x = 0; x < Math.Min(FloorTiles.GetLength(0), WorldSize.X); x++)
            for (int y = 0; y < Math.Min(FloorTiles.GetLength(1), WorldSize.Y); y++)
            {
                newFloors[x, y] = FloorTiles[x, y];
                newStruct[x, y] = Structures[x, y];
            }
            FloorTiles = newFloors;
            Structures = newStruct;

            GameConsole.WriteLine($"{ID} had incorrect board size and was padded/cropped accordingly.");
        }
    }

    public void LoadFromBoard()
    {
        for (int x = 0; x < WorldSize.X; x++)
        for (int y = 0; y < WorldSize.Y; y++)
        {
            FloorTiles[x, y] = World.GetFloorTile(x, y).Template.ID;
            Structures[x, y] = World.GetTile(x, y)?.Template.ID ?? "";
        }
    }

    public void LoadToBoard()
    {
        for (int x = 0; x < WorldSize.X; x++)
        for (int y = 0; y < WorldSize.Y; y++)
        {
            World.SetFloorTile(Assets.Get<FloorTileTemplate>(FloorTiles[x,y]), x, y);
            if (!Assets.Exists<StructureTemplate>(Structures[x,y])) continue;
            World.SetTile(Assets.Get<StructureTemplate>(Structures[x,y]),World.RightTeam,x,y);
        }
    }
    
    // TODO: This struct should live somewhere else.
    public struct FortSpawnZone(int X, int Y, int Rotations, bool Flip)
    {
        public int X;
        public int Y;
        public int Rotations;
        public bool Flip;

        public Int2D Offset()
        {
            return new Int2D(X, Y);
        }
    }
}
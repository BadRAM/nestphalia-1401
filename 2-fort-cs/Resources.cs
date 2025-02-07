using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace _2_fort_cs;

public static class Resources
{
    public static Texture2D wabbit;
    public static Texture2D wall;
    public static Texture2D floor1;
    public static Texture2D floor2;
    public static Texture2D bullet;
    public static Texture2D turret;
    public static Texture2D spawner;
    
    public static void Load()
    {
        wabbit = LoadTexture("resources/wabbit_alpha.png");
        wall   = LoadTexture("resources/wall.png");
        floor1 = LoadTexture("resources/floor1.png");
        floor2 = LoadTexture("resources/floor2.png");  
        bullet = LoadTexture("resources/bullet.png");  
        turret = LoadTexture("resources/turret.png");  
        spawner = LoadTexture("resources/spawner.png");
    }

    public static void Unload()
    {
        UnloadTexture(wabbit);
        UnloadTexture(wall);
        UnloadTexture(floor1);
        UnloadTexture(floor2);
        UnloadTexture(bullet);
        UnloadTexture(turret);
    }

    public class Fort
    {
        [JsonInclude] public string Name = "Fort";
        [JsonInclude] public string Comment = "It's a fort!";
        [JsonInclude] public string[] Board = new string[20*20];
    }

    public static void SaveFort()
    {
        //if (right) World.Flip();
        Fort fort = new Fort();

        Directory.GetCurrentDirectory();
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                fort.Board[x+y*20] = World.GetTile(x+1,y+1).Template.Name;
            }
        }
        
        string jsonString = JsonSerializer.Serialize(fort);
        Console.WriteLine($"JSON fort looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/fort.json", jsonString);
        
        //if (right) World.Flip();
    }

    public static void LoadFort()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/fort.json");
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString);
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                World.SetTile(Assets.GetTileByName(fort.Board[x+y*20]), x+1,y+1);
            }
        }
    }
}
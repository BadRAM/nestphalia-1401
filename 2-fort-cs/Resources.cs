using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class Resources
{
    public static Texture wabbit;
    public static Texture wall;
    public static Texture floor1;
    public static Texture floor2;
    public static Texture bullet;
    public static Texture turret;
    public static Texture spawner;
    
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

    public static Fort LoadFort()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/fort.json");
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString);
        return fort;
    }
}
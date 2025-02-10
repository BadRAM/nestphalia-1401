using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class Resources
{
    public static Texture wabbit;
    public static Texture ant;
    public static Texture smant;
    public static Texture snail;
    public static Texture bee;
    public static Texture beetle;
    public static Texture wall;
    public static Texture wall2;
    public static Texture doorOpen;
    public static Texture doorClosed;
    public static Texture floor1;
    public static Texture floor2;
    public static Texture bullet;
    public static Texture turret;
    public static Texture spawner;
    public static Texture flag1;
    public static Texture flag2;
    public static Texture blank;
    public static List<Fort> CampaignLevels = new List<Fort>();
    
    public static void Load()
    {
        wabbit  = LoadTexture("resources/wabbit_alpha.png");
        ant     = LoadTexture("resources/ant.png");
        smant   = LoadTexture("resources/smant.png");
        snail   = LoadTexture("resources/snail.png");
        bee     = LoadTexture("resources/bee.png");
        beetle  = LoadTexture("resources/beetle.png");
        wall    = LoadTexture("resources/wall.png");
        wall2   = LoadTexture("resources/wall2.png");
        doorClosed = LoadTexture("resources/doorClosed.png");
        doorOpen = LoadTexture("resources/doorOpen.png");
        floor1  = LoadTexture("resources/floor1.png");
        floor2  = LoadTexture("resources/floor2.png");
        bullet  = LoadTexture("resources/bullet.png");
        turret  = LoadTexture("resources/turret.png");
        spawner = LoadTexture("resources/spawner.png");
        flag1   = LoadTexture("resources/flag1.png");
        flag2   = LoadTexture("resources/flag2.png");
        blank   = LoadTexture("resources/clear.png");
        
        CampaignLevels.Add(LoadFort("/resources/level1.fort"));
        CampaignLevels.Add(LoadFort("/resources/level2.fort"));
        CampaignLevels.Add(LoadFort("/resources/level3.fort"));
        CampaignLevels.Add(LoadFort("/resources/level4.fort"));
        CampaignLevels.Add(LoadFort("/resources/level5.fort"));
        CampaignLevels.Add(LoadFort("/resources/level6.fort"));
        CampaignLevels.Add(LoadFort("/resources/level7.fort"));
        CampaignLevels.Add(LoadFort("/resources/level8.fort"));
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

    public static void SaveFort(string fortName)
    {
        //if (right) World.Flip();
        Fort fort = new Fort();
        fort.Name = fortName;

        Directory.GetCurrentDirectory();
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                fort.Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.Name ?? "";
            }
        }
        
        string jsonString = JsonSerializer.Serialize(fort);
        Console.WriteLine($"JSON fort looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + $"/{fortName}.fort", jsonString);
        
        //if (right) World.Flip();
    }

    public static Fort LoadFort(string path)
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + path);
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString);
        return fort;
    }
}
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
}
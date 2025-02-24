using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class Screen
{
    public static int MinWidth = 1200;
    public static int MinHeight = 600;
    
    public static int HCenter;
    public static int VCenter;
    public static int Left;
    public static int Right = 0;
    public static int Top = 0;
    public static int Bottom;
    //public static Image WhiteNoise = Raylib.GenImageWhiteNoise(1024, 1024, 0.5f);
    public static List<List<bool>> backgroundNoise = new List<List<bool>>();
    public static Texture Tile1;
    public static Texture Tile2;

    public static void Initialize()
    {
        //WhiteNoise = Raylib.GenImageWhiteNoise(1024, 1024, 0.5f);
        Tile1 = Resources.GetTextureByName("floor1");
        Tile2 = Resources.GetTextureByName("floor2");
    }

    public static void RegenerateBackground()
    {
        backgroundNoise.Clear();
        
        while (backgroundNoise.Count <= Left/24)
        {
            backgroundNoise.Add(new List<bool>());
        }

        foreach (List<bool> row in backgroundNoise)
        {
            while (row.Count <= Bottom/24)
            {
                row.Add(Random.Shared.Next(2) == 0);
            }
        }
    }
    
    public static void UpdateBounds()
    {
        Left = Raylib.GetScreenWidth();
        Bottom = Raylib.GetScreenHeight();

        if (Left < MinWidth || Bottom < MinHeight)
        {
            Raylib.SetWindowSize(Math.Max(Left, MinWidth), Math.Max(Bottom, MinHeight));
            Left = Raylib.GetScreenWidth();
            Bottom = Raylib.GetScreenHeight();
        }
        
        HCenter = Left / 2;
        VCenter = Bottom / 2;

        RegenerateBackground();
    }

    public static void DrawBackground(Color tint)
    {
        for (int x = 0; x <= Left/24; x++)
        {
            for (int y = 0; y <= Bottom/24; y++)
            {
                Raylib.DrawTexture(backgroundNoise[x][y] ? Tile1 : Tile2, x * 24, y * 24 - 12, tint);
            }
        }
        Raylib.DrawRectangle(HCenter - 600, VCenter - 300, 1200, 600, new Color(10, 10, 10, 64));
    }
}
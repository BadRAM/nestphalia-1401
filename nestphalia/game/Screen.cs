using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

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
    private static List<List<bool>> _backgroundNoise = new List<List<bool>>();
    private static Texture2D _tile1;
    private static Texture2D _tile2;
    private static List<Texture2D> _graffiti = new List<Texture2D>();
    private static int _graffitiPosX;
    private static int _graffitiPosY;
    private static int _graffitiPicked;

    public static void Initialize()
    {
        ConfigFlags flags = ConfigFlags.ResizableWindow;
        if (Settings.Saved.WindowScale) flags |= ConfigFlags.HighDpiWindow;

        SetConfigFlags(flags);
        InitWindow(1200, 600, "2-fort");
        SetWindowMinSize((int)(GetWindowScaleDPI().X * 1200), (int)(GetWindowScaleDPI().Y * 600));
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);
        SetMouseScale(1, 1);
        UpdateBounds();
    }

    public static void Load()
    {
        _tile1 = Resources.GetTextureByName("floor1");
        _tile2 = Resources.GetTextureByName("floor2");
        
        _graffiti.Add(Resources.GetTextureByName("tag_badram_isopod"));
        _graffiti.Add(Resources.GetTextureByName("tag_badram_kilroy"));
        _graffiti.Add(Resources.GetTextureByName("tag_badram_rune"));
        _graffiti.Add(Resources.GetTextureByName("tag_badram_s"));
        _graffiti.Add(Resources.GetTextureByName("tag_badram_trogdor"));
        _graffiti.Add(Resources.GetTextureByName("tag_badram_triangle"));
        _graffiti.Add(Resources.GetTextureByName("tag_blip"));
        _graffiti.Add(Resources.GetTextureByName("tag_chartuch"));
        _graffiti.Add(Resources.GetTextureByName("tag_fruitility"));
        _graffiti.Add(Resources.GetTextureByName("tag_gnarwhal"));
        _graffiti.Add(Resources.GetTextureByName("tag_kilroy_xiii"));
        _graffiti.Add(Resources.GetTextureByName("tag_oldog"));
        _graffiti.Add(Resources.GetTextureByName("tag_paulby"));
        _graffiti.Add(Resources.GetTextureByName("tag_professorlucario"));
        _graffiti.Add(Resources.GetTextureByName("tag_rune"));
        _graffiti.Add(Resources.GetTextureByName("tag_sea"));
        _graffiti.Add(Resources.GetTextureByName("tag_sonicproof"));
    }

    public static void RegenerateBackground()
    {
        _backgroundNoise.Clear();
        
        while (_backgroundNoise.Count <= Left/24)
        {
            _backgroundNoise.Add(new List<bool>());
        }

        foreach (List<bool> row in _backgroundNoise)
        {
            while (row.Count <= Bottom/24)
            {
                row.Add(Random.Shared.Next(2) == 0);
            }
        }

        _graffitiPicked = Random.Shared.Next(_graffiti.Count);

        while (true)
        {
            _graffitiPosX = Random.Shared.Next(HCenter - 1000, HCenter + 1000);
            _graffitiPosY = Random.Shared.Next(VCenter - 600, VCenter + 600);

            if ((_graffitiPosX < HCenter - 664 || _graffitiPosX > HCenter + 600) && (_graffitiPosY < VCenter - 364 || _graffitiPosY > VCenter + 300) )
            {
                break;
            }
        }
    }
    
    public static void UpdateBounds()
    {
        Console.WriteLine($"WindowScale = {GUI.GetWindowScale()}");

        Vector2 scale = GUI.GetWindowScale();
        Console.WriteLine($"Scale is X:{scale.X},Y:{scale.Y}");
        
        Left = (int)(GetScreenWidth() / scale.X);
        Bottom = (int)(GetScreenHeight() / scale.Y);

        if (Left < MinWidth || Bottom < MinHeight)
        {
            SetWindowSize(Math.Max(Left, MinWidth), Math.Max(Bottom, MinHeight));
            Left = GetScreenWidth();
            Bottom = GetScreenHeight();
        }
        
        HCenter = Left / 2;
        VCenter = Bottom / 2;

        RegenerateBackground();
        
        SetWindowMinSize((int)(GetWindowScaleDPI().X * 1200), (int)(Raylib.GetWindowScaleDPI().Y * 600));
    }

    public static void DrawBackground(Color tint)
    {
        for (int x = 0; x <= Left/24; x++)
        {
            for (int y = 0; y <= Bottom/24; y++)
            {
                DrawTexture(_backgroundNoise[x][y] ? _tile1 : _tile2, x * 24, y * 24 - 12, tint);
            }
        }
        DrawRectangle(HCenter - 600, VCenter - 300, 1200, 600, new Color(10, 10, 10, 64));
        DrawTexture(_graffiti[_graffitiPicked], _graffitiPosX, _graffitiPosY, Color.White);
    }
}
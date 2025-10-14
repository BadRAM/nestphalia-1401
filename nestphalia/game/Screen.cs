using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class Screen
{
    public static int MinWidth = 960;
    public static int MinHeight = 720;    
    
    public static int CenterX;
    public static int CenterY;
    public static int LeftX = 0;
    public static int RightX;
    public static int TopY = 0;
    public static int BottomY;
    public static Vector2 TopLeft;
    public static Vector2 Top;
    public static Vector2 TopRight;
    public static Vector2 Left;
    public static Vector2 Center;
    public static Vector2 Right;
    public static Vector2 BottomLeft;
    public static Vector2 Bottom;
    public static Vector2 BottomRight;

    public static bool DebugMode;

    //public static Image WhiteNoise = Raylib.GenImageWhiteNoise(1024, 1024, 0.5f);
    private static List<List<bool>> _backgroundNoise = new List<List<bool>>();
    private static Texture2D _tile1;
    private static Texture2D _tile2;
    private static List<Texture2D> _graffiti = new List<Texture2D>();
    private static int _graffitiPosX;
    private static int _graffitiPosY;
    private static int _graffitiPicked;

    private static Camera2D _activeCamera;
    private static Camera2D _screenCamera = new Camera2D();
    
    public static void Initialize()
    {
        ConfigFlags flags = ConfigFlags.ResizableWindow;
        // if (Settings.Saved.WindowScale) flags |= ConfigFlags.HighDpiWindow;

        SetConfigFlags(flags);
        Vector2 scaleDpi = (float)Settings.Saved.WindowScale * Vector2.One;
        
        InitWindow(MinWidth, MinHeight, "Nestphalia 1401");
        SetTargetFPS(Time.FrameRate);
        SetExitKey(KeyboardKey.Null);
        BeginMode2D(_screenCamera);
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
        
        while (_backgroundNoise.Count <= RightX/24)
        {
            _backgroundNoise.Add(new List<bool>());
        }

        foreach (List<bool> row in _backgroundNoise)
        {
            while (row.Count <= BottomY/24)
            {
                row.Add(Random.Shared.Next(2) == 0);
            }
        }

        _graffitiPicked = Random.Shared.Next(_graffiti.Count);

        while (true)
        {
            _graffitiPosX = Random.Shared.Next(CenterX - 1000, CenterX + 1000);
            _graffitiPosY = Random.Shared.Next(CenterY - 600, CenterY + 600);

            if ((_graffitiPosX < CenterX - 664 || _graffitiPosX > CenterX + 600) && (_graffitiPosY < CenterY - 364 || _graffitiPosY > CenterY + 300) )
            {
                break;
            }
        }
    }
    
    public static void UpdateBounds(Vector2? windowSize = null)
    {
        GameConsole.WriteLine($"WindowScale = {GUI.GetWindowScale()}");

        Vector2 scale = GUI.GetWindowScale();
        GameConsole.WriteLine($"Scale is X:{scale.X},Y:{scale.Y}");
        SetWindowMinSize((int)(scale.X * MinWidth), (int)(scale.Y * MinHeight));
        _screenCamera.Zoom = (float)Settings.Saved.WindowScale;
        
        if (windowSize != null && !IsWindowMaximized() && !IsWindowFullscreen())
        {
            SetWindowSize((int)windowSize.Value.X, (int)windowSize.Value.Y);
        }
        
        RightX = (int)(GetScreenWidth() / scale.X);
        BottomY = (int)(GetScreenHeight() / scale.Y);
        
        CenterX = RightX / 2;
        CenterY = BottomY / 2;
        
        TopLeft = new Vector2(LeftX, TopY);
        Top = new Vector2(CenterX, TopY);
        TopRight = new Vector2(RightX, TopY);
        Left = new Vector2(LeftX, CenterY);
        Center = new Vector2(CenterX, CenterY);
        Right = new Vector2(RightX, CenterY);
        BottomLeft = new Vector2(LeftX, BottomY);
        Bottom = new Vector2(CenterX, BottomY);
        BottomRight = new Vector2(RightX, BottomY);

        RegenerateBackground();
    }

    public static void DrawBackground(Color tint)
    {
        SetCamera();
        
        for (int x = 0; x <= RightX/24; x++)
        for (int y = 0; y <= BottomY/24; y++)
        {
            DrawTexture(_backgroundNoise[x][y] ? _tile1 : _tile2, x * 24, y * 24, tint);
        }
        DrawRectangle(CenterX - MinWidth/2, CenterY - MinHeight/2, MinWidth, MinHeight, new Color(10, 10, 10, 64));
        DrawTexture(_graffiti[_graffitiPicked], _graffitiPosX, _graffitiPosY, Color.White);
    }

    public static void DrawBackgroundTex(Texture2D tex, Color tint)
    {
        for (int x = 0; x <= RightX/tex.Width; x++)
        for (int y = 0; y <= BottomY/tex.Height; y++)
        {
            DrawTexture(tex, x * tex.Width, y * tex.Height, tint);
        }
    }

    public static void SetCamera(Camera2D? camera = null)
    {
        EndMode2D();
        _activeCamera = camera ?? _screenCamera;
        BeginMode2D(_activeCamera);
    }

    public static void BeginDrawing()
    {
        Raylib.BeginDrawing();
        SetCamera();
        if (Input.Pressed(Input.InputAction.Debug)) DebugMode = !DebugMode;
    }

    public static void EndDrawing()
    {
        if (DebugMode && Input.Held(KeyboardKey.LeftShift))
        {
            Vector2 mPos = GUI.GetScaledMousePosition() - Center;
            GUI.DrawTextLeft((int)mPos.X + 10, (int)mPos.Y, $"mPos: {mPos}\n" +
                                                            $"Abs: {GUI.GetScaledMousePosition()}\n" +
                                                            $"Unscaled: {GetMousePosition()}\n" +
                                                            $"World: {GetScreenToWorld2D(GetMousePosition(), World.Camera)}\n" +
                                                            $"World Tile: {World.GetMouseTilePos()}");
        }
        Raylib.EndDrawing();
    }
}
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
    }
}
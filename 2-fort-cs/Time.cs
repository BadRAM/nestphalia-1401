using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class Time
{
    public static double Scaled;
    public static double Unscaled;
    public static double Real;
    // annoying that this is a constant but I can't think of a better way >:(
    public static double DeltaTime = 1f/60f;
    public static double TimeScale = 1;
    
    public static void UpdateTime()
    {
        Unscaled += DeltaTime;
        Scaled += DeltaTime * TimeScale;
        Real = Raylib.GetTime();
    }
}
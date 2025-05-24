using Raylib_cs;

namespace nestphalia;

public static class Time
{
    public static double Scaled;
    public static double Unscaled;
    public static double Real;
    public const double DeltaTime = 1.0/60.0;
    public static double TimeScale = 1;
    
    public static void UpdateTime()
    {
        Unscaled += DeltaTime;
        Scaled += DeltaTime * TimeScale;
        Real = Raylib.GetTime();
    }

    public static void Reset()
    {
        Scaled = 0;
        Unscaled = 0;
    }
}
using Raylib_cs;

namespace nestphalia;

public static class Time
{
    public static double Scaled;
    public static double Unscaled;
    public static int Tick;
    public static double Real; // Seconds since program started
    public const int FrameRate = 60;
    public const double DeltaTime = 1.0/FrameRate;
    public static bool Paused;
    
    public static void UpdateTime(bool fastForward = false)
    {
        if (!fastForward) Unscaled += DeltaTime;
        if (!Paused)
        {
            Tick++;
            Scaled = Tick * DeltaTime;
        }
        Real = Raylib.GetTime();
    }

    public static void Reset()
    {
        Scaled = 0;
        Unscaled = 0;
        Tick = 0;
    }
}
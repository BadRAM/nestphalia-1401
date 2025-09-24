using Raylib_cs;

namespace nestphalia;

public static class Time
{
    public static double Scaled;
    public static double Unscaled;
    public static int Tick;
    private static double _tickDelta;
    public static double Real;
    public const double DeltaTime = 1.0/60.0;
    public static double TimeScale = 1;
    
    public static void UpdateTime(bool fastForward = false)
    {
        if (!fastForward) Unscaled += DeltaTime;
        Scaled += DeltaTime * TimeScale;
        Real = Raylib.GetTime();
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (TimeScale == 1)
        {
            Tick++;
        }
        else
        {
            _tickDelta += DeltaTime * TimeScale;
            if (_tickDelta > DeltaTime)
            {
                _tickDelta -= DeltaTime;
                Tick++;
            }
        }
    }

    public static void Reset()
    {
        Scaled = 0;
        Unscaled = 0;
        Tick = 0;
    }
}
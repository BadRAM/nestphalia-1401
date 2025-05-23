using System.Diagnostics;

namespace nestphalia;

// This is a watchdog that verifies determinism
public static class Determinator
{
    public static string battleName;
    public static string Stacks = "";
    private static List<int> _destiny = new List<int>();
    private static int _frame;
    // public static bool Fated;
    public static FateModes Fate;
    
    public enum FateModes
    {
        Inactive,
        Learning,
        Guarding
    }

    public static void Start(bool deterministic)
    {
        _frame = 0;
        if (deterministic)
        {
            if (battleName == $"recording {World.LeftTeam.Name} vs {World.RightTeam.Name}...")
            {
                Fate = FateModes.Guarding;
            }
            else
            {
                Fate = FateModes.Learning;
                battleName = $"recording {World.LeftTeam.Name} vs {World.RightTeam.Name}...";
                _destiny.Clear();
            }
        }
        else
        {
            Fate = FateModes.Inactive;
            battleName = $"not binding";
            _destiny.Clear();
        }
    }
    
    public static void Update()
    {
#if DEBUG
        switch (Fate)
        {
            case FateModes.Learning:
                _destiny.Add(World.RandomInt(100));
                break;
            case FateModes.Guarding:
                int i = World.RandomInt(100);
                if (i == _destiny[_frame])
                {
                    //Console.WriteLine($"Determinator: {i} == {_destiny[_frame]}");
                }
                else
                {
                    Console.WriteLine($"Nondeterminism detected! Stacks for this update:\n{Stacks}");
                }
                Debug.Assert(i == _destiny[_frame]);
                break;
        }

        _frame++;
        Stacks = "";
#endif
    }
}
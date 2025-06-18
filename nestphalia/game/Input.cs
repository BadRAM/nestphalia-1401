using Raylib_cs;

namespace nestphalia;

public static class Input
{
    public static bool Suppressed;
    public static Dictionary<Action, KeyboardKey> Bindings = new Dictionary<Action, KeyboardKey>()
    {
        { Action.Exit,            KeyboardKey.Escape},
        { Action.Pause,           KeyboardKey.P },
        { Action.FrameStep,       KeyboardKey.O },
        { Action.FastForward,     KeyboardKey.F },
        { Action.CameraUp,        KeyboardKey.W },
        { Action.CameraDown,      KeyboardKey.S },
        { Action.CameraLeft,      KeyboardKey.A },
        { Action.CameraRight,     KeyboardKey.D },
        { Action.Debug,           KeyboardKey.F3 },
        { Action.PathDebug,       KeyboardKey.Q },
        { Action.AdvanceDialogue, KeyboardKey.T },
        { Action.Use1,            KeyboardKey.One },
        { Action.Use2,            KeyboardKey.Two },
        { Action.Use3,            KeyboardKey.Three },
        { Action.Use4,            KeyboardKey.Four },
        { Action.Use5,            KeyboardKey.Five },
        { Action.Use6,            KeyboardKey.Six },
        { Action.Use7,            KeyboardKey.Seven },
        { Action.Use8,            KeyboardKey.Eight },
        { Action.Use9,            KeyboardKey.Nine },
        { Action.Use0,            KeyboardKey.Zero },
    };
    
    public enum Action
    {
        Exit,
        Pause,
        FrameStep,
        FastForward,
        CameraUp,
        CameraDown,
        CameraLeft,
        CameraRight,
        Debug,
        PathDebug,
        AdvanceDialogue,
        Use1,
        Use2,
        Use3,
        Use4,
        Use5,
        Use6,
        Use7,
        Use8,
        Use9,
        Use0
    }
    
    public static bool Held(KeyboardKey key)
    {
        if (Suppressed) return false;
        return Raylib.IsKeyDown(key);
    }
    
    public static bool Held(Action action)
    {
        if (Suppressed || !Bindings.ContainsKey(action)) return false;
        return Raylib.IsKeyDown(Bindings[action]);
    }
    
    public static bool Pressed(KeyboardKey key)
    {
        if (Suppressed) return false;
        return Raylib.IsKeyPressed(key);
    }
    
    public static bool Pressed(Action action)
    {
        if (Suppressed || !Bindings.ContainsKey(action)) return false;
        return Raylib.IsKeyPressed(Bindings[action]);
    }
    
    public static bool Released(KeyboardKey key)
    {
        if (Suppressed) return false;
        return Raylib.IsKeyReleased(key);
    }
    
    public static bool Released(Action action)
    {
        if (Suppressed || !Bindings.ContainsKey(action)) return false;
        return Raylib.IsKeyReleased(Bindings[action]);
    }
}
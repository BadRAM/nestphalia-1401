using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public static class Input
{
    private static bool _suppressed;
    private static Vector2 _clickStartPos;
    
    public static Dictionary<InputAction, KeyboardKey> Bindings = new Dictionary<InputAction, KeyboardKey>()
    {
        { InputAction.Exit,            KeyboardKey.Escape},
        { InputAction.Pause,           KeyboardKey.P },
        { InputAction.FrameStep,       KeyboardKey.O },
        { InputAction.FastForward,     KeyboardKey.F },
        { InputAction.CameraUp,        KeyboardKey.W },
        { InputAction.CameraDown,      KeyboardKey.S },
        { InputAction.CameraLeft,      KeyboardKey.A },
        { InputAction.CameraRight,     KeyboardKey.D },
        { InputAction.Debug,           KeyboardKey.F3 },
        { InputAction.PathDebug,       KeyboardKey.Q },
        { InputAction.AdvanceDialogue, KeyboardKey.T },
        { InputAction.Use1,            KeyboardKey.One },
        { InputAction.Use2,            KeyboardKey.Two },
        { InputAction.Use3,            KeyboardKey.Three },
        { InputAction.Use4,            KeyboardKey.Four },
        { InputAction.Use5,            KeyboardKey.Five },
        { InputAction.Use6,            KeyboardKey.Six },
        { InputAction.Use7,            KeyboardKey.Seven },
        { InputAction.Use8,            KeyboardKey.Eight },
        { InputAction.Use9,            KeyboardKey.Nine },
        { InputAction.Use0,            KeyboardKey.Zero },
    };
    
    public enum InputAction
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
    
    // Suppression Source Priority is determined by order/value in this enum. Lower numbers (first in the list) have stronger priority.
    public enum SuppressionSource
    {
        GameConsole,
        Popup,
        None
    }

    private static List<SuppressionSource> _suppressionSources = new List<SuppressionSource>(Enum.GetValues(typeof(SuppressionSource)).Cast<SuppressionSource>());
    private static Dictionary<SuppressionSource, bool> _inputLockers = new Dictionary<SuppressionSource, bool>();
    private static SuppressionSource _activeSuppressionSource = SuppressionSource.None;

    static Input()
    {
        foreach (SuppressionSource suppressionSource in _suppressionSources)
        {
            _inputLockers.Add(suppressionSource, false);
        }
    }
    
    public static bool IsSuppressed()
    {
        return _suppressed;
    }

    public static void SetSuppressed(SuppressionSource source, bool value)
    {
        _inputLockers[source] = value;
        UpdateSuppressionFlag();
    }

    public static void StartSuppressionOverride(SuppressionSource source)
    {
        if (_activeSuppressionSource != SuppressionSource.None) throw new Exception("Tried to double override input lock!");
        _activeSuppressionSource = source;
        UpdateSuppressionFlag();
    }

    public static void EndSuppressionOverride()
    {
        _activeSuppressionSource = SuppressionSource.None;
        UpdateSuppressionFlag();
    }

    private static void UpdateSuppressionFlag()
    {
        bool flag = false;
        for (int i = 0; i < _suppressionSources.Count; i++)
        {
            if (_activeSuppressionSource == _suppressionSources[i]) break;
            if (_inputLockers[_suppressionSources[i]])
            {
                flag = true;
                break;
            }
        }
        _suppressed = flag;
    }
    
    public static bool Held(InputAction inputAction)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyDown(Bindings[inputAction]);
    }
    
    public static bool Held(KeyboardKey key)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyDown(key);
    }
    
    public static bool Held(MouseButton mouseButton)
    {
        if (_suppressed) return false;
        return Raylib.IsMouseButtonDown(mouseButton);
    }
    
    public static bool Pressed(InputAction inputAction)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyPressed(Bindings[inputAction]);
    }
    
    public static bool Pressed(KeyboardKey key)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyPressed(key);
    }
    
    public static bool Pressed(MouseButton mouseButton)
    {
        if (_suppressed) return false;
        return Raylib.IsMouseButtonPressed(mouseButton);
    }
    
    public static bool Released(InputAction inputAction)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyReleased(Bindings[inputAction]);
    }
    
    public static bool Released(KeyboardKey key)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyReleased(key);
    }
    
    public static bool Released(MouseButton mouseButton)
    {
        if (_suppressed) return false;
        return Raylib.IsMouseButtonReleased(mouseButton);
    }

    public static bool Repeat(KeyboardKey key)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyPressedRepeat(key);
    }

    public static void Poll()
    {
        // This could support all mouse buttons with a for loop
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            _clickStartPos = Raylib.GetMousePosition();
            // GameConsole.WriteLine($"_clickStartPos set to {_clickStartPos}");
        }
    }

    public static Vector2 GetClickPos()
    {
        return _clickStartPos;
    }

    public static void ResetClickPos()
    {
        _clickStartPos = new Vector2(-100000, -100000);
    }

    public static Vector2 GetScaledClickPos()
    {
        return _clickStartPos / GUI.GetWindowScale();
    }
}
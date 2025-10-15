using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public enum InputAction
{
    Up,
    Down,
    Left,
    Right,
    FineMove,
    Click,
    AltClick,
    Exit,
    Pause,
    FrameStep,
    FastForward,
    CameraUp,
    CameraDown,
    CameraLeft,
    CameraRight,
    Debug,
    QuickLoad,
    Use1,
    Use2,
    Use3,
    Use4,
    Use5,
    Use6,
    Use7,
    Use8,
    Use9,
    Use0,
    ViewClose,
    ViewWide,
}

public static class Input
{
    private static bool _suppressed;
    private static Vector2 _clickStartPos;
    public static bool GamepadMode;
    public static bool VirtualMouseMode;
    private static Vector2 _cursor;
    private static Vector2 _cursorDelta;
    public static Texture2D CursorSprite;
    public static float CursorSpeed = 500;

    private static Dictionary<InputAction, KeyboardKey> _keyboardBindings = new Dictionary<InputAction, KeyboardKey>()
    {
        { InputAction.Up,              KeyboardKey.Up},
        { InputAction.Down,            KeyboardKey.Down},
        { InputAction.Left,            KeyboardKey.Left},
        { InputAction.Right,           KeyboardKey.Right},
        { InputAction.Exit,            KeyboardKey.Escape},
        { InputAction.Pause,           KeyboardKey.P },
        { InputAction.FrameStep,       KeyboardKey.O },
        { InputAction.FastForward,     KeyboardKey.F },
        { InputAction.CameraUp,        KeyboardKey.W },
        { InputAction.CameraDown,      KeyboardKey.S },
        { InputAction.CameraLeft,      KeyboardKey.A },
        { InputAction.CameraRight,     KeyboardKey.D },
        { InputAction.Debug,           KeyboardKey.F3 },
        { InputAction.QuickLoad,       KeyboardKey.T },
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

    public static Dictionary<InputAction, GamepadButton> _gamepadBindings = new Dictionary<InputAction, GamepadButton>()
    {
        { InputAction.Up,          GamepadButton.LeftFaceUp },
        { InputAction.Down,        GamepadButton.LeftFaceDown },
        { InputAction.Left,        GamepadButton.LeftFaceLeft },
        { InputAction.Right,       GamepadButton.LeftFaceRight },
        { InputAction.FineMove,    GamepadButton.LeftTrigger1 },
        { InputAction.Click,       GamepadButton.RightFaceRight },
        { InputAction.AltClick,    GamepadButton.RightFaceDown },
        { InputAction.FastForward, GamepadButton.RightFaceLeft },
        { InputAction.Exit,        GamepadButton.MiddleLeft },
        { InputAction.QuickLoad,   GamepadButton.MiddleRight },
        { InputAction.Pause,       GamepadButton.MiddleRight },
        { InputAction.ViewClose,   GamepadButton.RightTrigger1},
        { InputAction.ViewWide,    GamepadButton.LeftTrigger1},
    };

    // Suppression Source Priority is determined by order/value in this enum. Lower numbers (first in the list) have stronger priority.
    public enum SuppressionSource
    {
        Internal,
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
    
    // Main update function, called once before each frame.
    public static void Poll()
    {
        StartSuppressionOverride(SuppressionSource.Internal);
        if (GamepadMode)
        {
            if (Raylib.GetKeyPressed() != 0 || Pressed(MouseButton.Left) || Pressed(MouseButton.Right))
            {
                SetCursor(GetCursor());
                GamepadMode = false;
            }
        }
        else
        {
            if (Raylib.IsGamepadAvailable(0))
            {
                foreach (var button in _gamepadBindings)
                {
                    if (Raylib.IsGamepadButtonPressed(0, button.Value))
                    {
                        GamepadMode = true;
                    }
                }
            }
        }
        
        if (GamepadMode)
        {
            Vector2 lastCursor = GetCursor();
            float delta = CursorSpeed * (float)Time.DeltaTime * (Held(InputAction.FineMove) ? 0.25f : 1);
            if (Held(InputAction.Right)) _cursor.X += delta;
            if (Held(InputAction.Left))  _cursor.X -= delta;
            if (Held(InputAction.Down))  _cursor.Y += delta;
            if (Held(InputAction.Up))    _cursor.Y -= delta;
            Vector2 joystick = new Vector2(
                Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) * CursorSpeed * (float)Time.DeltaTime,
                Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) * CursorSpeed * (float)Time.DeltaTime);
            if (joystick.Length() > 0.2) _cursor += joystick;
            _cursor.X = Math.Clamp(_cursor.X, 0, Screen.RightX * GUI.GetWindowScale().X);
            _cursor.Y = Math.Clamp(_cursor.Y, 0, Screen.BottomY * GUI.GetWindowScale().Y);
            _cursorDelta = GetCursor() - lastCursor;
        }
        else
        {
            _cursor = Raylib.GetMousePosition();
            _cursorDelta = Raylib.GetMouseDelta();
        }
        
        // This could support all mouse buttons with a for loop
        if (Pressed(InputAction.Click))
        {
            _clickStartPos = GetCursor();
        }
        EndSuppressionOverride();
    }

    public static void Draw()
    {
        Raylib.DrawTextureV(CursorSprite, (_cursor - Vector2.One) / GUI.GetWindowScale(), Color.White);
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

    public static Vector2 GetCursor()
    {
        return new Vector2((int)_cursor.X, (int)_cursor.Y);
    }

    public static Vector2 GetCursorDelta()
    {
        return _cursorDelta;
    }
    
    public static void SetCursor(Vector2 pos)
    {
        _cursor = pos;
        Raylib.SetMousePosition((int)pos.X, (int)pos.Y);
    }
    
    public static bool Held(InputAction inputAction)
    {
        if (_suppressed) return false;
        if (!GamepadMode)
        {
            if (inputAction == InputAction.Click) return Raylib.IsMouseButtonDown(MouseButton.Left);
            if (inputAction == InputAction.AltClick) return Raylib.IsMouseButtonDown(MouseButton.Right);
            if (_keyboardBindings.ContainsKey(inputAction)) return Raylib.IsKeyDown(_keyboardBindings[inputAction]);
        }
        else if (_gamepadBindings.ContainsKey(inputAction)) return Raylib.IsGamepadButtonDown(0, _gamepadBindings[inputAction]);
        return false;
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
    
    public static bool Held(GamepadButton gamepadButton)
    {
        if (_suppressed) return false;
        return Raylib.IsGamepadButtonDown(0, gamepadButton);
    }
    
    public static bool Pressed(InputAction inputAction)
    {
        if (_suppressed) return false;
        if (!GamepadMode)
        {
            if (inputAction == InputAction.Click) return Raylib.IsMouseButtonPressed(MouseButton.Left);
            if (inputAction == InputAction.AltClick) return Raylib.IsMouseButtonPressed(MouseButton.Right);
            if (_keyboardBindings.ContainsKey(inputAction)) return Raylib.IsKeyPressed(_keyboardBindings[inputAction]);
        }
        else if (_gamepadBindings.ContainsKey(inputAction)) 
            return Raylib.IsGamepadButtonPressed(0, _gamepadBindings[inputAction]);
        return false;
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
    
    public static bool Pressed(GamepadButton gamepadButton)
    {
        if (_suppressed) return false;
        return Raylib.IsGamepadButtonPressed(0, gamepadButton);
    }
    
    public static bool Released(InputAction inputAction)
    {
        if (_suppressed) return false;
        if (!GamepadMode)
        {
            if (inputAction == InputAction.Click) return Raylib.IsMouseButtonReleased(MouseButton.Left);
            if (inputAction == InputAction.AltClick) return Raylib.IsMouseButtonReleased(MouseButton.Right);
            if (_keyboardBindings.ContainsKey(inputAction)) return Raylib.IsKeyReleased(_keyboardBindings[inputAction]);
        }
        else if (_gamepadBindings.ContainsKey(inputAction)) return Raylib.IsGamepadButtonReleased(0, _gamepadBindings[inputAction]);
        return false;
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
    
    public static bool Released(GamepadButton gamepadButton)
    {
        if (_suppressed) return false;
        return Raylib.IsGamepadButtonReleased(0, gamepadButton);
    }

    public static bool Repeat(KeyboardKey key)
    {
        if (_suppressed) return false;
        return Raylib.IsKeyPressedRepeat(key);
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
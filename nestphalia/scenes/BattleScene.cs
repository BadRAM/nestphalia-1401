using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class BattleScene
{
    private static Fort _leftFort;
    private static Fort _rightFort;
    private static Team? _winner;
    private static int _skips;
    private static bool _pathFinderDebug;
    private static Action<Team?> _battleOverCallback;
    private static SceneState _state;
    
    private enum SceneState
    {
        StartPending,
        BattleActive,
        BattleFinished,
        Paused
    }
    
    public static void Start(Fort leftFort, Fort rightFort, Action<Team?> battleOverCallback, bool leftIsPlayer = true, bool rightIsPlayer = false, bool deterministic = false)
    {
        _winner = null;
        _state = SceneState.BattleActive;
        
        _leftFort = leftFort;
        _rightFort = rightFort;
        Debug.Assert(_leftFort != null && _rightFort != null);

        _battleOverCallback = battleOverCallback;
        
        Time.TimeScale = 1;
        Program.CurrentScene = Scene.Battle;
        World.InitializeBattle(leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);
        
        Resources.PlayMusicByName("jesper-kyd-highlands");
    }

    public static void Update()
    {
        // ----- INPUT PHASE -----
        HandleInputs();

        // ----- WORLD UPDATE PHASE -----
        if (_state == SceneState.BattleActive)
        {
            UpdateWorld();
            CheckWinner(); // If winner is found, changes state to BattleFinished
        }
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(new Color(16, 8, 4, 255));
        
        World.DrawFloor();
        World.Draw();
        
        if (_pathFinderDebug) PathFinder.DrawDebug();

        switch (_state)
        {
            case SceneState.StartPending:
                break;
            case SceneState.BattleActive:
                if (IsKeyDown(KeyboardKey.F))
                {
                    DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                    DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{_skips+1}X SPEED", 48);
                }
                break;
            case SceneState.BattleFinished:
                DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                DrawTextCentered(Screen.HCenter, Screen.VCenter-48, "BATTLE OVER!", 48);
                DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{_winner.Name} is victorious!", 48);
                if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Return"))
                {
                    SetMasterVolume(1f); // if F is still held, restore full volume.
                    _battleOverCallback(_winner);
                }
                break;
            case SceneState.Paused:
                DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                DrawTextCentered(Screen.HCenter, Screen.VCenter, "PAUSED", 48);
                if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Quit"))
                {
                    _battleOverCallback(null);
                }
                break;
        }
        
        EndDrawing();
    }

    private static void HandleInputs()
    {
        if (IsKeyPressed(KeyboardKey.P) || IsKeyPressed(KeyboardKey.Escape))
        {
            switch (_state)
            {
                case SceneState.BattleActive:
                    _state = SceneState.Paused;
                    Time.TimeScale = 0;
                    break;
                case SceneState.Paused:
                    _state = SceneState.BattleActive;
                    Time.TimeScale = 1;
                    break;
            }
        }
        
        if (IsKeyPressed(KeyboardKey.F))
        {
            SetMasterVolume(0.25f);
        }
        if (IsKeyReleased(KeyboardKey.F))
        {
            SetMasterVolume(1f);
        }

        if (IsKeyDown(KeyboardKey.A))
        {
            World.Camera.Offset.X += 4;
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            World.Camera.Offset.X -= 4;
        }
        if (IsKeyDown(KeyboardKey.W))
        {
            World.Camera.Offset.Y += 4;
        }
        if (IsKeyDown(KeyboardKey.S))
        {
            World.Camera.Offset.Y -= 4;
        }

        if (IsKeyPressed(KeyboardKey.Q))
        {
            _pathFinderDebug = !_pathFinderDebug;
        }
        
        if (IsKeyPressed(KeyboardKey.F3))
        {
            World.DrawDebugInfo = !World.DrawDebugInfo;
        }
        
        if (IsMouseButtonDown(MouseButton.Right))
        {
            World.Camera.Offset += GetMouseDelta();
        }
        
        // Snipped from raylib example
        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            // Get the world point that is under the mouse
            Vector2 mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), World.Camera);

            // Set the offset to where the mouse is
            World.Camera.Offset = GetMousePosition();

            // Set the target to match, so that the camera maps the world space point 
            // under the cursor to the screen space point under the cursor at any zoom
            World.Camera.Target = mouseWorldPos;

            // Zoom increment
            // float scaleFactor = 1.0f + 0.25f;
            // if (wheel < 0) scaleFactor = 1.0f/scaleFactor;
            World.Camera.Zoom = Math.Clamp(World.Camera.Zoom + wheel * 0.5f, 0.5f, 4f);
        }
    }

    // Updates the world, and repeats if FFW is enabled.
    private static void UpdateWorld()
    {
        double startTime = GetTime();
            
        World.Update();

        _skips = 0;
        if (IsKeyDown(KeyboardKey.F))
        {
            while ((GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < 0.016)
            {
                Time.UpdateTime();
                World.Update();
                _skips++;
            }
        }
    }
    
    private static void CheckWinner()
    {
        if (World.LeftTeam.GetHealth() <= 0)
        {
            _winner = World.RightTeam;
            _state = SceneState.BattleFinished;
        }
        if (World.RightTeam.GetHealth() <= 0)
        {
            _winner = World.LeftTeam;
            _state = SceneState.BattleFinished;
        }
    }
    
    
}
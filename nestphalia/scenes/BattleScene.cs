using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class BattleScene : Scene
{
    private Fort _leftFort;
    private Fort _rightFort;
    private Team? _winner;
    private int _skips;
    private bool _pathFinderDebug;
    private Action<Team?> _battleOverCallback;
    private SceneState _state;

    private string _log;
    
    private enum SceneState
    {
        StartPending,
        BattleActive,
        BattleFinished,
        Paused,
        PausedSettings
    }
    
    public void Start(Fort leftFort, Fort rightFort, Action<Team?> battleOverCallback, bool leftIsPlayer = true, bool rightIsPlayer = false, bool deterministic = false)
    {
        _winner = null;
        _state = SceneState.BattleActive;
        
        _leftFort = leftFort;
        _rightFort = rightFort;
        Debug.Assert(_leftFort != null && _rightFort != null);

        _battleOverCallback = battleOverCallback;
        
        Time.TimeScale = 1;
        Program.CurrentScene = this;
        World.InitializeBattle(leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);

        _log += $"first random: {World.RandomInt(100)}\n";
        
        Resources.PlayMusicByName("jesper-kyd-highlands");
    }

    public override void Update()
    {
        // ----- INPUT PHASE -----
        HandleInputs();

        // ----- WORLD UPDATE PHASE -----
        if (_state == SceneState.BattleActive)
        {
            UpdateWorld();
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
                    DrawTextCentered(0, 0, $"{_skips+1}X SPEED", 48);
                }
                break;
            case SceneState.BattleFinished:
                DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                DrawTextCentered(0, -48, "BATTLE OVER!", 48);
                DrawTextCentered(0, 0, $"{_winner.Name} is victorious!", 48);
                if (World.DrawDebugInfo) DrawTextCentered(0, 100, $"{_log}");
                if (Button100(-50, 30, "Return"))
                {
                    SetMasterVolume(1f); // if F is still held, restore full volume.
                    _battleOverCallback(_winner);
                }
                break;
            case SceneState.Paused:
                DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                DrawTextCentered(0, 0, "PAUSED", 48);
                if (Button100(-50, 40, "Settings"))
                {
                    _state = SceneState.PausedSettings;
                }
                if (Button100(-50, 80, "Resume"))
                {
                    _state = SceneState.BattleActive;
                }
                if (Button100(-50, 120, "Quit"))
                {
                    _battleOverCallback(null);
                }
                break;
            case SceneState.PausedSettings:
                DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
                if (Settings.DrawSettingsMenu()) _state = SceneState.Paused;
                break;
        }
        
        EndDrawing();
    }

    private void HandleInputs()
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
        if (_state == SceneState.Paused && IsKeyPressed(KeyboardKey.O)) // Frame step
        {
            Time.TimeScale = 1;
            Time.UpdateTime();
            UpdateWorld();
            Time.TimeScale = 0;
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
    private void UpdateWorld()
    {
        double startTime = GetTime();
        
        World.Update();
        CheckWinner();
        
        _skips = 0;
        if (IsKeyDown(KeyboardKey.F))
        {
            while (_state == SceneState.BattleActive && (GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < 0.016)
            {
                Time.UpdateTime();
                World.Update();
                CheckWinner();
                _skips++;
            }
        }
    }
    
    private void CheckWinner()
    {
        if (World.LeftTeam.GetHealth() <= 0)
        {
            _winner = World.RightTeam;
            _state = SceneState.BattleFinished;
            _log += $"last random: {World.RandomInt(100)}";
        }
        if (World.RightTeam.GetHealth() <= 0)
        {
            _winner = World.LeftTeam;
            _state = SceneState.BattleFinished;
            _log += $"last random: {World.RandomInt(100)}";
        }
    }
    
    
}
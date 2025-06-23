using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class BattleScene : Scene
{
    // private Fort _leftFort;
    // private Fort _rightFort;
    private Team? _winner;
    private int _skips;
    private bool _pathFinderDebug;
    private Action<Team?> _battleOverCallback;
    private SceneState _state;

    private double _zoomLevel = 5;
    private readonly List<Double> _zoomLevels = [0.05, 0.1, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 3.5, 4, 5, 6, 7, 8, 10, 12, 14, 16]; 
    
    // private List<Vector2> CameraShakeQueue = new List<Vector2>();
    private Vector2 _cameraShakeDisplacement = Vector2.Zero;
    private double _cameraShakeIntensity = 6;
    private double _cameraShakeRemaining = 0;
    
    private DialogBox _dialogBox;
    private List<DialogBox> _dialogQueue = new List<DialogBox>()
    {
        // new DialogBox("I Won't let you ruin my perfect perfect plan!         \nSacrifices MUST be made!!!", DialogBox.Mode.Left, Resources.GetTextureByName("kray")),
        // new DialogBox("What about that big opening in the wall?", DialogBox.Mode.Right, Resources.GetTextureByName("knux")),
        // new DialogBox("How could my perfect perfect plan be defeated so easily!", DialogBox.Mode.Both, Resources.GetTextureByName("knux"), Resources.GetTextureByName("kray")),
        // new DialogBox("Kray gave up...")
    };
    
    private string _log;
    
    
    
    private enum SceneState
    {
        StartPending,
        Dialogue,
        BattleActive,
        BattleFinished,
        Paused,
        PausedSettings
    }
    
    public void Start(Level level, Fort leftFort, Fort? rightFort, Action<Team?> battleOverCallback, bool leftIsPlayer = true, bool rightIsPlayer = false, bool deterministic = false)
    {
        // _leftFort = leftFort;
        // _rightFort = rightFort;
        // Debug.Assert(_leftFort != null && _rightFort != null);
        
        _winner = null;
        _state = SceneState.BattleActive;
        _battleOverCallback = battleOverCallback;
        int zoomIndex = _zoomLevels.IndexOf(GetWindowScaleDPI().X);
        _zoomLevel = zoomIndex == -1 ? 1 : zoomIndex;
        
        Time.TimeScale = 1;
        Program.CurrentScene = this;
        World.InitializeBattle(level, leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);
        World.Camera.Zoom = (float)_zoomLevels[(int)_zoomLevel];
        _log += $"first random: {World.RandomInt(100)}\n";
        
        Resources.PlayMusicByName("jesper-kyd-highlands");
    }

    public override void Update()
    {
        // ----- INPUT PHASE -----
        HandleInputs();

        // ----- WORLD UPDATE PHASE -----
        if (_state == SceneState.BattleActive || _state == SceneState.BattleFinished)
        {
            UpdateWorld();
            DoCameraShake();
        }
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(new Color(16, 8, 4, 255));
        
        World.DrawFloor();
        World.Draw();
        
        if (_pathFinderDebug) World.LeftTeam.PathFinder.DrawDebug();
        // if (_pathFinderDebug) World.RightTeam.PathFinder.DrawDebug();

        switch (_state)
        {
            case SceneState.StartPending:
                break;
            case SceneState.Dialogue:
                _dialogBox.Draw();
                break;
            case SceneState.BattleActive:
                if (Input.Pressed(Input.Action.FastForward))
                {
                    DrawRectangle(0,0,Screen.Right,Screen.Bottom,new Color(0,0,0,128));
                    DrawTextCentered(0, 0, $"{_skips+1}X SPEED", 48);
                }
                break;
            case SceneState.BattleFinished:
                DrawRectangle(Screen.HCenter-250,Screen.VCenter-150, 500, 300,new Color(0,0,0,128));
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
                if (World.DrawDebugInfo || _pathFinderDebug)
                {
                    DrawTextCentered(0, -Screen.Bottom/2, "PAUSED", 48);
                }
                else
                {
                    DrawRectangle(0,0,Screen.Right,Screen.Bottom,new Color(0,0,0,128));
                    DrawTextCentered(0, 0, "PAUSED", 48);
                    if (Button100(-50, 40, "Settings"))
                    {
                        _state = SceneState.PausedSettings;
                    }
                    if (Button100(-50, 80, "Resume"))
                    {
                        TogglePaused();
                    }
                    if (Button100(-50, 120, "Quit"))
                    {
                        _battleOverCallback(null);
                    }
                }
                break;
            case SceneState.PausedSettings:
                DrawRectangle(0,0,Screen.Right,Screen.Bottom,new Color(0,0,0,128));
                if (Settings.DrawSettingsMenu()) _state = SceneState.Paused;
                break;
        }
        
        GameConsole.Draw();
        EndDrawing();
    }

    private void HandleInputs()
    {
        if (Input.Pressed(Input.Action.Pause) || Input.Pressed(Input.Action.Exit))
        {
            TogglePaused();
        }
        if (_state == SceneState.Paused && Input.Pressed(Input.Action.FrameStep)) // Frame step
        {
            Time.TimeScale = 1;
            Time.UpdateTime();
            UpdateWorld();
            Time.TimeScale = 0;
        }
        
        if (Input.Pressed(Input.Action.FastForward))
        {
            SetMasterVolume(0.25f);
        }
        if (Input.Released(Input.Action.FastForward))
        {
            SetMasterVolume(1f);
        }

        if (Input.Held(Input.Action.CameraLeft))
        {
            World.Camera.Offset.X += 4;
        }
        if (Input.Held(Input.Action.CameraRight))
        {
            World.Camera.Offset.X -= 4;
        }
        if (Input.Held(Input.Action.CameraUp))
        {
            World.Camera.Offset.Y += 4;
        }
        if (Input.Held(Input.Action.CameraDown))
        {
            World.Camera.Offset.Y -= 4;
        }

        if (Input.Pressed(Input.Action.PathDebug))
        {
            _pathFinderDebug = !_pathFinderDebug;
        }

        if (Input.Pressed(KeyboardKey.T) && _dialogQueue.Count > 0)
        {
            Time.TimeScale = 0;
            _dialogBox = _dialogQueue[0];
            _dialogBox.Start();
            _dialogQueue.RemoveAt(0);
            _state = SceneState.Dialogue;
        }
        if (_state == SceneState.Dialogue && Input.Pressed(Input.Action.AdvanceDialogue))
        {
            Time.TimeScale = 1;
            _state = SceneState.BattleActive;
        }
        
        if (Input.Pressed(Input.Action.Debug))
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
            // World.Camera.Zoom = Math.Clamp(World.Camera.Zoom + wheel * 0.25f, 0.25f, 8f);
            _zoomLevel += wheel;
            _zoomLevel = Math.Clamp(_zoomLevel, 0, _zoomLevels.Count-1);
            World.Camera.Zoom = (float)_zoomLevels[(int)Math.Floor(_zoomLevel)];
        }
    }

    // Updates the world, and repeats if FFW is enabled.
    private void UpdateWorld()
    {
        double startTime = GetTime();
        
        World.Update();
        CheckWinner();
        
        if (_state == SceneState.BattleActive && Input.Held(Input.Action.FastForward))
        {
            _skips = 0;
            while (_state == SceneState.BattleActive && (GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < 0.016)
            {
                Time.UpdateTime();
                World.Update();
                CheckWinner();
                _skips++;
            }
        }
    }

    public void TogglePaused()
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
    
    private void CheckWinner()
    {
        if (_winner != null) return;

        Team loser = World.LeftTeam;
        if (World.LeftTeam.GetHealth() <= 0 || Input.Pressed(KeyboardKey.Minus))
        {
            _winner = World.RightTeam;
            loser = World.LeftTeam;
        }
        if (World.RightTeam.GetHealth() <= 0)
        {
            _winner = World.LeftTeam;
            loser = World.RightTeam;
        }

        if (_winner != null)
        {
            _log += $"last random: {World.RandomInt(100)}";
            _state = SceneState.BattleFinished;
            World.EndBattle();
            
            List<Int2D> edges = new List<Int2D>();
            for (int x = 0; x < World.BoardWidth; x++)
            for (int y = 0; y < World.BoardHeight; y++)
            {
                if (x == 0 || y == 0 || x == World.BoardWidth-1 || y == World.BoardHeight-1)
                {
                    edges.Add(new Int2D(x,y));
                }
            }

            List<NavPath> paths = new List<NavPath>();
            foreach (Minion minion in World.Minions)
            {
                if (minion.Team == _winner)
                {
                    minion.SetState(new Minion.Cheer(minion));
                }
                else
                {
                    minion.SetState(new Minion.Flee(minion));
                    NavPath n = minion.NavPath;
                    n.Reset(minion.Position);
                    // n.Points = new List<Int2D>(edges);
                    paths.Add(n);
                }
            }

            if (paths.Count > 0)
            {
                paths[0].Points = new List<Int2D>(edges);
                loser.PathFinder.FindPathsBatched(paths);
            }
        }
    }
    
    public void StartCameraShake(double duration, double intensity)
    {
        _cameraShakeRemaining += duration;
        _cameraShakeIntensity = intensity;
    }

    public void StopCameraShake()
    {
        World.Camera.Offset -= _cameraShakeDisplacement;
        _cameraShakeDisplacement = Vector2.Zero;
        _cameraShakeRemaining = 0;
        _cameraShakeIntensity = 0;
    }

    private void DoCameraShake()
    {
        if (_cameraShakeRemaining > 0)
        {
            World.Camera.Offset -= _cameraShakeDisplacement;
            _cameraShakeDisplacement = new Vector2
                (Random.Shared.NextSingle() - 0.5f, Random.Shared.NextSingle() - 0.5f) * 2 * (float)_cameraShakeIntensity;
            World.Camera.Offset += _cameraShakeDisplacement;
            _cameraShakeRemaining = Math.Max(_cameraShakeRemaining - Time.DeltaTime, 0);
        }
        else if (_cameraShakeDisplacement != Vector2.Zero)
        {
            World.Camera.Offset -= _cameraShakeDisplacement;
            _cameraShakeDisplacement = Vector2.Zero;
        }
    }

    public void AddDialog(DialogBox dialogBox)
    {
        _dialogQueue.Add(dialogBox);

        if (_state != SceneState.Dialogue)
        {
            Time.TimeScale = 0;
            _dialogBox = _dialogQueue[0];
            _dialogBox.Start();
            _dialogQueue.RemoveAt(0);
            _state = SceneState.Dialogue;
        }
    }
}
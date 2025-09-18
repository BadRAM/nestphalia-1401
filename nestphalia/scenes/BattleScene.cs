using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class BattleScene : Scene
{
    // private Fort _leftFort;
    // private Fort _rightFort;
    private Level _level;
    private Team? _winner;
    private int _skips;
    private bool _pathFinderDebug;
    private Action<Team?> _battleOverCallback;
    private SceneState _state;
    private WrenCommand _wrenCommand = new WrenCommand();
    private static List<BattleEvent> _events = new List<BattleEvent>();
    private static List<BattleEvent> _battleEndEvents = new List<BattleEvent>();
    // private static List<Action<string>> _battleOverEvent;

    private double _zoomLevel = 5;
    private readonly List<Double> _zoomLevels = [0.05, 0.1, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 3.5, 4, 5, 6, 7, 8, 10, 12, 14, 16]; 
    
    // private List<Vector2> CameraShakeQueue = new List<Vector2>();
    private Vector2 _cameraShakeDisplacement = Vector2.Zero;
    private double _cameraShakeIntensity = 6;
    private double _cameraShakeRemaining = 0;
    
    private string _log;

    private Vector2 _canopyTexOffset;
    private Texture2D _canopyShadow;
    private Texture2D _canopyShadowHighlights;
    
    private enum SceneState
    {
        StartPending,
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
        _level = level;
        
        _winner = null;
        _state = SceneState.BattleActive;
        _battleOverCallback = battleOverCallback;
        int zoomIndex = _zoomLevels.IndexOf(Settings.Saved.WindowScale);
        _zoomLevel = zoomIndex == -1 ? 1 : zoomIndex;
        
        Time.TimeScale = 1;
        Program.CurrentScene = this;
        World.InitializeBattle(level, leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);
        World.Camera.Zoom = (float)_zoomLevels[(int)_zoomLevel];
        _log += $"first random: {World.RandomInt(100)}\n";
        
        _wrenCommand.Execute(level.Script);
        GameConsole.WriteLine($"World.InitializeBattle executed script:{level.Script}");

        // TODO: Move weather effect rendering into world, or into it's own class managed by world.
        _canopyTexOffset = new Vector2(Random.Shared.Next(2048), Random.Shared.Next(2048));
        _canopyShadow = Resources.GetTextureByName("shadow_canopy");
        _canopyShadowHighlights = Resources.GetTextureByName("shadow_canopy_godray");
        SetTextureFilter(_canopyShadow, TextureFilter.Bilinear);
        SetTextureFilter(_canopyShadowHighlights, TextureFilter.Bilinear);
        
        Resources.PlayMusicByName(level.Music == "" ? "jesper-kyd-highlands" : level.Music);
    }

    public override void Update()
    {
        // ----- INPUT PHASE -----
        HandleInputs();

        // ----- WORLD UPDATE PHASE -----
        if (!PopupManager.PopupActive() && (_state == SceneState.BattleActive || _state == SceneState.BattleFinished))
        {
            UpdateWorld();
            DoCameraShake();
        }
        
        // ----- DRAW PHASE -----
        Screen.BeginDrawing();
        ClearBackground(new Color(16, 8, 4, 255));
        
        DrawGradientBackground(_level.GradientTop, _level.GradientBottom);        
        
        World.DrawFloor();
        World.Draw();

        DrawCanopyShadows();
        
        World.DrawGUI();
        
        
        if (_pathFinderDebug) World.LeftTeam.PathFinder.DrawDebug();
        // if (_pathFinderDebug) World.RightTeam.PathFinder.DrawDebug();

        switch (_state)
        {
            case SceneState.StartPending:
                break;
            case SceneState.BattleActive:
                if (Input.Held(Input.InputAction.FastForward))
                {
                    DrawRectangle(0,0,Screen.RightX,Screen.BottomY,new Color(0,0,0,128));
                    DrawTextCentered(0, 0, $"{_skips+1}X SPEED", 48);
                }
                break;
            case SceneState.BattleFinished:
                DrawRectangle(Screen.CenterX-250,Screen.CenterY-150, 500, 300,new Color(0,0,0,128));
                DrawTextCentered(0, -48, "BATTLE OVER!", 48);
                DrawTextCentered(0, 0, $"{_winner?.Name} is victorious!", 48);
                if (Screen.DebugMode) DrawTextCentered(0, 100, $"{_log}");
                if (Button100(-50, 30, "Return"))
                {
                    SetMasterVolume(1f); // if F is still held, restore full volume.
                    _battleOverCallback(_winner);
                }
                break;
            case SceneState.Paused:
                if (Screen.DebugMode || _pathFinderDebug)
                {
                    DrawTextCentered(0, -Screen.BottomY/2, "PAUSED", 48);
                }
                else
                {
                    DrawRectangle(0,0,Screen.RightX,Screen.BottomY,new Color(0,0,0,128));
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
                DrawRectangle(0,0,Screen.RightX,Screen.BottomY,new Color(0,0,0,128));
                if (Settings.DrawSettingsMenu()) _state = SceneState.Paused;
                break;
        }
    }

    private void HandleInputs()
    {
        if (Input.Pressed(Input.InputAction.Pause) || Input.Pressed(Input.InputAction.Exit))
        {
            TogglePaused();
        }
        if (_state == SceneState.Paused && Input.Pressed(Input.InputAction.FrameStep)) // Frame step
        {
            Time.TimeScale = 1;
            Time.UpdateTime();
            UpdateWorld();
            Time.TimeScale = 0;
        }
        
        if (Input.Pressed(Input.InputAction.FastForward))
        {
            SetMasterVolume(0.25f);
        }
        if (Input.Released(Input.InputAction.FastForward))
        {
            SetMasterVolume(1f);
        }

        if (Input.Held(Input.InputAction.CameraLeft))
        {
            World.Camera.Offset.X += 4;
        }
        if (Input.Held(Input.InputAction.CameraRight))
        {
            World.Camera.Offset.X -= 4;
        }
        if (Input.Held(Input.InputAction.CameraUp))
        {
            World.Camera.Offset.Y += 4;
        }
        if (Input.Held(Input.InputAction.CameraDown))
        {
            World.Camera.Offset.Y -= 4;
        }

        if (Input.Pressed(Input.InputAction.PathDebug))
        {
            _pathFinderDebug = !_pathFinderDebug;
        }
        
        if (Input.Held(MouseButton.Right))
        {
            World.Camera.Target -= GetMouseDelta() / World.Camera.Zoom;
            World.Camera.Target = Vector2.Clamp(World.Camera.Target, Vector2.Zero, new Vector2(World.BoardWidth, World.BoardHeight) * 24);
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

            World.Camera.Target -= (World.Camera.Offset - Screen.Center) / World.Camera.Zoom;
            World.Camera.Offset = Screen.Center;
            
            World.Camera.Target = Vector2.Clamp(World.Camera.Target, Vector2.Zero, new Vector2(World.BoardWidth, World.BoardHeight) * 24);
        }

        if (IsWindowResized())
        {
            World.Camera.Offset = Screen.Center;
            // World.Camera.Target = new Vector2(World.BoardWidth, World.BoardHeight) * 24 / 2;
        }
    }

    // Updates the world, and repeats if FFW is enabled.
    private void UpdateWorld()
    {
        double startTime = GetTime();
        
        World.Update();
        UpdateEvents();
        CheckWinner();
        
        if (_state == SceneState.BattleActive && Input.Held(Input.InputAction.FastForward))
        {
            _skips = 0;
            while (_state == SceneState.BattleActive && (GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < 0.016)
            {
                Time.UpdateTime();
                World.Update();
                UpdateEvents();
                CheckWinner();
                _skips++;
            }
        }
    }

    private void UpdateEvents()
    {
        for (int i = _events.Count - 1; i >= 0; i--)
        {
            if (_events[i].Update())
            {
                _events.RemoveAt(i);
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
        if (World.LeftTeam.Health <= 0 || Input.Pressed(KeyboardKey.Minus))
        {
            _winner = World.RightTeam;
            loser = World.LeftTeam;
        }
        if (World.RightTeam.Health <= 0)
        {
            _winner = World.LeftTeam;
            loser = World.RightTeam;
        }

        if (_winner != null)
        {
            foreach (BattleEvent e in _battleEndEvents)
            {
                e.Update();
            }
            
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

            World.LeftTeam.ClearFear();
            World.RightTeam.ClearFear();

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

    private void DrawGradientBackground(Color topColor, Color bottomColor)
    {
        DrawRectangle(0, 0, Screen.RightX, Screen.BottomY/4, topColor);
        DrawRectangleGradientV(0, Screen.BottomY/4, Screen.RightX, Screen.BottomY/4, topColor, bottomColor);
        DrawRectangle(0, Screen.BottomY/2, Screen.RightX, Screen.BottomY/2, bottomColor);
    }

    private void DrawCanopyShadows()
    {
        Screen.SetCamera(World.Camera);
        float scale = 4;
        Rectangle source = new Rectangle(new Vector2((float)(-40 + Math.Sin(Time.Scaled / 10) * 40)/scale, 0) + _canopyTexOffset, World.BoardWidth * 24/scale, World.BoardHeight * 24/scale);
        Rectangle dest = new Rectangle(new Vector2(0, 8), World.BoardWidth * 24, World.BoardHeight * 24);
        DrawTexturePro(_canopyShadow, source, dest, Vector2.Zero, 0, ColorAlpha(Color.White, 0.5f));
        for (int i = 0; i < 30; i++)
        {
            dest.Position += new Vector2(-2, -8);
            DrawTexturePro(_canopyShadowHighlights, source, dest, Vector2.Zero, 0, ColorAlpha(Color.White, 0.025f * ((30-i) / 30f)));
        }
        Screen.SetCamera();
    }

    public void AddEvent(BattleEvent e)
    {
        _events.Add(e);
    }

    public void AddEndEvent(BattleEvent e)
    {
        _battleEndEvents.Add(e);
    }
}
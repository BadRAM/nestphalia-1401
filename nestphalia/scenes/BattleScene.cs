using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class BattleScene : Scene
{
    public States State;
    public int Wave;
    public int WaveTick;
    private const double _waveDuration = 20;
    private const int _waveTickCap = (int)(_waveDuration * Time.FrameRate);
    
    private Level _level;
    private Team? _winner;
    private int _skips;
    private bool _pathFinderDebug;
    private Action<Team?> _battleOverCallback;
    private WrenCommand _wrenCommand = new WrenCommand();
    private List<BattleEvent> _events = new List<BattleEvent>();
    private List<BattleEvent> _battleEndEvents = new List<BattleEvent>();
    private Minion? _selectedMinion;
    // private static List<Action<string>> _battleOverEvent;

    private double _zoomLevel = 5;
    private readonly List<Double> _zoomLevels = [0.05, 0.1, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 3.5, 4, 5, 6, 7, 8, 10, 12, 14, 16]; 
    
    private Vector2 _cameraShakeDisplacement = Vector2.Zero;
    private double _cameraShakeIntensity = 6;
    private double _cameraShakeRemaining = 0;
    
    private string _log;

    private Vector2 _canopyTexOffset;
    private Texture2D _canopyShadow;
    private Texture2D _canopyShadowHighlights;
    private Texture2D _clock;
    private Texture2D _clockHand;
    private StretchyTexture _selectBoxBG;
    private static SoundResource _waveStartSoundEffect;
    
    public enum States
    {
        StartPending,
        BattleActive,
        BattleFinished,
        Paused,
        PausedSettings
    }
    
    public void Start(Level level, Fort leftFort, Fort? rightFort, Action<Team?> battleOverCallback, bool leftIsPlayer = true, bool rightIsPlayer = false, bool deterministic = false)
    {
        _level = level;
        
        _winner = null;
        State = States.BattleActive;
        _battleOverCallback = battleOverCallback;
        int zoomIndex = _zoomLevels.IndexOf(Settings.Saved.WindowScale);
        _zoomLevel = zoomIndex == -1 ? 1 : zoomIndex;
        
        Time.Paused = false;
        Program.CurrentScene = this;
        World.InitializeBattle(this, level, leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);
        World.Camera.Zoom = (float)_zoomLevels[(int)_zoomLevel];
        _log += $"first random: {World.RandomInt(100)}\n";
        
        _wrenCommand.Execute(level.Script);
        GameConsole.WriteLine($"World.InitializeBattle executed script:{level.Script}");

        WaveTick = _waveTickCap - 2 * Time.FrameRate; // Start battle two seconds before first wave.

        // TODO: Move weather effect rendering into components, maybe controlled by world?
        _canopyTexOffset = new Vector2(Random.Shared.Next(2048), Random.Shared.Next(2048));
        _canopyShadow = Resources.GetTextureByName("shadow_canopy");
        _canopyShadowHighlights = Resources.GetTextureByName("shadow_canopy_godray");
        _clock = Resources.GetTextureByName("clock");
        _clockHand = Resources.GetTextureByName("clock_hand");
        SetTextureFilter(_canopyShadow, TextureFilter.Bilinear);
        SetTextureFilter(_canopyShadowHighlights, TextureFilter.Bilinear);
        _waveStartSoundEffect = Resources.GetSoundByName("start");

        _selectBoxBG = Assets.Get<StretchyTexture>("stretch_default");
        
        // Resources.PlayMusicByName(level.Music == "" ? "jesper-kyd-highlands" : level.Music);
        Resources.PlayMusicByName(level.Music == "" ? "nd_battle_live" : level.Music);
    }

    public override void Update()
    {
        // ----- INPUT PHASE -----
        HandleInputs();

        // ----- WORLD UPDATE PHASE -----
        if (!PopupManager.PopupActive() && (State == States.BattleActive || State == States.BattleFinished))
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

        // DrawCanopyShadows();

        DrawSelectHud();
        
        World.DrawGUI();
        
        
        if (_pathFinderDebug) World.LeftTeam.PathFinder.DrawDebug();

        switch (State)
        {
            case States.StartPending:
                break;
            case States.BattleActive:
                DrawClock();
                if (Input.Held(Input.InputAction.FastForward))
                {
                    DrawRectangle(0,0,Screen.RightX,Screen.BottomY,new Color(0,0,0,128));
                    DrawTextCentered(0, 0, $"{_skips+1}X SPEED", 48);
                }
                break;
            case States.BattleFinished:
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
            case States.Paused:
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
                        State = States.PausedSettings;
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
            case States.PausedSettings:
                DrawRectangle(0,0,Screen.RightX,Screen.BottomY,new Color(0,0,0,128));
                if (Settings.DrawSettingsMenu()) State = States.Paused;
                break;
        }
    }

    private void UpdateWaveClock()
    {
        WaveTick++;
        if (WaveTick >= _waveTickCap)
        {
            WaveTick = 0;
            Wave++;
            _waveStartSoundEffect.Play();
        }
    }

    private void HandleInputs()
    {
        if (Input.Pressed(Input.InputAction.Pause) || Input.Pressed(Input.InputAction.Exit))
        {
            TogglePaused();
        }
        if (State == States.Paused && Input.Pressed(Input.InputAction.FrameStep)) // Frame step
        {
            Time.Paused = false;
            Time.UpdateTime();
            UpdateWorld();
            Time.Paused = true;
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

        if (Input.Pressed(MouseButton.Left))
        {
            _selectedMinion = null;
            foreach (Minion m in World.GetMinionsInRegion(World.GetMouseTilePos(), 4))
            {
                if (CheckCollisionPointCircle(World.GetMousePos(), m.Position.XYZ2D(), 2*m.Template.PhysicsRadius))
                {
                    if (_selectedMinion == null || Vector2.Distance(World.GetMousePos(), m.Position.XYZ2D()) < Vector2.Distance(World.GetMousePos(), _selectedMinion.Position.XYZ2D()))
                    {
                        _selectedMinion = m;
                    }
                }
            }
        }
        
        if (Input.Held(MouseButton.Right))
        {
            World.Camera.Target -= GetMouseDelta() / World.Camera.Zoom;
            World.Camera.Target = Vector2.Clamp(World.Camera.Target, Vector2.Zero, new Vector2(World.BoardWidth, World.BoardHeight) * 24);
        }
        
        // Snipped from raylib example
        Vector2 wheel = GetMouseWheelMoveV();
        if (wheel.Y != 0)
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
            _zoomLevel += wheel.Y;
            _zoomLevel = Math.Clamp(_zoomLevel, 0, _zoomLevels.Count-1);
            World.Camera.Zoom = (float)_zoomLevels[(int)Math.Floor(_zoomLevel)];

            World.Camera.Target -= (World.Camera.Offset - Screen.Center) / World.Camera.Zoom;
            World.Camera.Offset = Screen.Center;
            
            World.Camera.Target = Vector2.Clamp(World.Camera.Target, Vector2.Zero, new Vector2(World.BoardWidth, World.BoardHeight) * 24);
        }
        if (Screen.DebugMode)
        {
            World.Camera.Rotation += wheel.X;
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
        
        UpdateWaveClock();
        World.Update();
        UpdateEvents();
        CheckWinner();
        
        if (State == States.BattleActive && Input.Held(Input.InputAction.FastForward))
        {
            _skips = 0;
            while (State == States.BattleActive && (GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < Time.DeltaTime)
            {
                Time.UpdateTime(true);
                UpdateWaveClock();
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
        switch (State)
        {
            case States.BattleActive:
                State = States.Paused;
                Time.Paused = true;
                break;
            case States.Paused:
                State = States.BattleActive;
                Time.Paused = false;
                break;
        }
    }

    public void SetWinner(Team team)
    {
        _winner = team;
    }
    
    private void CheckWinner()
    {
        if (State == States.BattleFinished) return;

        Team loser = World.LeftTeam;
        if (World.LeftTeam.Health <= 0)
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
            State = States.BattleFinished;
            
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
    
    private void DrawSelectHud()
    {
        if (_selectedMinion == null) return;
        if (_selectedMinion.Health <= 0)
        {
            _selectedMinion = null;
            return;
        }
        
        Screen.SetCamera(World.Camera);
        _selectedMinion.DrawDebug(true);

        foreach (Minion m in World.Minions)
        {
            if (m.Origin == _selectedMinion.Origin)
            {
                DrawCircleLinesV(m.Position.XY(), m.Template.PhysicsRadius, Color.Blue);
            }
        }
        DrawCircleLinesV(_selectedMinion.Position.XY(), _selectedMinion.Template.PhysicsRadius, Color.Green);
        Screen.SetCamera();

        DrawStretchyTexture(_selectBoxBG, new Rectangle(30, -150, 240, 320), Screen.Left);
        string text = $"{_selectedMinion.Template.Name}\n" +
                      $"HP: {_selectedMinion.Health:N1}/{_selectedMinion.Template.MaxHealth} - {(100 * _selectedMinion.Health / _selectedMinion.Template.MaxHealth):N1}%\n" +
                      $"{_selectedMinion.GetStateString()}\n" +
                      $"{_selectedMinion.Status.ListEffects()}";
        text = WrapText(text, 240);
        DrawTextLeft(30, -150, text, color:Color.Black, anchor:Screen.Left);
        if (Input.Pressed(KeyboardKey.B)) _selectedMinion.Status.Add(new BurningStatus(2, 5));
    }

    private void DrawGradientBackground(Color topColor, Color bottomColor)
    {
        DrawRectangle(0, 0, Screen.RightX, Screen.BottomY/4, topColor);
        DrawRectangleGradientV(0, Screen.BottomY/4, Screen.RightX, Screen.BottomY/4, topColor, bottomColor);
        DrawRectangle(0, Screen.BottomY/2, Screen.RightX, Screen.BottomY/2, bottomColor);
    }

    private void DrawClock()
    {
        DrawTexture(_clock, 10, 10, Color.White);
        DrawTexturePro(_clockHand, _clockHand.Rect(), new Rectangle(74, 74, _clockHand.Size()), _clockHand.Size()/2, (float)Time.Scaled * 18 - 27, Color.White);
        DrawTextLeft(174, 10, Wave.ToString(), 128, anchor:Screen.TopLeft);
        if (Screen.DebugMode) DrawTextLeft(256, 10, $"WaveTick: {WaveTick}", anchor:Screen.TopLeft);
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
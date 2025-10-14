using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class CustomBattleMenu : Scene
{
    private static Fort? _leftFort;
    private static Fort? _rightFort;
    private static bool _leftIsPlayer;
    private static bool _rightIsPlayer;
    private static bool _deterministicMode;
    private string _outcomeMessage = "";
    private string _activeDirectory = "";
    private Level _arenaLevel;

    public void Start(Fort? leftFort = null, Fort? rightFort = null)
    {
        _arenaLevel = Assets.Get<Level>("level_arena");
        World.InitializePreview(_arenaLevel);

        if (leftFort != null) _leftFort = leftFort;
        if (rightFort != null) _rightFort = rightFort;
        _leftFort?.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
        _rightFort?.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
        _outcomeMessage = "";
        _activeDirectory = "";
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        // Resources.PlayMusicByName("scene03");
        Resources.PlayMusicByName("nd_credits_live");
    }
    
    public override void Update()
    {
        World.Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY+50) * GUI.GetWindowScale();
        
        if (Input.Pressed(Input.InputAction.Exit))
        {
            new MenuScene().Start();
        }
        
        Screen.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        Screen.DrawBackground(Color.DarkGray);
        
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY+50) * GUI.GetWindowScale();
        World.Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        World.DrawFloor();
        World.Draw();
        
        if (GUI.Button100(100, -260, $"Pick Left" ))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/", 
            fort =>
            {
                _leftFort = fort;
                _leftFort.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
            },
            path =>
            {
                new EditorScene().Start(fort => {Start(leftFort:fort);}, new Fort(path));
            }));
        }
        if (GUI.Button100(100, -220, $"Pick Right"))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/", 
            fort =>
            {
                _rightFort = fort;
                _rightFort.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
            },
            path =>
            {
                new EditorScene().Start(fort => {Start(rightFort:fort);}, new Fort(path));
            }));
        }
        if (_leftFort != null  && GUI.Button300(300, -260, $"Edit {_leftFort.Name}" )) new EditorScene().Start(f => {Start(leftFort:f);}, _leftFort);
        if (_rightFort != null && GUI.Button300(300, -220, $"Edit {_rightFort.Name}")) new EditorScene().Start(f => {Start(rightFort:f);}, _rightFort);
        
        if (_leftFort != null  && GUI.Button100(200, -260, _leftIsPlayer  ? "PLAYER" : "CPU" )) _leftIsPlayer =  !_leftIsPlayer;
        if (_rightFort != null && GUI.Button100(200, -220, _rightIsPlayer ? "PLAYER" : "CPU" )) _rightIsPlayer = !_rightIsPlayer;
        
        string vs = (_leftFort  != null ? _leftFort.Name  : "???") + " VS " +
                    (_rightFort != null ? _rightFort.Name : "???");
        
        GUI.DrawTextCentered(0, -250, vs, 24);
        GUI.DrawTextLeft(-200, -200, _leftFort?.FortSummary() ?? "");
        GUI.DrawTextLeft(100, -200, _rightFort?.FortSummary() ?? "");
        GUI.DrawTextCentered(0, 220, _outcomeMessage, 24);
        
        if (GUI.Button300(300, -180, "Open Forts Folder")) Utils.OpenFolder(@"\forts");
        
        if (GUI.Button300(300, -140, $"Deterministic Mode {(_deterministicMode ? "On" : "Off")}"))
        {
            _deterministicMode = !_deterministicMode;
        }
        
        if (GUI.Button300(300, 260, "Back"))
        {
            new MenuScene().Start();
        }
        
        if (_leftFort != null && _rightFort != null &&
            (GUI.Button300(-150, 260, "Begin!") || Input.Pressed(KeyboardKey.T)))
        {
            new BattleScene().Start(Assets.Get<Level>("level_arena"), _leftFort, _rightFort, BattleOver, _leftIsPlayer, _rightIsPlayer, _deterministicMode);
        }
        else if (Input.Pressed(KeyboardKey.T))
        {
            if (_rightFort == null || _leftFort == null)
            {
                List<Fort> forts = new List<Fort>();
                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/"))
                {
                    if (file.Substring(file.Length - 5) == ".fort")
                    {
                        forts.Add(Fort.LoadFromDisc(file));
                    }
                }
                forts = forts.OrderBy(o => o.Name.ToLower()).ToList();
                _leftFort = forts[0];
                _leftFort.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
                _rightFort = forts[0];
                _rightFort.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
            }
        }
    }

    private void BattleOver(Team? winner)
    {
        Start();
        if (winner == null)
        {
            _outcomeMessage = "Battle aborted.";
        }
        else
        {
            _outcomeMessage = winner.Name + " won the battle!";
        }
    }
}
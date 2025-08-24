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
    private bool _loadingLeftSide = true;
    private string _outcomeMessage = "";
    private string _activeDirectory = "";

    public void Start(Fort? leftFort = null, Fort? rightFort = null)
    {
        World.InitializePreview();

        if (leftFort != null) _leftFort = leftFort;
        if (rightFort != null) _rightFort = rightFort;
        // if (_leftFort  != null) _leftFort  = Resources.LoadFort(_leftFort.Path  + "/" + _leftFort.Name  + ".fort") ?? _leftFort;
        // if (_rightFort != null) _rightFort = Resources.LoadFort(_rightFort.Path + "/" + _rightFort.Name + ".fort") ?? _rightFort;
        _leftFort?.LoadToBoard(new Int2D(1,1),false);
        _rightFort?.LoadToBoard(new Int2D(26, 1), true);
        _loadingLeftSide = true;
        _outcomeMessage = "";
        _activeDirectory = "";
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        Resources.PlayMusicByName("scene03");
    }
    
    public override void Update()
    {
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
        
        if (GUI.Button300(300, -300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }
        
        if (GUI.Button100(100, -260, $"Pick Left" ))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/", 
            fort =>
            {
                _leftFort = fort;
                _leftFort.LoadToBoard(new Int2D(1, 1), false);
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
                _rightFort.LoadToBoard(new Int2D(26, 1), true);
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
            GUI.Button300(-150, 260, "Begin!"))
        {
            new BattleScene().Start(Assets.GetLevelByID("level_arena"), _leftFort, _rightFort, BattleOver, _leftIsPlayer, _rightIsPlayer, _deterministicMode);
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
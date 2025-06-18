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
    private int _fortListPage = 1;
    private string[] _directories = [];
    private string[] _fortFiles = [];

    public void Start()
    {
        World.InitializePreview();

        if (_leftFort  != null) _leftFort  = Resources.LoadFort(_leftFort.Path  + "/" + _leftFort.Name  + ".fort") ?? _leftFort;
        if (_rightFort != null) _rightFort = Resources.LoadFort(_rightFort.Path + "/" + _rightFort.Name + ".fort") ?? _rightFort;
        _leftFort?.LoadToBoard(false);
        _rightFort?.LoadToBoard(true);
        _loadingLeftSide = true;
        _outcomeMessage = "";
        _activeDirectory = "";
        _fortListPage = 1;
        _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
        _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        Resources.PlayMusicByName("scene03");
    }
    
    public override void Update()
    {
        if (Input.Pressed(Input.Action.Exit))
        {
            new MenuScene().Start();
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        Screen.DrawBackground(Color.DarkGray);
        
        World.Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter+50) * GUI.GetWindowScale();
        World.Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        World.DrawFloor();
        World.Draw();

        if (GUI.Button300(300, -300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }
        
        if (_leftFort != null  && GUI.Button300(300, -260, $"Edit {_leftFort.Name}" )) new EditorScene().Start(Start, _leftFort);
        if (_rightFort != null && GUI.Button300(300, -220, $"Edit {_rightFort.Name}")) new EditorScene().Start(Start, _rightFort);
        
        if (_leftFort != null  && GUI.Button100(200, -260, _leftIsPlayer  ? "PLAYER" : "CPU" )) _leftIsPlayer =  !_leftIsPlayer;
        if (_rightFort != null && GUI.Button100(200, -220, _rightIsPlayer ? "PLAYER" : "CPU" )) _rightIsPlayer = !_rightIsPlayer;
        
        ListForts();
        
        string vs = (_leftFort  != null ? _leftFort.Name  : "???") + " VS " +
                    (_rightFort != null ? _rightFort.Name : "???");
        
        GUI.DrawTextCentered(0, -250, vs, 24);
        GUI.DrawTextLeft(-200, -200, _leftFort?.FortSummary() ?? "");
        GUI.DrawTextLeft(100, -200, _rightFort?.FortSummary() ?? "");
        GUI.DrawTextCentered(0, 220, _outcomeMessage, 24);
        
        if (GUI.Button300(300, -180, "Open Forts Folder"))
        {
            System.Diagnostics.Process.Start("explorer.exe", Directory.GetCurrentDirectory() + @"\forts");
        }
        
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
            new BattleScene().Start(_leftFort, _rightFort, BattleOver, _leftIsPlayer, _rightIsPlayer, _deterministicMode);
        }
        
        Raylib.EndDrawing();
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

    private void ListForts()
    {
        if (GUI.Button100(-600, 260, "<", _fortListPage > 1)) _fortListPage--;
            GUI.Button100(-500, 260, _fortListPage.ToString(), false);
        if (GUI.Button100(-400, 260, ">", _fortListPage <= ((_activeDirectory == "" ? 0 : 1) + _directories.Length + _fortFiles.Length)/12)) _fortListPage++;
        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage - 1) * 12 + (_activeDirectory != "" ? -1 : 0);
            if (index >= _directories.Length + _fortFiles.Length + 1) break;
            if (index == -1) // This is the 'return to parent folder' button
            {
                if (GUI.Button300(-600, i * 40 - 240, "^  Return to Parent Folder  ^"))
                {
                    _activeDirectory = "";
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length) // This is a directory
            {
                if (GUI.Button300(-600, i * 40 - 240,
                        $"/{Path.GetFileName(_directories[index])}/"))
                {
                    _activeDirectory = Path.GetFileName(_directories[index]);
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length + _fortFiles.Length) // This is a fort
            {
                string fortPath = _fortFiles[index - _directories.Length];
                if (GUI.Button300(-600, i * 40 - 240,
                        Path.GetFileNameWithoutExtension(fortPath)))
                {
                    GameConsole.WriteLine("Loading " + Path.GetFileName(fortPath));
                    if (_loadingLeftSide)
                    {
                        _leftFort = Resources.LoadFort(fortPath.Substring(Directory.GetCurrentDirectory().Length));
                        _leftFort.Name = Path.GetFileNameWithoutExtension(fortPath);
                        _leftFort.Comment = _leftFort.FortSummary();
                        _leftFort.LoadToBoard(false);
                    }
                    else
                    {
                        _rightFort = Resources.LoadFort(fortPath.Substring(Directory.GetCurrentDirectory().Length));
                        _rightFort.Name = Path.GetFileNameWithoutExtension(fortPath);
                        _rightFort.Comment = _rightFort.FortSummary();
                        _rightFort.LoadToBoard(true);
                    }

                    _loadingLeftSide = !_loadingLeftSide;
                }
            }
            else
            {
                if (GUI.Button300(-600, i * 40 - 240, "+  New Fort  +"))
                {
                    string path = "/forts/" + _activeDirectory;
                    Fort f = new Fort(Resources.GetUnusedFortName(path), path);
                    new EditorScene().Start(Start, f);
                }
            }
        }
    }
}
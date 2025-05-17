using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public static class CustomBattleMenu
{
    public static Fort? LeftFort;
    public static Fort? RightFort;
    private static bool _loadingLeftSide = true;
    public static string OutcomeMessage;
    private static int _fortListPage = 1;
    private static bool _leftIsPlayer = false;
    private static bool _rightIsPlayer = false;
    private static bool _deterministicMode = false;
    private static string _activeDirectory = "";
    private static string[] _directories;
    private static string[] _fortFiles;

    public static void Start()
    {
        Program.CurrentScene = Scene.CustomBattleSetup;
        Screen.RegenerateBackground();
        _loadingLeftSide = true;
        OutcomeMessage = "";
        World.InitializePreview();
        LeftFort?.LoadToBoard(false);
        RightFort?.LoadToBoard(true);
        Resources.PlayMusicByName("scene03");
        _activeDirectory = "";
        _fortListPage = 1;
        _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
        _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
    }
    
    public static void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            MenuScene.Start();
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        Screen.DrawBackground(Color.DarkGray);
        
        World.Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter+50) * GUI.GetWindowScale();
        World.Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        World.DrawFloor();
        World.Draw();


        if (GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }
        
        if (LeftFort != null  && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 260, $"Edit {LeftFort.Name}" )) EditorScene.Start(LeftFort,  true);
        if (RightFort != null && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 220, $"Edit {RightFort.Name}")) EditorScene.Start(RightFort, true);
        
        if (LeftFort != null  && GUI.ButtonNarrow(Screen.HCenter + 200, Screen.VCenter - 260, _leftIsPlayer  ? "PLAYER" : "CPU" )) _leftIsPlayer =  !_leftIsPlayer;
        if (RightFort != null && GUI.ButtonNarrow(Screen.HCenter + 200, Screen.VCenter - 220, _rightIsPlayer ? "PLAYER" : "CPU" )) _rightIsPlayer = !_rightIsPlayer;
        
        ListForts();
        
        string vs = (LeftFort  != null ? LeftFort.Name  : "???") + " VS " +
                    (RightFort != null ? RightFort.Name : "???");
        
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter-250, vs, 24);
        GUI.DrawTextLeft(Screen.HCenter - 200, Screen.VCenter - 200, LeftFort?.Comment  ?? "");
        GUI.DrawTextLeft(Screen.HCenter + 100, Screen.VCenter - 200, RightFort?.Comment ?? "");
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter + 220, OutcomeMessage, 24);
        
        if (GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 180, "Open Forts Folder"))
        {
            System.Diagnostics.Process.Start("explorer.exe", Directory.GetCurrentDirectory() + @"\forts");
        }
        
        if (GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 140, $"Deterministic Mode {(_deterministicMode ? "On" : "Off")}"))
        {
            _deterministicMode = !_deterministicMode;
        }
        
        if (GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter + 260, "Back"))
        {
            MenuScene.Start();
        }
        
        if (LeftFort != null && RightFort != null &&
            GUI.ButtonWide(Screen.HCenter-150, Screen.VCenter + 260, "Begin!"))
        {
            BattleScene.Start(LeftFort, RightFort, _leftIsPlayer, _rightIsPlayer, _deterministicMode);
            BattleScene.CustomBattle = true;
        }
        
        Raylib.EndDrawing();
    }

    private static void ListForts()
    {
        if (GUI.ButtonNarrow(Screen.HCenter - 600, Screen.VCenter + 260, "<", _fortListPage > 1)) _fortListPage--;
            GUI.ButtonNarrow(Screen.HCenter - 500, Screen.VCenter + 260, _fortListPage.ToString(), false);
        if (GUI.ButtonNarrow(Screen.HCenter - 400, Screen.VCenter + 260, ">", _fortListPage <= ((_activeDirectory == "" ? 0 : 1) + _directories.Length + _fortFiles.Length)/12)) _fortListPage++;
        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage - 1) * 12 + (_activeDirectory != "" ? -1 : 0);
            if (index >= _directories.Length + _fortFiles.Length + 1) break;
            if (index == -1) // This is the 'return to parent folder' button
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "^  Return to Parent Folder  ^"))
                {
                    _activeDirectory = "";
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length) // This is a directory
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240,
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
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240,
                        Path.GetFileNameWithoutExtension(fortPath)))
                {
                    Console.WriteLine("Loading " + Path.GetFileName(fortPath));
                    if (_loadingLeftSide)
                    {
                        LeftFort = Resources.LoadFort(fortPath);
                        LeftFort.Name = Path.GetFileNameWithoutExtension(fortPath);
                        LeftFort.Comment = LeftFort.FortSummary();
                        LeftFort.Path = Path.GetDirectoryName(fortPath);
                        LeftFort.LoadToBoard(false);
                    }
                    else
                    {
                        RightFort = Resources.LoadFort(fortPath);
                        RightFort.Name = Path.GetFileNameWithoutExtension(fortPath);
                        RightFort.Comment = RightFort.FortSummary();
                        RightFort.Path = Path.GetDirectoryName(fortPath);
                        RightFort.LoadToBoard(true);
                    }

                    _loadingLeftSide = !_loadingLeftSide;
                }
            }
            else
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "+  New Fort  +"))
                {
                    Fort f = new Fort();
                    f.Path = Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory;
                    f.Name = Resources.GetUnusedFortName(f.Path);
                    EditorScene.Start(f, creativeMode: true);
                }
            }
        }
    }
}
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
    private static string _activeDirectory = "";
    private static string[] _directories;
    private static string[] _files;

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
    }
    
    public static void Update()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        Screen.DrawBackground(Color.DarkGray);
        World.DrawFloor();
        World.Draw();

        string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);
        string[] forts = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory);

        if (GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }
        
        if (LeftFort != null  && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 260, $"Edit {LeftFort.Name}" )) EditorScene.Start(LeftFort,  true);
        if (RightFort != null && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 220, $"Edit {RightFort.Name}")) EditorScene.Start(RightFort, true);
        
        if (LeftFort != null  && GUI.ButtonNarrow(Screen.HCenter + 200, Screen.VCenter - 260, _leftIsPlayer  ? "PLAYER" : "CPU" )) _leftIsPlayer =  !_leftIsPlayer;
        if (RightFort != null && GUI.ButtonNarrow(Screen.HCenter + 200, Screen.VCenter - 220, _rightIsPlayer ? "PLAYER" : "CPU" )) _rightIsPlayer = !_rightIsPlayer;
        
        if (GUI.ButtonNarrow(Screen.HCenter - 600, Screen.VCenter + 260, "<", _fortListPage > 1)) _fortListPage--;
            GUI.ButtonNarrow(Screen.HCenter - 500, Screen.VCenter + 260, _fortListPage.ToString(), false);
        if (GUI.ButtonNarrow(Screen.HCenter - 400, Screen.VCenter + 260, ">", _fortListPage <= (1 + directories.Length + forts.Length)/12)) _fortListPage++;

        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage-1)*12 + (_activeDirectory != "" ? -1 : 0);
            if (index >= directories.Length + forts.Length + 1) break;
            if (index == -1)
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "^  return to parent folder  ^"))
                {
                    _activeDirectory = "";
                }
            }
            else if (index < directories.Length)
            {
                if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter + i*40 - 240, $"/{Path.GetFileName(directories[index])}/"))
                {
                    _activeDirectory = Path.GetFileName(directories[index]);
                }
            }
            else if (index < directories.Length + forts.Length)
            {
                if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter + i*40 - 240, Path.GetFileNameWithoutExtension(forts[index - directories.Length])))
                {
                    Console.WriteLine("Loading " + Path.GetFileName(forts[index]));
                    if (_loadingLeftSide)
                    {
                        LeftFort = Resources.LoadFort(forts[index - directories.Length]);
                        LeftFort.Name = Path.GetFileNameWithoutExtension(forts[index - directories.Length]);
                        LeftFort.Comment = LeftFort.FortSummary();
                        LeftFort.Path = forts[index - directories.Length];
                        LeftFort.LoadToBoard(false);
                    }
                    else
                    {
                        RightFort = Resources.LoadFort(forts[index - directories.Length]);
                        RightFort.Name = Path.GetFileNameWithoutExtension(forts[index - directories.Length]);
                        RightFort.Comment = RightFort.FortSummary();
                        RightFort.Path = forts[index - directories.Length];
                        RightFort.LoadToBoard(true);
                    }
                    _loadingLeftSide = !_loadingLeftSide;
                }
            }
            else
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "+ New Fort +"))
                {
                    Fort f = new Fort();
                    f.Path = Directory.GetCurrentDirectory() + "/forts/" + _activeDirectory + "/newFort.fort";
                    EditorScene.Start(f, creativeMode: true);
                }
            }
        }
        
        string vs = (LeftFort  != null ? LeftFort.Name  : "???") + " VS " +
                    (RightFort != null ? RightFort.Name : "???");
        
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter-250, vs, 24);
        GUI.DrawTextLeft(Screen.HCenter - 200, Screen.VCenter - 200, LeftFort?.Comment  ?? "");
        GUI.DrawTextLeft(Screen.HCenter + 100, Screen.VCenter - 200, RightFort?.Comment ?? "");
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter + 220, OutcomeMessage, 24);
        
        if (GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter + 260, "Back"))
        {
            MenuScene.Start();
        }
        
        if (LeftFort != null && RightFort != null &&
            GUI.ButtonWide(Screen.HCenter-150, Screen.VCenter + 260, "Begin!"))
        {
            BattleScene.Start(LeftFort, RightFort, _leftIsPlayer, _rightIsPlayer);
            BattleScene.CustomBattle = true;
        }
        
        Raylib.EndDrawing();
    }
}
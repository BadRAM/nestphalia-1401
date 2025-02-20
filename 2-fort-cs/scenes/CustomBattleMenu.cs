using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class CustomBattleMenu
{
    public static Fort? LeftFort;
    public static Fort? RightFort;
    private static bool _loadingLeftSide = true;
    public static string OutcomeMessage;

    public static void Start()
    {
        Program.CurrentScene = Scene.CustomBattleSetup;
        Screen.RegenerateBackground();
        _loadingLeftSide = true;
        OutcomeMessage = "";
    }

    public static void Update()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.GRAY);
        Screen.DrawBackground(Raylib.DARKGRAY);

        
        string[] forts = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/");

        if (GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }

        if (LeftFort != null  && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 260, $"Edit {LeftFort.Name}" )) EditorScene.Start(LeftFort,  true);
        if (RightFort != null && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 220, $"Edit {RightFort.Name}")) EditorScene.Start(RightFort, true);
        
        for (int i = 0; i < forts.Length; i++)
        {
            if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter + i*40 - 300, Path.GetFileName(forts[i])))
            {
                Console.WriteLine("Loading " + Path.GetFileName(forts[i]));
                if (_loadingLeftSide)
                {
                    LeftFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[i]));
                    LeftFort.Name = Path.GetFileNameWithoutExtension(forts[i]);
                    LeftFort.Comment = LeftFort.FortSummary();
                }
                else
                {
                    RightFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[i]));
                    RightFort.Name = Path.GetFileNameWithoutExtension(forts[i]);
                    RightFort.Comment = RightFort.FortSummary();
                }
                _loadingLeftSide = !_loadingLeftSide;
            }
        }

        string vs = (LeftFort != null ? LeftFort.Name : "???") + " VS " +
                    (RightFort  != null ? RightFort.Name  : "???");
        
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter-250, vs, 24);
        GUI.DrawTextLeft(Screen.HCenter - 100, Screen.VCenter - 100, LeftFort?.Comment ?? "");
        GUI.DrawTextLeft(Screen.HCenter + 100, Screen.VCenter - 100, RightFort?.Comment ?? "");
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter + 100, OutcomeMessage, 24);

        if (GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter + 260, "Back"))
        {
            MenuScene.Start();
        }
        
        if (LeftFort != null && RightFort != null &&
            GUI.ButtonWide(Screen.HCenter-150, Screen.VCenter + 260, "Begin!"))
        {
            BattleScene.Start(LeftFort, RightFort);
            BattleScene.CustomBattle = true;
        }
        
        Raylib.EndDrawing();
    }
}
using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class CustomBattleMenu
{
    public static Fort? LeftFort;
    public static Fort? RightFort;
    private static bool _loadingLeftSide = true;
    public static string OutcomeMessage;
    private static int _fortListPage = 1;

    public static void Start()
    {
        Program.CurrentScene = Scene.CustomBattleSetup;
        Screen.RegenerateBackground();
        _loadingLeftSide = true;
        OutcomeMessage = "";
        World.InitializePreview();
        World.Camera.zoom = 0.5f;
        World.Camera.offset = new Vector2(Screen.HCenter-288, Screen.VCenter-100);
        LeftFort?.LoadToBoard(false);
        RightFort?.LoadToBoard(true);
    }

    public static void Update()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.GRAY);
        Screen.DrawBackground(Raylib.DARKGRAY);
        World.Draw();
        
        string[] forts = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/");

        if (GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, _loadingLeftSide ? "Selecting Left Fort" : "Selecting Right Fort"))
        {
            _loadingLeftSide = !_loadingLeftSide;
        }

        if (LeftFort != null  && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 260, $"Edit {LeftFort.Name}" )) EditorScene.Start(LeftFort,  true);
        if (RightFort != null && GUI.ButtonWide(Screen.HCenter + 300, Screen.VCenter - 220, $"Edit {RightFort.Name}")) EditorScene.Start(RightFort, true);

        if (GUI.ButtonNarrow(Screen.HCenter - 600, Screen.VCenter + 260, "<", _fortListPage > 1)) _fortListPage--;
            GUI.ButtonNarrow(Screen.HCenter - 500, Screen.VCenter + 260, _fortListPage.ToString(), false);
        if (GUI.ButtonNarrow(Screen.HCenter - 400, Screen.VCenter + 260, ">", _fortListPage <= forts.Length/12)) _fortListPage++;

        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage-1)*12;
            if (index >= forts.Length) break;
            if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter + i*40 - 240, Path.GetFileName(forts[index])))
            {
                Console.WriteLine("Loading " + Path.GetFileName(forts[index]));
                if (_loadingLeftSide)
                {
                    LeftFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[index]));
                    LeftFort.Name = Path.GetFileNameWithoutExtension(forts[index]);
                    LeftFort.Comment = LeftFort.FortSummary();
                    LeftFort.LoadToBoard(false);
                }
                else
                {
                    RightFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[index]));
                    RightFort.Name = Path.GetFileNameWithoutExtension(forts[index]);
                    RightFort.Comment = RightFort.FortSummary();
                    RightFort.LoadToBoard(true);
                }
                _loadingLeftSide = !_loadingLeftSide;
            }
        }

        string vs = (LeftFort != null ? LeftFort.Name : "???") + " VS " +
                    (RightFort  != null ? RightFort.Name  : "???");
        
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter-250, vs, 24);
        GUI.DrawTextLeft(Screen.HCenter - 200, Screen.VCenter - 200, LeftFort?.Comment ?? "");
        GUI.DrawTextLeft(Screen.HCenter + 100, Screen.VCenter - 200, RightFort?.Comment ?? "");
        GUI.DrawTextCentered(Screen.HCenter, Screen.VCenter + 220, OutcomeMessage, 24);

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
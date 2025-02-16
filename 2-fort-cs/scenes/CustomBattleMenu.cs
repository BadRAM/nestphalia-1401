using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class CustomBattleMenu
{
    public static Fort? PlayerFort;
    public static Fort? EnemyFort;
    private static bool _loadingPlayerSide = true;
    public static string OutcomeMessage;

    public static void Start()
    {
        Program.CurrentScene = Scene.CustomBattleSetup;
        _loadingPlayerSide = true;
        OutcomeMessage = "";
    }

    public static void Update()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.GRAY);
        
        string[] forts = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/");

        if (RayGui.GuiButton(new Rectangle(Screen.HCenter+300, Screen.VCenter-290, 280, 26), _loadingPlayerSide ? "Selecting Left Fort" : "Selecting Right Fort") != 0)
        {
            _loadingPlayerSide = !_loadingPlayerSide;
        }

        
        for (int i = 0; i < forts.Length; i++)
        {
            if (RayGui.GuiButton(new Rectangle(Screen.HCenter-600, Screen.VCenter + i*30 - 300, 280, 26), Path.GetFileName(forts[i])) != 0)
            {
                Console.WriteLine("Loading " + Path.GetFileName(forts[i]));
                if (_loadingPlayerSide)
                {
                    PlayerFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[i]));
                    PlayerFort.Name = Path.GetFileNameWithoutExtension(forts[i]);
                    PlayerFort.Comment = PlayerFort.FortSummary();
                }
                else
                {
                    EnemyFort = Resources.LoadFort("/forts/" + Path.GetFileName(forts[i]));
                    EnemyFort.Name = Path.GetFileNameWithoutExtension(forts[i]);
                    EnemyFort.Comment = EnemyFort.FortSummary();
                }
                _loadingPlayerSide = !_loadingPlayerSide;
            }
        }

        string vs = (PlayerFort != null ? PlayerFort.Name : "???") + " VS " +
                    (EnemyFort  != null ? EnemyFort.Name  : "???");
        Raylib.DrawText(vs, Screen.HCenter - 200, Screen.VCenter - 290, 20, Raylib.WHITE);
        Raylib.DrawText(PlayerFort?.Comment ?? "", Screen.HCenter - 200, Screen.VCenter - 260, 10, Raylib.WHITE);
        Raylib.DrawText(EnemyFort?.Comment ?? "", Screen.HCenter - 100, Screen.VCenter - 260, 10, Raylib.WHITE);
        Raylib.DrawText(OutcomeMessage, Screen.HCenter - 100, Screen.VCenter + 100, 20, Raylib.WHITE);
        
        if (RayGui.GuiButton(new Rectangle(Screen.HCenter-600, Screen.VCenter+250, 280, 50), "Back") != 0)
        {
            MenuScene.Start();
        }
        
        if (PlayerFort != null && EnemyFort != null &&
            RayGui.GuiButton(new Rectangle(Screen.HCenter + 300, Screen.VCenter+250, 280, 50), "Begin!") != 0)
        {
            BattleScene.Start(PlayerFort, EnemyFort);
            BattleScene.CustomBattle = true;
        }
        
        Raylib.EndDrawing();
    }
}
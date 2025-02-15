using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;
using ZeroElectric.Vinculum.Extensions;

namespace _2_fort_cs;

public static class MenuScene
{
    private static bool _helpWindowOpen = false;

    
    public static void Start()
    {
        _helpWindowOpen = false;
        Program.CurrentScene = Scene.MainMenu;
    }

    public static void Update()
    {
        BeginDrawing();
        ClearBackground(GRAY);
        
        DrawText("2fort BUG GAME", 400, 100, 48, WHITE);
            
        if (RayGui.GuiButton(new Rectangle(400, 200, 400, 40), "Play") != 0)
        {
            Program.Campaign = new Campaign();
            if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
            {
                Program.Campaign = Campaign.Load();
            }
            Program.Campaign.Start();
        }

        // if (RayGui.GuiButton(new Rectangle(400, 250, 400, 40), "Sandbox load") != 0)
        // {
        //     Fort f = Resources.LoadFort("/creativeFort.fort");
        //     EditorScene.Start(f, true);
        // }
        if (RayGui.GuiButton(new Rectangle(400, 250, 400, 40), "Custom Battle") != 0) CustomBattleMenu.Start();
        if (RayGui.GuiButton(new Rectangle(400, 300, 400, 40), "Sandbox") != 0) EditorScene.Start(creativeMode:true);
        //if (RayGui.GuiButton(new Rectangle(400, 350, 400, 40), "Help") != 0) _helpWindowOpen = !_helpWindowOpen;
        if (RayGui.GuiButton(new Rectangle(400, 400, 400, 40), "Quit") != 0)
        {
            EndDrawing();
            CloseWindow();
            return;
        }

        if (_helpWindowOpen)
        {
            RayGui.GuiDummyRec(new Rectangle(550, 100, 400, 400), "TODO: Write help text");
        }
            
        EndDrawing();
    }
}
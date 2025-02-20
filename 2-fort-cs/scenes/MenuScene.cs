using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;
using ZeroElectric.Vinculum.Extensions;
using static _2_fort_cs.GUI;
using static _2_fort_cs.Screen;

namespace _2_fort_cs;

public static class MenuScene
{
    private static bool _helpWindowOpen = false;

    
    public static void Start()
    {
        _helpWindowOpen = false;
        Program.CurrentScene = Scene.MainMenu;
        RegenerateBackground();
    }

    public static void Update()
    {
        BeginDrawing();
        ClearBackground(GRAY);
        DrawBackground(GRAY);
        
        DrawTextCentered(HCenter, VCenter-200, "NESTPHALIA 1401", 48);
        //DrawTextEx(Resources.Font, "BUGSBUGSBUGS", new Vector2(HCenter-200, VCenter-200), 48, 4, WHITE);
            
        if (ButtonWide(HCenter-150, VCenter-80, "Start"))
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
        if (ButtonWide(HCenter-150, VCenter-40, "Custom Battle")) CustomBattleMenu.Start();
        if (ButtonWide(HCenter-150, VCenter, "Sandbox")) EditorScene.Start(creativeMode:true);
        if (ButtonWide(HCenter-150, VCenter+40, "Quit")) 
        {
            EndDrawing();
            CloseWindow();
            return;
        }

        if (ButtonNarrow(HCenter + 500, VCenter + 260, "Font")) Resources.ToggleFontAccessibility();
        
        //if (RayGui.GuiButton(new Rectangle(Screen.HCenter-200, Screen.VCenter-100, 400, 40), "Custom Battle") != 0) CustomBattleMenu.Start();
        // if (RayGui.GuiButton(new Rectangle(Screen.HCenter-150, Screen.VCenter-50, 400, 40), "Sandbox") != 0) EditorScene.Start(creativeMode:true);
        //if (RayGui.GuiButton(new Rectangle(400, 350, 400, 40), "Help") != 0) _helpWindowOpen = !_helpWindowOpen;
        // if (RayGui.GuiButton(new Rectangle(Screen.HCenter-200, Screen.VCenter+50, 400, 40), "Quit") != 0)
        // {
        //     EndDrawing();
        //     CloseWindow();
        //     return;
        // }

        if (_helpWindowOpen)
        {
            RayGui.GuiDummyRec(new Rectangle(550, 100, 400, 400), "TODO: Write help text");
        }
            
        EndDrawing();
    }
}
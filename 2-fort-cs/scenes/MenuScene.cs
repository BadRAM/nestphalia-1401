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
        DrawBackground(LIGHTGRAY);
        
        DrawTextCentered(HCenter, VCenter-200, "NESTPHALIA 1401", 48);
            
        if (ButtonWide(HCenter-150, VCenter-80, "Start"))
        {
            Program.Campaign = new Campaign();
            if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
            {
                Program.Campaign = Campaign.Load();
            }
            Program.Campaign.Start();
        }
        
        if (ButtonWide(HCenter-150, VCenter-40, "Custom Battle")) CustomBattleMenu.Start();
        if (ButtonWide(HCenter-150, VCenter, "Sandbox")) EditorScene.Start(creativeMode:true);
        if (ButtonWide(HCenter-150, VCenter+40, "Quit")) 
        {
            EndDrawing();
            CloseWindow();
            return;
        }

        if (ButtonNarrow(HCenter + 500, VCenter + 260, "Font")) Resources.ToggleFontAccessibility();
        
        
        DrawTextLeft(HCenter-600, VCenter+280, "By BadRAM and rosettedotnet");

        if (_helpWindowOpen)
        {
            RayGui.GuiDummyRec(new Rectangle(550, 100, 400, 400), "TODO: Write help text");
        }
            
        EndDrawing();
    }
}
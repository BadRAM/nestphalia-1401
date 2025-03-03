using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;
using ZeroElectric.Vinculum.Extensions;
using static nestphalia.GUI;
using static nestphalia.Screen;

namespace nestphalia;

public static class MenuScene
{
    private static bool _helpWindowOpen = false;
    
    public static void Start()
    {
        _helpWindowOpen = false;
        Program.CurrentScene = Scene.MainMenu;
        RegenerateBackground();
        Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
    }

    public static void Update()
    {
        BeginDrawing();
        ClearBackground(GRAY);
        DrawBackground(LIGHTGRAY);
        
        DrawTextCentered(HCenter, VCenter-200, "NESTPHALIA 1401", 48);
        DrawTextLeft(HCenter-600, VCenter-300, "V1.1.2 - Tournament Edition");
            
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
        if (ButtonNarrow(HCenter + 500, VCenter + 220, "Mute"))
        {
            Program.Muted = !Program.Muted;
            SetMasterVolume(Program.Muted ? 0 : 1);
        }
        
        
        DrawTextLeft(HCenter-590, VCenter+260, "By BadRAM and rosettedotnet\nWith music from the mod archive");

        if (_helpWindowOpen)
        {
            RayGui.GuiDummyRec(new Rectangle(550, 100, 400, 400), "TODO: Write help text");
        }
            
        EndDrawing();
    }
}
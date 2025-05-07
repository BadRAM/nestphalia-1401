using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
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
        ClearBackground(Color.Gray);
        DrawBackground(Color.LightGray);
        
        DrawTextCentered(HCenter, VCenter-200, "NESTPHALIA 1401", 48);
        DrawTextLeft(HCenter-600, VCenter-300, "V1.2.1 - Champion's Edition");
        
        if (ButtonWide(HCenter-150, VCenter-80, "Start")) 
        {
            // todo: find a way to move this check into Campaign.cs
            Program.Campaign = new Campaign();
            if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
            {
                Program.Campaign = Campaign.Load();
            }
            Program.Campaign.Start();
        }
        
        if (ButtonWide(HCenter-150, VCenter-40, "Custom Battle")) CustomBattleMenu.Start();
        //if (ButtonWide(HCenter-150, VCenter, "Sandbox")) EditorScene.Start(creativeMode:true);
        if (ButtonWide(HCenter-150, VCenter+40, "Quit")) 
        {
            EndDrawing();
            CloseWindow();
            return;
        }
        
        if (ButtonNarrow(HCenter - 600, VCenter - 80, "Mute Music")) 
        {
            SettingsScene.ToggleMuteMusic();
            Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
        }
        if (ButtonNarrow(HCenter - 600, VCenter - 40, "Mute SFX")) SettingsScene.ToggleMuteSFX();
        if (ButtonNarrow(HCenter - 600, VCenter + 0 , "Font")) SettingsScene.ToggleFontAccessibility();
        if (ButtonNarrow(HCenter - 600, VCenter + 40, "High DPI")) SettingsScene.ToggleWindowScale();
        if (SettingsScene.RestartNeeded) DrawTextLeft(HCenter - 488, VCenter + 52, "Restart to apply changes");
        
        
        DrawTextLeft(HCenter-590, VCenter+260, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        
        EndDrawing();
    }
}
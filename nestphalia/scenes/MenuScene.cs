using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;
using static nestphalia.Screen;

namespace nestphalia;

public class MenuScene : Scene
{
    public void Start()
    {
        Program.CurrentScene = this;
        RegenerateBackground();
        Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
    }
    
    public override void Update()
    {
        BeginDrawing();
        ClearBackground(Color.Gray);
        DrawBackground(Color.LightGray);
        
        DrawTextCentered(HCenter, VCenter-200, "NESTPHALIA 1401", 48);
        DrawTextLeft(HCenter-600, VCenter-300, "V2.0.0 - Conquest Update");
        
        if (ButtonWide(HCenter-150, VCenter-80, "Start")) new CampaignScene().Start();
        if (ButtonWide(HCenter-150, VCenter-40, "Custom Battle")) new CustomBattleMenu().Start();
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
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
        
        DrawTextCentered(0, -200, "NESTPHALIA 1401", 48);
        DrawTextLeft(-600, -300, "V2.0.0 - Conquest Update");
        
        if (Button300(-150, -80, "Start")) new CampaignScene().Start();
        if (Button300(-150, -40, "Custom Battle")) new CustomBattleMenu().Start();
        if (Button300(-150,  40, "Quit")) 
        {
            EndDrawing();
            CloseWindow();
            return;
        }
        
        if (Button100(-600, -80, "Mute Music")) 
        {
            SettingsScene.ToggleMuteMusic();
            Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
        }
        if (Button100(-600, -40, "Mute SFX")) SettingsScene.ToggleMuteSFX();
        if (Button100(-600, 0 , "Font")) SettingsScene.ToggleFontAccessibility();
        if (Button100(-600, 40, "High DPI")) SettingsScene.ToggleWindowScale();
        if (SettingsScene.RestartNeeded) DrawTextLeft(-488, 52, "Restart to apply changes");
        
        
        DrawTextLeft(-590, 260, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        
        EndDrawing();
    }
}
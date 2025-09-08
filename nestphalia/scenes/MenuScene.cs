using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;
using static nestphalia.Screen;

namespace nestphalia;

public class MenuScene : Scene
{
    private bool _settingsOpen;
    
    public void Start()
    {
        Program.CurrentScene = this;
        RegenerateBackground();
        Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
    }
    
    public override void Update()
    {
        Screen.BeginDrawing();
        ClearBackground(Color.Gray);
        DrawBackground(Color.LightGray);

        if (!_settingsOpen)
        {
            DrawTextCentered(0, -200, "NESTPHALIA 1401", 48);
            DrawTextLeft(-470, -350, "V2.0.a07 - Quest Update");
            DrawTextLeft(-470, 320, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        
            if (Button300(-150, -80, "Start")) new CampaignScene().Start();
            if (Button300(-150, -40, "Custom Battle") || Input.Pressed(KeyboardKey.T)) new CustomBattleMenu().Start();
            if (Button300(-150,   0, "Settings")) _settingsOpen = !_settingsOpen;
            if (Button300(-150,  80, "Quit")) 
            {
                Screen.EndDrawing();
                CloseWindow();
                return;
            }
            if (Button180(150,  80, "Intro Test")) new IntroScene().Start();

            if (Settings.RestartNeeded) DrawTextLeft(155, 92, "Restart to apply changes");
            
            if (DebugMode && Button300(200, -40, "Level Editor")) new LevelEditorScene().Start();
        }
        else
        {
            _settingsOpen = !Settings.DrawSettingsMenu();
        }
    }
}
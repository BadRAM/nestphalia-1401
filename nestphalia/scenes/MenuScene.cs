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
        BeginDrawing();
        ClearBackground(Color.Gray);
        DrawBackground(Color.LightGray);

        if (!_settingsOpen)
        {
            DrawTextCentered(0, -200, "NESTPHALIA 1401", 48);
            DrawTextLeft(-600, -300, "V2.0.0 - Conquest Update");
        
            if (Button300(-150, -80, "Start")) new CampaignScene().Start();
            if (Button300(-150, -40, "Custom Battle")) new CustomBattleMenu().Start();
            if (Button300(-150,   0, "Settings")) _settingsOpen = !_settingsOpen;
            if (Button300(-150,  80, "Quit")) 
            {
                EndDrawing();
                CloseWindow();
                return;
            }
            if (Settings.RestartNeeded) DrawTextLeft(155, 92, "Restart to apply changes");
        
            DrawTextLeft(-590, 260, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        }
        else
        {
            _settingsOpen = !Settings.DrawSettingsMenu();
        }
        
        EndDrawing();
    }
}
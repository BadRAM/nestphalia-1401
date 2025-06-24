using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;
using static nestphalia.Screen;

namespace nestphalia;

public class MenuScene : Scene
{
    private bool _settingsOpen;
    private Rectangle _panel;
    
    public void Start()
    {
        Program.CurrentScene = this;
        _panel = new Rectangle(CenterX-200, CenterY-220, 401, 201);
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
            _panel = Draw9Slice(Resources.GetTextureByName("9slice"), _panel, Vector2.Zero, true, true);
            Button300(30, 30, "Button on a window!", anchor: _panel.Position);
            
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
            
            if (Button300(200, -40, "Level Editor")) new LevelEditorScene().Start();
            if (Button300(200, -40, "Level Editor")) new LevelEditorScene().Start();

        
            DrawTextLeft(-590, 260, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        }
        else
        {
            _settingsOpen = !Settings.DrawSettingsMenu();
        }
        
        EndDrawing();
    }
}
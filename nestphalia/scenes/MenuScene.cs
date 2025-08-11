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
        Screen.BeginDrawing();
        ClearBackground(Color.Gray);
        DrawBackground(Color.LightGray);

        if (!_settingsOpen)
        {
            _panel = Draw9Slice(Resources.GetTextureByName("9slice"), _panel, Center, true, true);
            if (Button300(-150, 30, "Button on a window!", anchor: Center + _panel.Top()))
            {
                PopupManager.Start(new AlertPopup("Heading!", "bodybodybodybodybodybody\nbodybodybodybodybodybody\nbodybodybodybodybodybody\nbodybodybodybodybodybody\nbodybodybodybodybodybody\n", "go away", () => {}));
            }
            
            DrawTextCentered(0, -200, "NESTPHALIA 1401", 48);
            DrawTextLeft(-470, -350, "V2.0.a2 - Quest Update");
        
            if (Button300(-150, -80, "Start")) new CampaignScene().Start();
            if (Button300(-150, -40, "Custom Battle")) new CustomBattleMenu().Start();
            if (Button300(-150,   0, "Settings")) _settingsOpen = !_settingsOpen;
            if (Button300(-150,  80, "Quit")) 
            {
                Screen.EndDrawing();
                CloseWindow();
                return;
            }
            if (Settings.RestartNeeded) DrawTextLeft(155, 92, "Restart to apply changes");
            
            if (Button300(200, -40, "Level Editor")) new LevelEditorScene().Start();
        
            DrawTextLeft(-470, 320, "By BadRAM and rosettedotnet\nWith music from the mod archive");
        }
        else
        {
            _settingsOpen = !Settings.DrawSettingsMenu();
        }
    }
}
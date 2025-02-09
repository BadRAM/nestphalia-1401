using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class MenuScene
{
    private static Button PlayButton;
    private static Button SandboxButton;
    private static Button HelpButton;
    private static Button QuitButton;
    
    public static void Start()
    {
        PlayButton = new Button();
        PlayButton.Label = "Play";
        PlayButton.Bounds = new Rectangle(100, 100, 400, 40);
        
        SandboxButton = new Button();
        SandboxButton.Label = "Sandbox";
        SandboxButton.Bounds = new Rectangle(100, 150, 400, 40);
        
        HelpButton = new Button();
        HelpButton.Label = "Help";
        HelpButton.Bounds = new Rectangle(100, 200, 400, 40);
        
        QuitButton = new Button();
        QuitButton.Label = "Quit";
        QuitButton.Bounds = new Rectangle(100, 250, 400, 40);

        while (!WindowShouldClose())
        {
            PlayButton.Update();
            SandboxButton.Update();
            HelpButton.Update();

            if (QuitButton.Update()) break;
            
            BeginDrawing();
            ClearBackground(BLACK);
            
            PlayButton.Draw();
            SandboxButton.Draw();
            HelpButton.Draw();
            QuitButton.Draw();
            
            EndDrawing();
        }

        
        
        
    }
}
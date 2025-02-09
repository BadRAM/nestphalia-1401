using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;
using ZeroElectric.Vinculum.Extensions;

namespace _2_fort_cs;

public static class MenuScene
{
    private static bool _helpWindowOpen = false;

    
    public static void Start()
    {
        _helpWindowOpen = false;
        Program.CurrentScene = Scene.MainMenu;
    }

    public static void Update()
    {
        BeginDrawing();
        ClearBackground(GRAY);
            
        // Questionable pattern here, what happens when two begindrawing/enddrawing calls happen in a row?
        if (RayGui.GuiButton(new Rectangle(400, 100, 400, 40), "Play") != 0)
        {
            Program.Campaign = new Campaign();
            if (File.Exists(Directory.GetCurrentDirectory() + "/save/campaign.sav"))
            {
                Program.Campaign = Campaign.Load();
            }
            Program.Campaign.Start();
        }
        if (RayGui.GuiButton(new Rectangle(400, 150, 400, 40), "Sandbox") != 0) EditorScene.Start();
        if (RayGui.GuiButton(new Rectangle(400, 200, 400, 40), "Help") != 0) _helpWindowOpen = !_helpWindowOpen;
        if (RayGui.GuiButton(new Rectangle(400, 250, 400, 40), "Quit") != 0) return;

        if (_helpWindowOpen)
        {
            RayGui.GuiDummyRec(new Rectangle(550, 100, 400, 400), "TODO: Write help text");
        }
            
        EndDrawing();
    }
}
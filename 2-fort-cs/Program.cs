using System.Diagnostics;
using System.Numerics;
using static ZeroElectric.Vinculum.Raylib;
namespace _2_fort_cs;

public enum Scene
{
	Intro,
	MainMenu,
	Campaign,
	Editor,
	CustomBattleSetup,
	Battle
}

static class Program
{
	private static bool Paused;
	public static Scene CurrentScene;
	public static Campaign Campaign;
	
    public static void Main()
    {
        InitWindow(1200, 600, "2-fort");
        SetTargetFPS(60);
        
        // Load a texture from the resources directory
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/"))
        {
	        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/");
        }
        Resources.Load();
        Assets.Load();
        
        IntroScene.Start();
        while (!WindowShouldClose())
        {
	        Time.UpdateTime();

	        switch (CurrentScene)
	        {
		        case Scene.Intro:
			        IntroScene.Update();
			        break;
		        case Scene.MainMenu:
			        MenuScene.Update();
			        break;
		        case Scene.Campaign:
			        Campaign.Update();
			        break;
		        case Scene.Editor:
			        EditorScene.Update();
			        break;
		        case Scene.CustomBattleSetup:
			        CustomBattleMenu.Update();
			        break;		       
		        case Scene.Battle:
			        BattleScene.Update();
			        break;
	        }
        }

        Console.WriteLine("Quitting time!");
        
	    Resources.Unload();
        
        //CloseWindow();
    }
}
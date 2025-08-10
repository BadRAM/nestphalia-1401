using Raylib_cs;
namespace nestphalia;

static class Program
{
	public static Scene CurrentScene;
	
    public static void Main()
    {
	    // Complete install if needed
	    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/"))
	    {
		    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/");
	    }
	    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/Campaign/"))
	    {
		    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/Campaign/");
	    }
	    
	    // Startup sequence
	    Settings.Load();
	    Screen.Initialize();
	    Resources.PreLoad();
	    Raylib.InitAudioDevice();
	    
	    Screen.BeginDrawing();
	    GUI.DrawTextCentered(0,0,"Loading...", 48, anchor:Screen.Center);
	    Screen.EndDrawing();
	    
	    Resources.Load();
	    Screen.Load();
	    Assets.Load();
        GUI.Initialize();
        
        // Start the first scene. TODO: loading screen, then intro cutscene, rather than menu
        
        new MenuScene().Start();
        
        while (!Raylib.WindowShouldClose())
        {
	        Time.UpdateTime();
	        Input.Poll();
			
	        if (Raylib.IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }
	        
			CurrentScene.Update();
			if (Raylib.WindowShouldClose()) break;
			
			PopupManager.Update();
			GameConsole.Draw();
			GUI.UpdateCursor();
			
			Screen.EndDrawing();
			
			Raylib.UpdateMusicStream(Resources.MusicPlaying);
        }
		
        GameConsole.WriteLine("Quitting time!");
        
	    Resources.Unload();
        
	    Raylib.CloseAudioDevice();
        //CloseWindow();
    }
}
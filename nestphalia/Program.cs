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
	    Raylib.InitAudioDevice();
	    Resources.Load();
	    Screen.Load();
	    Assets.Load();
        GUI.Initialize();
        
        // Start the first scene. TODO: loading screen, then intro cutscene, rather than menu
        new MenuScene().Start();
        
        while (!Raylib.WindowShouldClose())
        {
	        Time.UpdateTime();
			
	        if (Raylib.IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }
	        
			CurrentScene.Update();
			if (Raylib.WindowShouldClose()) break;
			
			Popup.Update();
			GameConsole.Draw();
			GUI.UpdateCursor();
			
			Screen.EndDrawing();

			// Ugly hack to fix loud sound at start of first song.
	        if (Time.Scaled <= 1)
	        {
		        Raylib.SetMusicVolume(Resources.MusicPlaying, MathF.Min((float)Time.Scaled, 0.3f));
	        }
	        Raylib.UpdateMusicStream(Resources.MusicPlaying);
        }
		
        GameConsole.WriteLine("Quitting time!");
        
	    Resources.Unload();
        
	    Raylib.CloseAudioDevice();
        //CloseWindow();
    }
}
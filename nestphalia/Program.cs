using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using static Raylib_cs.Raylib;
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
	    InitAudioDevice();
	    Resources.Load();
	    Screen.Load();
	    Assets.Load();
        GUI.Initialize();
        
        // Start the first scene. TODO: loading screen, then intro cutscene, rather than menu
        new MenuScene().Start();
        
        while (!WindowShouldClose())
        {
	        Time.UpdateTime();
			
	        if (IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }
	        
			CurrentScene.Update();
			if (WindowShouldClose()) break;
			
			Popup.Update();
			GameConsole.Draw();
			GUI.UpdateCursor();
			
			EndDrawing();

			// Ugly hack to fix loud sound at start of first song.
	        if (Time.Scaled <= 1)
	        {
		        SetMusicVolume(Resources.MusicPlaying, MathF.Min((float)Time.Scaled, 0.3f));
	        }
	        UpdateMusicStream(Resources.MusicPlaying);
        }
		
        GameConsole.WriteLine("Quitting time!");
		
        if (Settings.RestartNeeded)
        {
	        Settings.Saved.WindowScale = !Settings.Saved.WindowScale;
	        Settings.Save();
        }
        
	    Resources.Unload();
        
	    CloseAudioDevice();
        //CloseWindow();
    }
}
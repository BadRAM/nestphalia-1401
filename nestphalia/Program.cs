using System.Diagnostics;
using Raylib_cs;

namespace nestphalia;

static class Program
{
	public static Scene CurrentScene;
	
    public static void Main()
    {
	    Stopwatch startupTimer = Stopwatch.StartNew();
	    Settings.Load();
	    
	    // Complete install if needed
	    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/"))
	    {
		    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/");
		    GameConsole.WriteLine("Created forts folder");
	    }
	    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/Campaign/"))
	    {
		    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/Campaign/");
		    GameConsole.WriteLine("Created Campaign forts folder");
	    }
	    
	    // Startup sequence
	    Screen.Initialize();
	    Resources.PreLoad();
	    Raylib.InitAudioDevice();
	    GameConsole.WriteLine($"Preload completed in {startupTimer.ElapsedMilliseconds}ms");
	    startupTimer.Restart();
	    
	    Screen.BeginDrawing();
	    GUI.DrawTextCentered(0,0,"Loading...", 48, anchor:Screen.Center);
	    Screen.EndDrawing();
	    
	    Resources.Load();
	    GameConsole.WriteLine($"Resources loaded in {startupTimer.ElapsedMilliseconds}ms");
	    startupTimer.Restart();
	    Assets.Load();
	    GameConsole.WriteLine($"Assets loaded in {startupTimer.ElapsedMilliseconds}ms");
	    startupTimer.Stop();
	    Screen.Load();
        GUI.Initialize();
        GameConsole.WrenCommand.Execute("""System.print("Wren VM is running.")""");
        
        // Start the first scene. TODO: loading screen, then intro cutscene, rather than menu
        
        new IntroScene().Start();
        
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
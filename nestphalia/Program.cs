using System.Diagnostics;
using Raylib_cs;

namespace nestphalia;

static class Program
{
	public static Scene CurrentScene;
	public static CampaignSaveData? ActiveCampaign;
	
    public static void Main()
    {
#if DEBUG
	    Start();
	    Loop();
	    End();
#else
	    try
	    {
			Start();
			Loop();
			End();
	    }
	    catch (Exception e)
	    {
			string log = string.Join('\n', GameConsole.LogHistory) + "\n\n======== CRITICAL ERROR ========\n" + e.ToString();
			string path = Directory.GetCurrentDirectory() + $"\\crash-{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}.log";
		    File.WriteAllText(path, log);
		    Console.WriteLine($"Wrote crash log out to {path}");
	    }
#endif
    }

    private static void Start()
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
        
	    // Start the first scene.
        
	    new IntroScene().Start();
    }

    private static void Loop()
    {
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
		    Input.Draw();
		    GUI.UpdateCursor();
			
		    Screen.EndDrawing();
			
		    Raylib.UpdateMusicStream(Resources.MusicPlaying);
	    }
    }

    private static void End()
    {
	    GameConsole.WriteLine("Quitting time!");
        
	    Resources.Unload();
        
	    Raylib.CloseAudioDevice();
    }
}
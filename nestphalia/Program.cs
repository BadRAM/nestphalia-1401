using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace nestphalia;

static class Program
{
	private static bool Paused;
	public static Scene CurrentScene;
	
    public static void Main()
    {
	    Settings.Load();
	    ConfigFlags flags = ConfigFlags.ResizableWindow;
	    if (Settings.Saved.WindowScale) flags |= ConfigFlags.HighDpiWindow;

	    SetConfigFlags(flags);
	    InitWindow(1200, 600, "2-fort");
	    SetWindowMinSize(1200, 600);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);
		
        InitAudioDevice();
        
        Screen.UpdateBounds();
        
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/"))
        {
	        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/");
        }
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/Campaign/"))
        {
	        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/Campaign/");
        }
        Resources.Load();
        Assets.Load();
        Screen.Initialize();
        GUI.Initialize();
        new MenuScene().Start();
        
        while (!WindowShouldClose())
        {
	        Time.UpdateTime();

	        if (IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }

	        GUI.UpdateCursor();
	        
			CurrentScene.Update();

	        if (Time.Scaled <= 1)
	        {
		        SetMusicVolume(Resources.MusicPlaying, MathF.Min((float)Time.Scaled, 0.3f));
	        }
	        UpdateMusicStream(Resources.MusicPlaying);
        }

        Console.WriteLine("Quitting time!");

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
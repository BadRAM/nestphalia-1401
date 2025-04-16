using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace nestphalia;

public enum Scene
{
	Intro,
	MainMenu,
	Campaign,
	Editor,
	CustomBattleSetup,
	Battle,
	PostBattle
}

static class Program
{
	private static bool Paused;
	public static bool SFXMute;
	public static bool MusicMute;
	public static Scene CurrentScene;
	public static Campaign Campaign;
	
    public static void Main()
    {
	    // Stopwatch sw = new Stopwatch();
	    // List<int> testList = new List<int>();
	    // sw.Start();
	    // for (int i = 0; i < 100000; i++)
	    // {
		   //  testList.Insert(0, i);
	    // }
	    // sw.Stop();
	    // Console.WriteLine($"Inserted 100000 times in {sw.Elapsed.TotalMilliseconds}ms");
	    // sw.Restart();
	    // for (int i = 0; i < 100000; i++)
	    // {
		   //  testList.RemoveAt(0);
	    // }
	    // sw.Stop();
	    // Console.WriteLine($"Removed first 100000 times in {sw.Elapsed.TotalMilliseconds}ms");
	    // testList = new List<int>();
	    // sw.Restart();
	    // for (int i = 0; i < 100000; i++)
	    // {
		   //  testList.Insert(i, i);
	    // }
	    // sw.Stop();
	    // Console.WriteLine($"Inserted at end 100000 times in {sw.Elapsed.TotalMilliseconds}ms");
	    // sw.Restart();
	    // for (int i = 0; i < 100000; i++)
	    // {
		   //  testList.RemoveAt(testList.Count-1);
	    // }
	    // sw.Stop();
	    // Console.WriteLine($"Removed last 100000 times in {sw.Elapsed.TotalMilliseconds}ms");
	    // testList = new List<int>();
	    // sw.Restart();
	    // for (int i = 0; i < 100000; i++)
	    // {
		   //  testList.Add(i);
	    // }
	    // sw.Stop();
	    // Console.WriteLine($"Added 100000 times in {sw.Elapsed.TotalMilliseconds}ms");
	    // sw.Restart();
	    // testList.Clear();
	    // sw.Stop();
	    // Console.WriteLine($"Cleared 100000 entries in {sw.Elapsed.TotalMilliseconds}ms");

	    
	    SetConfigFlags(ConfigFlags.ResizableWindow);
	    InitWindow(1200, 600, "2-fort");
	    SetWindowMinSize(1200, 600);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);
		
        InitAudioDevice();
        
        Screen.UpdateBounds();
        
        // Load a texture from the resources directory
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/forts/"))
        {
	        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/forts/");
        }
        Resources.Load();
        Assets.Load();
        Screen.Initialize();
        GUI.Initialize();
        
        
        MenuScene.Start();
        while (!WindowShouldClose())
        {
	        Time.UpdateTime();

	        if (IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }

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

	        if (Time.Scaled <= 1)
	        {
		        SetMusicVolume(Resources.MusicPlaying, MathF.Min((float)Time.Scaled, 0.3f));
	        }
	        UpdateMusicStream(Resources.MusicPlaying);
        }

        Console.WriteLine("Quitting time!");
        
	    Resources.Unload();
        
	    CloseAudioDevice();     
        //CloseWindow();
    }
}
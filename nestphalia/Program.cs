﻿using System.Diagnostics;
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
	// public static bool SFXMute;
	// public static bool MusicMute;
	public static Scene CurrentScene;
	public static Campaign Campaign;
	
    public static void Main()
    {
	    SettingsScene.Load();
	    ConfigFlags flags = ConfigFlags.ResizableWindow;
	    if (SettingsScene.WindowScale) flags |= ConfigFlags.HighDpiWindow;

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
        
        
        MenuScene.Start();
        while (!WindowShouldClose())
        {
	        Time.UpdateTime();

	        if (IsWindowResized())
	        {
		        Screen.UpdateBounds();
	        }

	        GUI.UpdateCursor();
	        
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

        if (SettingsScene.RestartNeeded)
        {
	        SettingsScene.WindowScale = !SettingsScene.WindowScale;
	        SettingsScene.Save();
        }
        
	    Resources.Unload();
        
	    CloseAudioDevice();     
        //CloseWindow();
    }
}
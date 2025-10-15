using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using static nestphalia.GUI;

namespace nestphalia;

public class SavedSettings
{
    public double SFXVolume = 1;
    public double MusicVolume = 1;
    public double WindowScale = 1;
    public bool AccessibleFont;
    public bool SmallScreenMode;
    public bool Fullscreen;
    public string ResourcePathOverride = "";
}

public static class Settings
{
    public static SavedSettings Saved;
    
    public static bool RestartNeeded;
    
    public static void Save()
    {
        string jsonString = JObject.FromObject(Saved).ToString();
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/settings.cfg", jsonString);
    }

    public static void Load()
    {
        if (File.Exists(Directory.GetCurrentDirectory() + "/settings.cfg"))
        {
            string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/settings.cfg");
            Saved = JObject.Parse(jsonString).ToObject<SavedSettings>() ?? throw new NullReferenceException("Failed to deserialize settings file");
        }
        else
        {
            Saved = new SavedSettings();
            Save();
        }
        
        Resources.Dir = Saved.ResourcePathOverride == "" ? Directory.GetCurrentDirectory() : Saved.ResourcePathOverride;
    }
    
    // returns true if the 'close' button has been pressed
    public static bool DrawSettingsMenu()
    {
        double vol = Slider(-150, -200, "Music Volume", Saved.MusicVolume);
        if (Math.Abs(vol - Saved.MusicVolume) > 0.0001) // If volume was changed
        {
            Saved.MusicVolume = vol;
            Resources.MusicVolumeChanged();
            Save();
        }
        
        vol = Slider(-150, -150, "SFX Volume", Saved.SFXVolume);
        if (Math.Abs(vol - Saved.SFXVolume) > 0.0001) // If volume was changed
        {
            Saved.SFXVolume = vol;
            Save();
        }
        
        if (Button270(-135, -100, "Accessible Font"))
        {
            Saved.AccessibleFont = !Saved.AccessibleFont;
            Resources.SetFontAccessibility(Saved.AccessibleFont);
            Save();
        }

        if (Button270(-135, -60, "Fullscreen"))
        {
            Saved.Fullscreen = !Saved.Fullscreen;
            Raylib.ToggleBorderlessWindowed();
            Screen.UpdateBounds(Saved.Fullscreen ? null : Vector2.One);
            Save();
        }

        // if (Button270(-135, -20, "Dense UI Mode"))
        // {
        //     Saved.SmallScreenMode = !Saved.SmallScreenMode;
        //     Screen.UpdateBounds();
        // }
        
        bool resizing = false;
        if (Button90(-150, 40, "-") || Input.Pressed(KeyboardKey.Minus))
        {
            Vector2 res = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()) / (float)Saved.WindowScale;
            Saved.WindowScale -= 0.25;
            Saved.WindowScale = Math.Max(Saved.WindowScale, 0.25);
            res *= (float)Saved.WindowScale;
            Save();
            Screen.UpdateBounds(res);
            resizing = true;
        }
        Button90(-50, 40, Saved.WindowScale.ToString("N2"));
        if (Button90(50, 40, "+") || Input.Pressed(KeyboardKey.Equal))
        {
            Vector2 res = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()) / (float)Saved.WindowScale;
            Saved.WindowScale += 0.25;
            res *= (float)Saved.WindowScale;
            Save();
            Screen.UpdateBounds(res);
            resizing = true;
        }
        
        return Button300(-150, 160, "Save & Return", enabled: !resizing) || Input.Pressed(InputAction.Exit);
    }
}
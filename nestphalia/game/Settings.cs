using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using static nestphalia.GUI;

namespace nestphalia;

public class SavedSettings
{
    [JsonInclude] public double SFXVolume = 1;
    [JsonInclude] public double MusicVolume = 1;
    [JsonInclude] public double WindowScale = 1;
    [JsonInclude] public bool AccessibleFont;
    [JsonInclude] public string ResourcePathOverride = "";
}

public static class Settings
{
    public static SavedSettings Saved;
    
    public static bool RestartNeeded;
    
    public static void Save()
    {
        string jsonString = JsonSerializer.Serialize(Saved, SourceGenerationContext.Default.SavedSettings);
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/settings.cfg", jsonString);
    }

    public static void Load()
    {
        if (File.Exists(Directory.GetCurrentDirectory() + "/settings.cfg"))
        {
            string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/settings.cfg");
            Saved = JsonSerializer.Deserialize<SavedSettings>(jsonString, SourceGenerationContext.Default.SavedSettings) 
                    ?? throw new NullReferenceException("Failed to deserialize campaign save file");
        }
        else
        {
            Saved = new SavedSettings();
            Save();
        }
    }
    
    // returns true if the 'close' button has been pressed
    public static bool DrawSettingsMenu()
    {
        double vol = Slider(-150, -100, "Music Volume", Saved.MusicVolume);
        if (Math.Abs(vol - Saved.MusicVolume) > 0.0001) // If volume was changed
        {
            Saved.MusicVolume = vol;
            Resources.MusicVolumeChanged();
            Save();
        }
        
        vol = Slider(-150, -50, "SFX Volume", Saved.SFXVolume);
        if (Math.Abs(vol - Saved.SFXVolume) > 0.0001) // If volume was changed
        {
            Saved.SFXVolume = vol;
            Save();
        }
        
        if (Button300(-150, 0, "Accessible Font"))
        {
            Saved.AccessibleFont = !Saved.AccessibleFont;
            Resources.SetFontAccessibility(Saved.AccessibleFont);
            Save();
        }
        
        bool resizing = false;
        if (Button100(-150, 40, "-") || Input.Pressed(KeyboardKey.Minus))
        {
            Vector2 res = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()) / (float)Saved.WindowScale;
            Saved.WindowScale -= 0.25;
            Saved.WindowScale = Math.Max(Saved.WindowScale, 0.25);
            res *= (float)Saved.WindowScale;
            Save();
            Screen.UpdateBounds(res);
            resizing = true;
        }
        Button100(-50, 40, Saved.WindowScale.ToString("N2"));
        if (Button100(50, 40, "+") || Input.Pressed(KeyboardKey.Equal))
        {
            Vector2 res = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()) / (float)Saved.WindowScale;
            Saved.WindowScale += 0.25;
            res *= (float)Saved.WindowScale;
            Save();
            Screen.UpdateBounds(res);
            resizing = true;
        }
        
        return Button300(-150, 160, "Save & Return", enabled: !resizing);
    }
}
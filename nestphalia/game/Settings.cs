using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using static nestphalia.GUI;

namespace nestphalia;

public class SavedSettings
{
    // [JsonInclude] public bool SFXMute;
    [JsonInclude] public double SFXVolume = 1;
    // [JsonInclude] public bool MusicMute;
    [JsonInclude] public double MusicVolume = 1;
    [JsonInclude] public bool WindowScale;
    [JsonInclude] public bool AccessibleFont;

    // Default settings constructor
    public SavedSettings() { }
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

        if (Button300(-150, 40, "High DPI mode"))
        {
            RestartNeeded = !RestartNeeded;
        }
        if (RestartNeeded) DrawTextLeft(155, 52, "Restart to apply changes");

        return Button300(-150, 80, "Save & Exit");
    }
}
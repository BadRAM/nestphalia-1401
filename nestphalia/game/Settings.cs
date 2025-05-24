using System.Text.Json;
using System.Text.Json.Serialization;
using static nestphalia.GUI;

namespace nestphalia;

public class SavedSettings
{
    [JsonInclude] public bool SFXMute;
    [JsonInclude] public bool MusicMute;
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
        if (Button300(-150, -80, Saved.MusicMute ? "Unmute Music" : "Mute Music")) 
        {
            Saved.MusicMute = !Saved.MusicMute;
            Save();
            // Resources.PlayMusicByName("unreal_technology_demo_95_-_unreals");
            Resources.RestartMusic();
        }
        if (Button300(-150, -40, Saved.SFXMute ? "Unmute SFX" : "Mute SFX"))
        {
            Saved.SFXMute = !Saved.SFXMute;
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
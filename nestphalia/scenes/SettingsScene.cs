using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;

namespace nestphalia;

public static class SettingsScene
{
    private static SavedSettings savedSettings;

    public static bool SFXMute;
    public static bool MusicMute;
    public static bool WindowScale;
    public static bool AccessibleFont;

    public static bool RestartNeeded;
    
    // TODO: Settings menu
    // For now the serializable settings object will live here.
    
    public static void Save()
    {
        savedSettings.SFXMute = SFXMute;
        savedSettings.MusicMute = MusicMute;
        savedSettings.WindowScale = WindowScale;
        savedSettings.AccessibleFont = AccessibleFont;
        
        string jsonString = JsonSerializer.Serialize(savedSettings, SourceGenerationContext.Default.SavedSettings);
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/settings.cfg", jsonString);
    }

    public static void Load()
    {
        if (File.Exists(Directory.GetCurrentDirectory() + "/settings.cfg"))
        {
            string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/settings.cfg");
            savedSettings = JsonSerializer.Deserialize<SavedSettings>(jsonString, SourceGenerationContext.Default.SavedSettings) ?? throw new NullReferenceException("Failed to deserialize campaign save file");
        }
        else
        {
            savedSettings = new SavedSettings();
            Save();
        }

        SFXMute = savedSettings.SFXMute;
        MusicMute = savedSettings.MusicMute;
        WindowScale = savedSettings.WindowScale;
        AccessibleFont = savedSettings.AccessibleFont;
    }
    
    public class SavedSettings
    {
        [JsonInclude] public bool SFXMute;
        [JsonInclude] public bool MusicMute;
        [JsonInclude] public bool WindowScale;
        [JsonInclude] public bool AccessibleFont;
    }

    public static void ToggleMuteSFX()
    {
        SFXMute = !SFXMute;
        Save();
    }
    
    public static void ToggleMuteMusic()
    {
        MusicMute = !MusicMute;
        Save();
    }

    public static void ToggleFontAccessibility()
    {
        AccessibleFont = !AccessibleFont;
        Resources.SetFontAccessibility(AccessibleFont);
        Save();
    }

    public static void ToggleWindowScale()
    {
        RestartNeeded = !RestartNeeded;
    }
}
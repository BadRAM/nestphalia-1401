using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class SpriteResource
{
    public string Name;
    public Texture2D Tex;

    public SpriteResource(string name, Texture2D tex)
    {
        Name = name;
        Tex = tex;
    }
}

public class SoundResource
{
    public string Name;
    private Sound _sound;
    private Sound[] _soundBuffer;
    private int _bufferIndex;
    private float _volume = 1;

    public SoundResource(string name, Sound sound, int bufferSize)
    {
        Name = name;
        _sound = sound;
        SetSoundVolume(_sound, 0.75f);
        _soundBuffer = new Sound[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            _soundBuffer[i] = LoadSoundAlias(sound);
        }
    }
    
    public void Play(float pan = 0.5f, float pitch = 1f, float volume = 0.75f)
    {
        if (SettingsScene.SFXMute || IsSoundPlaying(_soundBuffer[_bufferIndex])) return;
        SetSoundPan(_soundBuffer[_bufferIndex], pan);
        SetSoundPitch(_soundBuffer[_bufferIndex], pitch);
        SetSoundVolume(_soundBuffer[_bufferIndex], volume);
        PlaySound(_soundBuffer[_bufferIndex]);
        _bufferIndex++;
        _bufferIndex %= _soundBuffer.Length;
    }

    public void PlayRandomPitch(float pan = 0.5f, float volume = 0.75f)
    {
        Play(pan, 0.8f + Random.Shared.NextSingle() * 0.4f, volume);
    }

    public static float WorldToPan(float X)
    {
        return 0.75f - (X / (World.BoardWidth * 24)) * 0.5f;
    }
}

public class MusicResource
{
    public string Name;
    public Music Music;

    public MusicResource(string name, Music music)
    {
        Name = name;
        Music = music;
    }
}

public static class Resources
{
    public static Texture2D MissingTexture;
    private static List<SpriteResource> _sprites = new List<SpriteResource>();
    private static List<SoundResource> _sounds = new List<SoundResource>();
    private static List<MusicResource> _music = new List<MusicResource>();
    public static List<Fort> CampaignLevels = new List<Fort>();
    public static Font Font;
    private static Font _accessibleFont;
    private static Font _defaultFont;
    public static Music MusicPlaying;
    
    
    public static void Load()
    {
        MissingTexture = LoadTexture("resources/sprites/missingtex.png");
        _accessibleFont = LoadFont("resources/pixelplay16.png");
        _defaultFont = LoadFont("resources/alagard.png");
        Font = SettingsScene.AccessibleFont ? _accessibleFont : _defaultFont;
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sprites"))
        {
            _sprites.Add(new SpriteResource(Path.GetFileNameWithoutExtension(path), LoadTexture("resources/sprites/" + Path.GetFileName(path))));
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sfx"))
        {
            _sounds.Add(new SoundResource(Path.GetFileNameWithoutExtension(path), LoadSound("resources/sfx/" + Path.GetFileName(path)), 16));
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/music"))
        {
            _music.Add(new MusicResource(Path.GetFileNameWithoutExtension(path), LoadMusicStream("resources/music/" + Path.GetFileName(path))));
        }

        MusicPlaying = _music[0].Music;
        PlayMusicStream(MusicPlaying);

        //Sounds.Add(new SoundResource("explosion", LoadSound("resources/sfx/explosion.wav"), 8));
        
        CampaignLevels.Add(LoadFort("/resources/level1.fort"));
        CampaignLevels.Add(LoadFort("/resources/level2.fort"));
        CampaignLevels.Add(LoadFort("/resources/level3.fort"));
        CampaignLevels.Add(LoadFort("/resources/level4.fort"));
        CampaignLevels.Add(LoadFort("/resources/level5.fort"));
        CampaignLevels.Add(LoadFort("/resources/level6.fort"));
        CampaignLevels.Add(LoadFort("/resources/level7.fort"));
        CampaignLevels.Add(LoadFort("/resources/level8.fort"));
        CampaignLevels.Add(LoadFort("/resources/level9.fort"));
        CampaignLevels.Add(LoadFort("/resources/level10.fort"));
        CampaignLevels.Add(LoadFort("/resources/level11.fort"));
        CampaignLevels.Add(LoadFort("/resources/level12.fort"));
        CampaignLevels.Add(LoadFort("/resources/level13.fort"));
        CampaignLevels.Add(LoadFort("/resources/level14.fort"));
        CampaignLevels.Add(LoadFort("/resources/level15.fort"));
        // CampaignLevels.Add(LoadFort("/resources/level16.fort"));
    }
    
    public static Texture2D GetTextureByName(string name)
    {
        SpriteResource? s = _sprites.FirstOrDefault(x => x.Name == name);
        return s?.Tex ?? MissingTexture;
    }
    
    public static SoundResource GetSoundByName(string name)
    {
        SoundResource? s = _sounds.FirstOrDefault(x => x.Name == name);
        return s ?? _sounds[0];
    }

    public static void PlayMusicByName(string name)
    {
        StopMusicStream(MusicPlaying);
        if (SettingsScene.MusicMute) return;
        
        MusicResource? s = _music.FirstOrDefault(x => x.Name == name);
        
        if (s != null)
        {
            MusicPlaying = s.Music;
            SetMusicVolume(MusicPlaying, 0.3f);
            PlayMusicStream(MusicPlaying);
        }
        else
        {
            Console.WriteLine("MusicResource was null! Music names are:");
            foreach (MusicResource m in _music)
            {
                Console.WriteLine($" - {m.Name}");
            }
            // MusicPlaying = Music[0].Music;
            // PlayMusicStream(MusicPlaying);
        }
    }

    public static void Unload()
    {
        foreach (SpriteResource spriteResource in _sprites)
        {
            UnloadTexture(spriteResource.Tex);
        }
    }

    public static void SetFontAccessibility(bool accessible)
    {
        Font = accessible ? _accessibleFont : _defaultFont;
    }

    public static void SaveFort(Fort fort)
    {
        //if (right) World.Flip();
        // Fort fort = new Fort(fortName, path);
        // fort.Name = fortName;
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                fort.Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? "";
            }
        }
        
        fort.Comment = fort.FortSummary();
        
        string jsonString = JsonSerializer.Serialize(fort, SourceGenerationContext.Default.Fort);
        //Console.WriteLine($"JSON fort looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + fort.Path + "/" + fort.Name + ".fort", jsonString);
    }

    // Takes relative path
    public static Fort? LoadFort(string filepath)
    {
        filepath = Directory.GetCurrentDirectory() + filepath;
        if (!Path.Exists(filepath))
        {
            Console.WriteLine($"Failed to find fort at {filepath}");
            return null;
        }
        string jsonString = File.ReadAllText(filepath);
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString, SourceGenerationContext.Default.Fort)!;
        fort.UpdateCost();
        fort.Path = Path.GetDirectoryName(filepath)!;
        fort.Path = fort.Path.Substring(Directory.GetCurrentDirectory().Length);
        Console.WriteLine($"Loaded {fort.Name}, path: {fort.Path}");
        return fort;
    }

    public static string GetUnusedFortName(string path)
    {
        path = Directory.GetCurrentDirectory() + path;
        int number = 1;
        while (true)
        {
            if (!File.Exists(path + $"fort{number}.fort"))
            {
                return $"fort{number}";
            }
            if (number >= 1000)
            {
                return "TooManyForts";
            }
            number++;
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Fort))]
[JsonSerializable(typeof(CampaignSaveData))]
[JsonSerializable(typeof(SettingsScene.SavedSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
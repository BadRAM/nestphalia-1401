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
        if (Settings.Saved.SFXVolume == 0 || IsSoundPlaying(_soundBuffer[_bufferIndex])) return;
        SetSoundPan(_soundBuffer[_bufferIndex], pan);
        SetSoundPitch(_soundBuffer[_bufferIndex], pitch);
        SetSoundVolume(_soundBuffer[_bufferIndex], volume * (float)Settings.Saved.SFXVolume);
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
    private static Dictionary<String, SpriteResource> _sprites = new Dictionary<String, SpriteResource>();
    private static Dictionary<String, SoundResource> _sounds = new Dictionary<String, SoundResource>();
    private static Dictionary<String, MusicResource> _music = new Dictionary<String, MusicResource>();
    public static List<Fort> CampaignLevels = new List<Fort>();
    public static Font Font;
    private static Font _accessibleFont;
    private static Font _defaultFont;
    public static Music MusicPlaying;
    
    // public static Shader TeamColorShader;
    // private static int _teamColorLoc;
    
    public static void Load()
    {
        MissingTexture = LoadTexture("resources/sprites/missingtex.png");
        
        _accessibleFont = LoadFont("resources/pixelplay16.png");
        _defaultFont = LoadFont("resources/alagard.png");
        Font = Settings.Saved.AccessibleFont ? _accessibleFont : _defaultFont;
        
        // TeamColorShader = LoadShader("", "resources/TeamColor.glsl");
        // _teamColorLoc = GetShaderLocation(TeamColorShader, "teamColor1");
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sprites"))
        {
            SpriteResource r = new SpriteResource(Path.GetFileNameWithoutExtension(path), LoadTexture("resources/sprites/" + Path.GetFileName(path)));
            _sprites.Add(r.Name, r);
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sfx"))
        {
            SoundResource r = new SoundResource(Path.GetFileNameWithoutExtension(path), LoadSound("resources/sfx/" + Path.GetFileName(path)), 16);
            _sounds.Add(r.Name, r);
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/music"))
        {
            MusicResource r = new MusicResource(Path.GetFileNameWithoutExtension(path), LoadMusicStream("resources/music/" + Path.GetFileName(path)));
            _music.Add(r.Name, r);
        }

        //MusicPlaying = _music["unreal_technology_demo_95_-_unreals"].Music;
        //PlayMusicStream(MusicPlaying);

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
        if (!_sprites.ContainsKey(name)) return MissingTexture;
        return _sprites[name].Tex;
        // SpriteResource? s = _sprites.FirstOrDefault(x => x.Name == name);
        // return s?.Tex ?? MissingTexture;
    }
    
    public static SoundResource GetSoundByName(string name)
    {
        if (!_sounds.ContainsKey(name)) return _sounds["shovel"];
        return _sounds[name];
        // SoundResource? s = _sounds.FirstOrDefault(x => x.Name == name);
        // return s ?? _sounds[0];
    }

    public static void PlayMusicByName(string name)
    {
        StopMusicStream(MusicPlaying);
        
        // MusicResource? s = _music.FirstOrDefault(x => x.Name == name);
        MusicResource? s = null;
        if (_music.ContainsKey(name)) s = _music[name];

        // if (Settings.Saved.MusicVolume == 0) return;
        
        if (s != null)
        {
            MusicPlaying = s.Music;
            SetMusicVolume(MusicPlaying, (float)Settings.Saved.MusicVolume * 0.3f);
            PlayMusicStream(MusicPlaying);
        }
        else
        {
            GameConsole.WriteLine("MusicResource was null! Music names are:");
            foreach (KeyValuePair<string, MusicResource> m in _music.ToArray())
            {
                GameConsole.WriteLine($" - {m.Key}");
            }
        }
    }

    public static void MusicVolumeChanged()
    {
        SetMusicVolume(MusicPlaying, (float)Settings.Saved.MusicVolume * 0.3f);
    }
    
    // public static void RestartMusic()
    // {
    //     StopMusicStream(MusicPlaying);
    //     if (Settings.Saved.MusicVolume == 0) return;
    //     PlayMusicStream(MusicPlaying);
    // }

    public static void Unload()
    {
        foreach (KeyValuePair<string, SpriteResource> spriteResource in _sprites.ToArray())
        {
            UnloadTexture(spriteResource.Value.Tex);
        }
        _sprites.Clear();
    }

    public static void SetFontAccessibility(bool accessible)
    {
        Font = accessible ? _accessibleFont : _defaultFont;
    }

    public static void SaveFort(Fort fort)
    {
        for (int x = 0; x < 20; x++)
        for (int y = 0; y < 20; y++)
        {
            fort.Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? "";
        }
        fort.Comment = fort.FortSummary();

        string jsonString = JsonSerializer.Serialize(fort, SourceGenerationContext.Default.Fort);
        File.WriteAllText(Directory.GetCurrentDirectory() + fort.Path + "/" + fort.Name + ".fort", jsonString);
    }

    // Takes relative path
    public static Fort? LoadFort(string filepath)
    {
        filepath = Directory.GetCurrentDirectory() + filepath;
        if (!Path.Exists(filepath))
        {
            GameConsole.WriteLine($"Failed to find fort at {filepath}");
            return null;
        }
        string jsonString = File.ReadAllText(filepath);
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString, SourceGenerationContext.Default.Fort)!;
        fort.UpdateCost();
        fort.Path = Path.GetDirectoryName(filepath)!;
        fort.Path = fort.Path.Substring(Directory.GetCurrentDirectory().Length);
        GameConsole.WriteLine($"Loaded {fort.Name}, path: {fort.Path}");
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
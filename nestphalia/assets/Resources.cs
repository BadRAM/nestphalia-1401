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
        if (IsSoundPlaying(_soundBuffer[_bufferIndex])) return;
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
    public static List<SpriteResource> Sprites = new List<SpriteResource>();
    public static List<SoundResource> Sounds = new List<SoundResource>();
    public static List<MusicResource> Music = new List<MusicResource>();
    public static List<Fort> CampaignLevels = new List<Fort>();
    public static Font Font;
    private static Font _accessibleFont;
    private static Font _defaultFont;
    private static bool _fontAccessibility;
    public static Music MusicPlaying;
    
    
    public static void Load()
    {
        MissingTexture = LoadTexture("resources/sprites/missingtex.png");
        _accessibleFont = LoadFont("resources/pixelplay16.png");
        _defaultFont = LoadFont("resources/alagard.png");
        Font = _defaultFont;
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sprites"))
        {
            Sprites.Add(new SpriteResource(Path.GetFileNameWithoutExtension(path), LoadTexture("resources/sprites/" + Path.GetFileName(path))));
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sfx"))
        {
            Sounds.Add(new SoundResource(Path.GetFileNameWithoutExtension(path), LoadSound("resources/sfx/" + Path.GetFileName(path)), 16));
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/music"))
        {
            Music.Add(new MusicResource(Path.GetFileNameWithoutExtension(path), LoadMusicStream("resources/music/" + Path.GetFileName(path))));
        }

        MusicPlaying = Music[0].Music;
        PlayMusicStream(MusicPlaying);

        //Sounds.Add(new SoundResource("explosion", LoadSound("resources/sfx/explosion.wav"), 8));
        
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level1.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level2.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level3.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level4.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level5.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level6.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level7.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level8.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level9.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level10.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level11.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level12.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level13.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level14.fort"));
        CampaignLevels.Add(LoadFort(Directory.GetCurrentDirectory() + "/resources/level15.fort"));
        // CampaignLevels.Add(LoadFort("/resources/level16.fort"));
    }
    
    public static Texture2D GetTextureByName(string name)
    {
        SpriteResource? s = Sprites.FirstOrDefault(x => x.Name == name);
        return s?.Tex ?? MissingTexture;
    }
    
    public static SoundResource GetSoundByName(string name)
    {
        SoundResource? s = Sounds.FirstOrDefault(x => x.Name == name);
        return s ?? Sounds[0];
    }

    public static void PlayMusicByName(string name)
    {
        StopMusicStream(MusicPlaying);
        MusicResource? s = Music.FirstOrDefault(x => x.Name == name);
        
        if (s != null)
        {
            MusicPlaying = s.Music;
            SetMusicVolume(MusicPlaying, 0.5f);
            PlayMusicStream(MusicPlaying);
        }
        else
        {
            Console.WriteLine("MusicResource was null! Music names are:");
            foreach (MusicResource m in Music)
            {
                Console.WriteLine($" - {m.Name}");
            }
            // MusicPlaying = Music[0].Music;
            // PlayMusicStream(MusicPlaying);
        }
    }

    public static void Unload()
    {
        foreach (SpriteResource spriteResource in Sprites)
        {
            UnloadTexture(spriteResource.Tex);
        }
    }

    public static void ToggleFontAccessibility()
    {
        _fontAccessibility = !_fontAccessibility;
        Font = _fontAccessibility ? _accessibleFont : _defaultFont;   
    }

    public static void SaveFort(string fortName, string path)
    {
        //if (right) World.Flip();
        Fort fort = new Fort();
        fort.Name = fortName;
        
        Directory.GetCurrentDirectory();
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                fort.Board[x+y*20] = World.GetTile(x+1,y+1)?.Template.ID ?? "";
            }
        }
        
        string jsonString = JsonSerializer.Serialize(fort);
        //Console.WriteLine($"JSON fort looks like: {jsonString}");
        File.WriteAllText(path, jsonString);
        
        //if (right) World.Flip();
    }

    public static Fort LoadFort(string filepath)
    {
        string jsonString = File.ReadAllText(filepath);
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString);
        return fort;
    }
}
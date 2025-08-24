using Raylib_cs;

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
        Raylib.SetSoundVolume(_sound, 0.75f);
        _soundBuffer = new Sound[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            _soundBuffer[i] = Raylib.LoadSoundAlias(sound);
        }
    }
    
    public void Play(float pan = 0.5f, float pitch = 1f, float volume = 0.75f)
    {
        if (Settings.Saved.SFXVolume == 0 || Raylib.IsSoundPlaying(_soundBuffer[_bufferIndex])) return;
        Raylib.SetSoundPan(_soundBuffer[_bufferIndex], pan);
        Raylib.SetSoundPitch(_soundBuffer[_bufferIndex], pitch);
        Raylib.SetSoundVolume(_soundBuffer[_bufferIndex], volume * (float)Settings.Saved.SFXVolume);
        Raylib.PlaySound(_soundBuffer[_bufferIndex]);
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
    public static Font Font;
    private static Font _accessibleFont;
    private static Font _defaultFont;
    public static Music MusicPlaying;

    public static void PreLoad()
    {
        MissingTexture = Raylib.LoadTexture("resources/sprites/missingtex.png");
        
        _accessibleFont = Raylib.LoadFont("resources/pixelplay16.png");
        _defaultFont = Raylib.LoadFont("resources/alagard.png");
        Font = Settings.Saved.AccessibleFont ? _accessibleFont : _defaultFont;
    }
    
    public static void Load()
    {
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/music"))
        {
            MusicResource r = new MusicResource(Path.GetFileNameWithoutExtension(path), Raylib.LoadMusicStream("resources/music/" + Path.GetFileName(path)));
            _music.Add(r.Name, r);
        }

        MusicPlaying = _music["unreal_technology_demo_95_-_unreals"].Music;
        Raylib.PlayMusicStream(MusicPlaying);
        Raylib.SetMusicVolume(MusicPlaying, 0f); // Purge the random garbage from the music stream
        Raylib.UpdateMusicStream(MusicPlaying);
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sprites"))
        {
            SpriteResource r = new SpriteResource(Path.GetFileNameWithoutExtension(path), Raylib.LoadTexture("resources/sprites/" + Path.GetFileName(path)));
            _sprites.Add(r.Name, r);
        }
        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sfx"))
        {
            SoundResource r = new SoundResource(Path.GetFileNameWithoutExtension(path), Raylib.LoadSound("resources/sfx/" + Path.GetFileName(path)), 16);
            _sounds.Add(r.Name, r);
        }

        //Sounds.Add(new SoundResource("explosion", LoadSound("resources/sfx/explosion.wav"), 8));
    }
    
    public static Texture2D GetTextureByName(string name)
    {
        if (!_sprites.ContainsKey(name)) return MissingTexture;
        return _sprites[name].Tex;
    }
    
    public static SoundResource GetSoundByName(string name)
    {
        if (!_sounds.ContainsKey(name)) return _sounds["shovel"];
        return _sounds[name];
    }

    public static void PlayMusicByName(string name)
    {
        Raylib.StopMusicStream(MusicPlaying);
        
        MusicResource? s = null;
        if (_music.ContainsKey(name)) s = _music[name];
        
        if (s != null)
        {
            MusicPlaying = s.Music;
            Raylib.SetMusicVolume(MusicPlaying, (float)Settings.Saved.MusicVolume * 0.3f);
            Raylib.PlayMusicStream(MusicPlaying);

            while (!Raylib.IsMusicValid(MusicPlaying))
            {
                GameConsole.WriteLine("Music isn't valid yet!");
            }
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
        Raylib.SetMusicVolume(MusicPlaying, (float)Settings.Saved.MusicVolume * 0.3f);
    }

    public static void Unload()
    {
        foreach (KeyValuePair<string, SpriteResource> spriteResource in _sprites.ToArray())
        {
            Raylib.UnloadTexture(spriteResource.Value.Tex);
        }
        _sprites.Clear();
    }

    public static void SetFontAccessibility(bool accessible)
    {
        Font = accessible ? _accessibleFont : _defaultFont;
    }
}
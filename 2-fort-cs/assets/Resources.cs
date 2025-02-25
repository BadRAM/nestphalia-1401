using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public class SpriteResource
{
    public string Name;
    public Texture Tex;

    public SpriteResource(string name, Texture tex)
    {
        Name = name;
        Tex = tex;
    }
}

public static class Resources
{
    public static Texture MissingTexture;
    public static List<SpriteResource> Sprites = new List<SpriteResource>();
    public static List<Fort> CampaignLevels = new List<Fort>();
    public static Font Font;
    private static Font _accessibleFont;
    private static Font _defaultFont;
    private static bool _fontAccessibility;
    
    public static void Load()
    {
        MissingTexture = LoadTexture("resources/sprites/missingtex.png");
        _accessibleFont = LoadFont("resources/pixelplay16.png");
        _defaultFont = LoadFont("resources/alagard.png");
        Font = _defaultFont;
        RayGui.GuiSetFont(Font);
        RayGui.GuiSetStyle(0, 16, 12);

        
        foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/sprites"))
        {
            Sprites.Add(new SpriteResource(Path.GetFileNameWithoutExtension(path), LoadTexture("resources/sprites/" + Path.GetFileName(path))));
        }
        
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
    
    public static Texture GetTextureByName(string name)
    {
        SpriteResource? s = Sprites.FirstOrDefault(x => x.Name == name);
        return s?.Tex ?? MissingTexture;
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

    public static void SaveFort(string fortName)
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
        File.WriteAllText(Directory.GetCurrentDirectory() + $"/{fortName}.fort", jsonString);
        
        //if (right) World.Flip();
    }

    public static Fort LoadFort(string filename)
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + filename);
        Fort fort = JsonSerializer.Deserialize<Fort>(jsonString);
        return fort;
    }
}
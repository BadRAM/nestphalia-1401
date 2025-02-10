using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class Campaign
{
    [JsonInclude] public float Money = 1000;
    [JsonInclude] public float Battles;
    [JsonInclude] public int Level;
    [JsonInclude] public List<FloorTileTemplate> Inventory = new List<FloorTileTemplate>();
    [JsonInclude] public Fort Fort1 = new Fort();
    [JsonInclude] public Fort Fort2 = new Fort();
    [JsonInclude] public Fort Fort3 = new Fort();
    [JsonInclude] public int SelectedFort;
    private int _selectedLevel = -1;

    public void Save()
    {
        string jsonString = JsonSerializer.Serialize(this);
        Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/campaign.sav", jsonString);
    }

    public static Campaign Load()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/campaign.sav");
        return JsonSerializer.Deserialize<Campaign>(jsonString) ?? throw new NullReferenceException("Failed to deserialize campaign save file");
    }

    public void Start()
    {
        _selectedLevel = -1;
        Program.CurrentScene = Scene.Campaign;
        Save();
    }
    
    public void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_FOUR) && Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT)) // ye ole moneyhacks
        {
            Money += 1000;
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.BLACK);
        
        Raylib.DrawText($"Bug Dollars: ${Money}", 10, 600, 16, Raylib.WHITE);

        
        if (RayGui.GuiButton(new Rectangle(20,   50, 200, 40), "Edit Fort") != 0) EditorScene.Start(Fort1);
        // if (RayGui.GuiButton(new Rectangle(20,  100, 200, 40), "Edit Fort 2") != 0) EditorScene.Start(Fort2);
        // if (RayGui.GuiButton(new Rectangle(20,  150, 200, 40), "Edit Fort 3") != 0) EditorScene.Start(Fort3);
        if (Level >= 7 && RayGui.GuiButton(new Rectangle(960,  50, 200, 40), "Level 8") != 0) _selectedLevel = 7;
        if (Level >= 6 && RayGui.GuiButton(new Rectangle(960, 100, 200, 40), "Level 7") != 0) _selectedLevel = 6;
        if (Level >= 5 && RayGui.GuiButton(new Rectangle(960, 150, 200, 40), "Level 6") != 0) _selectedLevel = 5;
        if (Level >= 4 && RayGui.GuiButton(new Rectangle(960, 200, 200, 40), "Level 5") != 0) _selectedLevel = 4;
        if (Level >= 3 && RayGui.GuiButton(new Rectangle(960, 250, 200, 40), "Level 4") != 0) _selectedLevel = 3;
        if (Level >= 2 && RayGui.GuiButton(new Rectangle(960, 300, 200, 40), "Level 3") != 0) _selectedLevel = 2;
        if (Level >= 1 && RayGui.GuiButton(new Rectangle(960, 350, 200, 40), "Level 2") != 0) _selectedLevel = 1;
        if (Level >= 0 && RayGui.GuiButton(new Rectangle(960, 400, 200, 40), "Level 1") != 0) _selectedLevel = 0;

        if (_selectedLevel != -1)
        {
            Raylib.DrawText($"{Resources.CampaignLevels[_selectedLevel].Name}\n{Resources.CampaignLevels[_selectedLevel].Comment}\nTravel cost: {_selectedLevel * 250} \nPrize: {(_selectedLevel+1) * 1000}", 960, 550, 16, Raylib.WHITE);
            if (Money >= _selectedLevel * 250 && RayGui.GuiButton(new Rectangle(960, 650, 200, 40), "To Battle!") != 0)
            {
                Money -= _selectedLevel * 250;
                BattleScene.Start(Fort1, Resources.CampaignLevels[_selectedLevel]);
            }
        }
        
        Raylib.EndDrawing();
    }
    
    public void ReportBattleOutcome(bool win)
    {
        Battles++;
        if (win)
        {
            Money += 1000 * (_selectedLevel + 1);
            if (Level == _selectedLevel)
            {
                Level++;
            }
        }
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class Campaign
{
    [JsonInclude] public double Money = 1000;
    [JsonInclude] public double Battles;
    [JsonInclude] public int Level;
    [JsonInclude] public List<FloorTileTemplate> Inventory = new List<FloorTileTemplate>();
    [JsonInclude] public Fort Fort1 = new Fort();
    [JsonInclude] public Fort Fort2 = new Fort();
    [JsonInclude] public Fort Fort3 = new Fort();
    [JsonInclude] public int SelectedFort;
    private int _selectedLevel = -1;
    private string _outcomeText = "";

    public void Save()
    {
        string jsonString = JsonSerializer.Serialize(this);
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
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
        
        Raylib.DrawText(_outcomeText, 10, 400, 16, Raylib.WHITE);

        Raylib.DrawText($"Bug Dollars: ${Money}", 10, 350, 16, Raylib.WHITE);

        
        if (RayGui.GuiButton(new Rectangle(20,   50, 200, 40), "Edit Fort") != 0) EditorScene.Start(Fort1);
        // if (RayGui.GuiButton(new Rectangle(20,  100, 200, 40), "Edit Fort 2") != 0) EditorScene.Start(Fort2);
        // if (RayGui.GuiButton(new Rectangle(20,  150, 200, 40), "Edit Fort 3") != 0) EditorScene.Start(Fort3);
        if (Level >= 7 && RayGui.GuiButton(new Rectangle(960,  50, 200, 40), Resources.CampaignLevels[7].Name) != 0) _selectedLevel = 7;
        if (Level >= 6 && RayGui.GuiButton(new Rectangle(960, 100, 200, 40), Resources.CampaignLevels[6].Name) != 0) _selectedLevel = 6;
        if (Level >= 5 && RayGui.GuiButton(new Rectangle(960, 150, 200, 40), Resources.CampaignLevels[5].Name) != 0) _selectedLevel = 5;
        if (Level >= 4 && RayGui.GuiButton(new Rectangle(960, 200, 200, 40), Resources.CampaignLevels[4].Name) != 0) _selectedLevel = 4;
        if (Level >= 3 && RayGui.GuiButton(new Rectangle(960, 250, 200, 40), Resources.CampaignLevels[3].Name) != 0) _selectedLevel = 3;
        if (Level >= 2 && RayGui.GuiButton(new Rectangle(960, 300, 200, 40), Resources.CampaignLevels[2].Name) != 0) _selectedLevel = 2;
        if (Level >= 1 && RayGui.GuiButton(new Rectangle(960, 350, 200, 40), Resources.CampaignLevels[1].Name) != 0) _selectedLevel = 1;
        if (Level >= 0 && RayGui.GuiButton(new Rectangle(960, 400, 200, 40), Resources.CampaignLevels[0].Name) != 0) _selectedLevel = 0;

        if (_selectedLevel != -1)
        {
            Raylib.DrawText($"{Resources.CampaignLevels[_selectedLevel].Name}\nTravel cost: {_selectedLevel * 250} \nPrize: {(_selectedLevel+1) * 1000}", 760, 400, 16, Raylib.WHITE);
            if (Money >= _selectedLevel * 250 && RayGui.GuiButton(new Rectangle(760, 500, 200, 40), "To Battle!") != 0)
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
            _outcomeText = $"You destroyed {Resources.CampaignLevels[_selectedLevel].Name}!\n${1000 * (_selectedLevel + 1)} prize earned.";
            Money += 1000 * (_selectedLevel + 1);
            if (Level == _selectedLevel)
            {
                Level++;
                _outcomeText += $"\nNew level and structure unlocked!";
                if (Level == 8)
                {
                    _outcomeText = "The capitol has fallen, you are the new bug emperor! \nCongratulations!";
                }
            }
        }
        else
        {
            _outcomeText = "You were beaten back...";
        }
    }
}
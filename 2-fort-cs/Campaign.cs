using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class Campaign
{
    [JsonInclude] public float Money;
    [JsonInclude] public float Battles;
    [JsonInclude] public int Level;
    [JsonInclude] public List<FloorTileTemplate> Inventory;
    [JsonInclude] public Fort Fort1;
    [JsonInclude] public Fort Fort2;
    [JsonInclude] public Fort Fort3;
    [JsonInclude] public int SelectedFort;

    public void Save()
    {
        string jsonString = JsonSerializer.Serialize(this);
        Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/save/campaign.sav", jsonString);
    }

    public static Campaign Load()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/save/campaign.sav");
        return JsonSerializer.Deserialize<Campaign>(jsonString);
    }

    public void Start()
    {
        Fort1 = Resources.CampaignLevels[0];
        Program.CurrentScene = Scene.Campaign;
    }

    public void Update()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.BLACK);
        
        if (RayGui.GuiButton(new Rectangle(20,   50, 200, 40), "Edit Fort 1") != 0) EditorScene.Start();
        if (RayGui.GuiButton(new Rectangle(20,  100, 200, 40), "Edit Fort 2") != 0) EditorScene.Start();
        if (RayGui.GuiButton(new Rectangle(20,  150, 200, 40), "Edit Fort 3") != 0) EditorScene.Start();
        if (RayGui.GuiButton(new Rectangle(960,  50, 200, 40), "Level 8") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[7]);
        if (RayGui.GuiButton(new Rectangle(960, 100, 200, 40), "Level 7") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[6]);
        if (RayGui.GuiButton(new Rectangle(960, 150, 200, 40), "Level 6") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[5]);
        if (RayGui.GuiButton(new Rectangle(960, 200, 200, 40), "Level 5") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[4]);
        if (RayGui.GuiButton(new Rectangle(960, 250, 200, 40), "Level 4") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[3]);
        if (RayGui.GuiButton(new Rectangle(960, 300, 200, 40), "Level 3") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[2]);
        if (RayGui.GuiButton(new Rectangle(960, 350, 200, 40), "Level 2") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[1]);
        if (RayGui.GuiButton(new Rectangle(960, 400, 200, 40), "Level 1") != 0) BattleScene.Start(Fort1, Resources.CampaignLevels[0]);
        
        Raylib.EndDrawing();
    }
}
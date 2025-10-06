using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class CampaignSaveData
{
    public double Money = 2000;
    public List<string> BeatenLevels = new List<string>();
    public List<string> UnlockedStructures = new List<string>();
    public List<string> NewUnlocks = new List<string>();
    public Color TeamColor = Color.Blue;
    
    // Stats
    public int Battles;
    public int BattlesWon;
    public int BattlesLost;
    public int StructuresDestroyed;
    public int TowersDestroyed;
    public double MoneyEarned;
    public double MoneySpent;
    public double TimeInEditor;
    public double TimeInBattle;
    
    public void Save()
    {
        string jsonString = JObject.FromObject(this).ToString();
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/campaign.sav", jsonString);
    }
    
    public static CampaignSaveData Load()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/campaign.sav");
        return JObject.Parse(jsonString).ToObject<CampaignSaveData>() ?? throw new NullReferenceException("Failed to deserialize campaign save file");
    }
    
    public int GetNestCap()
    {
        return BeatenLevels.Count * 2 + 10;
    }
}

public class CampaignScene : Scene
{
    private CampaignSaveData _data;
    private Fort? _fort;
    private string _fortValidityMessage = "";
    private string _outcomeText = "";
    private CampaignScreen _screen = CampaignScreen.Map;
    private Level? _selectedLevel;
    private List<Level> _levels = new List<Level>();

    private Vector3 _colorSelectHSV;

    private Texture2D _levelIcon;

    enum CampaignScreen
    {
        Map,
        Team,
        Settings
    }

    public CampaignScene()
    {
        if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
        {
            _data = CampaignSaveData.Load();
        }
        else
        {
            _data = new CampaignSaveData();
            _data.UnlockedStructures = new List<string>() {"wall_mud", "tower_basic", "nest_ant"};
            _data.Save();
        }
    }

    public void Start(Fort? fort = null)
    {
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        _data.Save();
        // Resources.PlayMusicByName("hook_-_paranoya");
        Resources.PlayMusicByName("nd_credits_live");
        if (fort != null && !Path.EndsInDirectorySeparator(fort.Path)) _fort = fort;
        _levels.Clear();
        foreach (Level level in Assets.GetAll<Level>())
        {
            bool locked = false;
            foreach (string levelID in level.Prerequisites)
            {
                if (!_data.BeatenLevels.Contains(levelID))
                {
                    locked = true;
                }
            }
            if(!locked) _levels.Add(level);
        }
        _levelIcon = Resources.GetTextureByName("level_icon");
        
        if (_fort != null)
        {
            _fort = Fort.LoadFromDisc(_fort.Path);
            _fortValidityMessage = _fort.IsValid(_data);
        }

        _colorSelectHSV = Raylib.ColorToHSV(_data.TeamColor);
    }
    
    public override void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Four) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole moneyhacks
        {
            _data.Money += 1000;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Three) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unmoneyhacks
        {
            _data.Money -= 1000;
        }
        

        
        Screen.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Screen.DrawBackground(Color.DarkBrown);

        switch (_screen)
        {
            case CampaignScreen.Map:
                MapScreen();
                break;
            case CampaignScreen.Team:
                TeamScreen();
                break;
            case CampaignScreen.Settings:
                SettingsScreen();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MapScreen()
    {
        int mapSize = 540;
        Rectangle mapRect = new Rectangle(Screen.CenterX - 70, Screen.CenterY - mapSize / 2, mapSize, mapSize);
        Raylib.DrawRectangleRec(mapRect, Color.Beige);
        Raylib.DrawRectangle(Screen.CenterX-470, Screen.CenterY-350, 380, 700, Color.Brown);
        Raylib.DrawRectangle((int)mapRect.X, Screen.CenterY-350, 360, 60, Color.Brown);
        GUI.DrawTextLeft(-68, -348, $"Bug Dollars: ${_data.Money}", size:32);
        
        for (int i = 0; i < _levels.Count; i++)
        {
            foreach (string level in _levels[i].Prerequisites)
            {
                Raylib.DrawLineEx(mapRect.Center() + _levels[i].Location.ToVector2(), mapRect.Center() + Assets.Get<Level>(level).Location.ToVector2(), 3, Color.DarkGreen);
            } 
        }
        for (int i = 0; i < _levels.Count; i++)
        {
            Rectangle rect = new Rectangle(_levels[i].Location.ToVector2() - new Vector2(16, 16), 32, 32);
            if (GUI.Button(rect, "", _levelIcon, anchor: mapRect.Center()))
            {
                _selectedLevel = _levels[i];
            }
        }
        
        if (_selectedLevel != null)
        {
            GUI.DrawTextLeft(-460, -340, $"{_selectedLevel.Name}\n" +
                                         $"{_selectedLevel.Description}\n" +
                                         $"Reward: {_selectedLevel.MoneyReward}");
        }
        
        string battleButtonText = "To Battle!";
        bool battleValid = false;

        if (_fort == null) battleButtonText = "Select a fort!";
        else if (_fortValidityMessage != "") battleButtonText = _fortValidityMessage;
        else if (_selectedLevel == null) battleButtonText = "Where to?";
        else battleValid = true;
        if (Screen.DebugMode) battleValid = true;

        if (GUI.Button180(300, 300, battleButtonText, battleValid))
        {
            _data.Money -= _fort.TotalCost;
            GameConsole.WriteLine($"Paid {_fort.TotalCost} to challenge {_selectedLevel.Name}. Money before: {_data.Money + _fort.TotalCost}");
            new BattleScene().Start(_selectedLevel, _fort, null, BattleOver);
            World.LeftTeam.Color = _data.TeamColor;
        }

        // Select fort button
        if (GUI.Button180(-460, 310, "Select Fort"))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/Campaign/", 
            i => 
            {
                _fort = i;
                _fortValidityMessage = _fort.IsValid(_data);
            },
            path =>
            {
                Fort f = new Fort(path);
                new EditorScene().Start(Start, f, _data);
            }));
        }
        
        if (GUI.Button180(-280, 310, "Edit Fort", _fort != null)) new EditorScene().Start(Start, _fort, _data);

        if (_fort != null)
        {
            GUI.DrawTextLeft(-460, 0, $"{_fort.Name}\n" +
                                         $"{_fort.Comment}\n" +
                                         $"Cost: {_fort.TotalCost}");
        }
        
        if (GUI.Button180(290, -350, "Quit") || Input.Pressed(Input.InputAction.Exit)) new MenuScene().Start();
        if (GUI.Button180(100, -350, "Settings")) _screen = CampaignScreen.Settings;
        if (GUI.Button180(0, 350, "Team")) _screen = CampaignScreen.Team;
    }

    private void TeamScreen()
    {
        _colorSelectHSV.X = (float)GUI.Slider(0, 0, "Hue", _colorSelectHSV.X/255) * 255;
        _colorSelectHSV.Y = (float)GUI.Slider(0, 40, "Saturation", _colorSelectHSV.Y);
        _colorSelectHSV.Z = (float)GUI.Slider(0, 80, "Value", _colorSelectHSV.Z);

        _data.TeamColor = Raylib.ColorFromHSV(_colorSelectHSV.X, _colorSelectHSV.Y, _colorSelectHSV.Z);
        
        Raylib.DrawCircle(Screen.CenterX - 100, Screen.CenterY, 10, _data.TeamColor);
        
        if (GUI.Button180(0, 200, "Map") || Input.Pressed(Input.InputAction.Exit)) _screen = CampaignScreen.Map;
    }

    private void SettingsScreen()
    {
        if (Settings.DrawSettingsMenu()) _screen = CampaignScreen.Map;
    }
    
    private void BattleOver(Team? winner)
    {
        _data.Battles++;
        if (winner?.IsPlayerControlled ?? false)
        {
            _outcomeText = $"You destroyed {_selectedLevel!.Name}!\n${_selectedLevel.MoneyReward} prize earned.";
            _data.Money += _selectedLevel.MoneyReward;
            if (!_data.BeatenLevels.Contains(_selectedLevel.ID))
            {
                _data.BeatenLevels.Add(_selectedLevel.ID);
            }
            _data.NewUnlocks.Clear();
            foreach (string unlock in _selectedLevel.UnlockReward)
            {
                if (!_data.UnlockedStructures.Contains(unlock))
                {
                    _data.UnlockedStructures.Add(unlock);
                    _data.NewUnlocks.Add(unlock);
                }
            }
        }
        else
        {
            _outcomeText = $"You were beaten back.\nyour workers salvage ${Math.Floor(_fort!.TotalCost/2)} worth of materials.";
            _data.Money += Math.Floor(_fort.TotalCost / 2);
        }

        if (_data.Money < 2000 || Random.Shared.Next(20) == 0)
        {
            int bailout = Math.Max(2000 - (int)_data.Money, 500) + Random.Shared.Next(500);
            _outcomeText += $"\nYour scouts uncover an ancient hoard! +${bailout}";
            _data.Money += bailout;
        }
        
        Start();
        
        PopupManager.Start(new AlertPopup("Battle Over!", _outcomeText, winner?.IsPlayerControlled ?? false ? "Hooray!" : "Darn", () => {}));
    }
}
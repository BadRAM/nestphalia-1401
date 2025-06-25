using System.Collections;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;

namespace nestphalia;

public class CampaignSaveData
{
    [JsonInclude] public double Money = 2000;
    [JsonInclude] public double Battles;
    [JsonInclude] public int Level;
    [JsonInclude] public List<string> UnlockedStructures;
    [JsonInclude] public bool NewUtil = false;
    [JsonInclude] public bool NewTower = false;
    [JsonInclude] public bool NewNest = false;

    public void Save()
    {
        string jsonString = JsonSerializer.Serialize(this, SourceGenerationContext.Default.CampaignSaveData);
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/campaign.sav", jsonString);
    }

    public static CampaignSaveData Load()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/campaign.sav");
        return JsonSerializer.Deserialize<CampaignSaveData>(jsonString, SourceGenerationContext.Default.CampaignSaveData) ?? throw new NullReferenceException("Failed to deserialize campaign save file");
    }
    
    public int GetNestCap()
    {
        return Level * 2 + 10;
    }
}

public class CampaignScene : Scene
{
    private CampaignSaveData _data;
    private int _selectedFort = -1;
    private Fort? _fort;
    private string _fortValidityMessage = "";
    private Level? _selectedLevel;
    private string _outcomeText = "";
    private string _activeDirectory = "";
    private string[] _directories;
    private string[] _fortFiles;
    private int _fortListPage = 1;
    private double _prize;
    private List<Level> _levels = new List<Level>();

    public CampaignScene()
    {
        if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
        {
            _data = CampaignSaveData.Load();
        }
        else
        {
            _data = new CampaignSaveData();
            _data.Save();
        }
    }

    public void Start()
    {
        // _selectedLevel = null;
        _selectedFort = -1;
        _prize = 0;
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        _data.Save();
        Resources.PlayMusicByName("hook_-_paranoya");
        _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
        _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
        _levels = new List<Level>(Assets.GetAllLevels());
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
        if (Raylib.IsKeyPressed(KeyboardKey.Five) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole progresshacks
        {
            _data.Level++;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Six) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unprogresshacks
        {
            _data.Level++;
        }
        
        if (Input.Pressed(Input.InputAction.Exit))
        {
            new MenuScene().Start();
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Screen.DrawBackground(Color.DarkBrown);
        
        GUI.DrawTextLeft(-200, -270, _outcomeText);

        GUI.DrawTextLeft(-200, -290, $"Bug Dollars: ${_data.Money}");
        
        ListForts();

        for (int i = 0; i < _levels.Count; i++)
        {
            if (GUI.Button300(300, -300 + i*40, _levels[i].Name)) SelectOpponent(_levels[i]);
        }
        
        // if (_data.Level >= 14 && GUI.Button300(300, -300, Resources.CampaignLevels[14].Name)) SelectOpponent(14);
        // if (_data.Level >= 13 && GUI.Button300(300, -260, Resources.CampaignLevels[13].Name)) SelectOpponent(13);
        // if (_data.Level >= 12 && GUI.Button300(300, -220, Resources.CampaignLevels[12].Name)) SelectOpponent(12);
        // if (_data.Level >= 11 && GUI.Button300(300, -180, Resources.CampaignLevels[11].Name)) SelectOpponent(11);
        // if (_data.Level >= 10 && GUI.Button300(300, -140, Resources.CampaignLevels[10].Name)) SelectOpponent(10);
        // if (_data.Level >=  9 && GUI.Button300(300, -100, Resources.CampaignLevels[ 9].Name)) SelectOpponent( 9);
        // if (_data.Level >=  8 && GUI.Button300(300,  -60, Resources.CampaignLevels[ 8].Name)) SelectOpponent( 8);
        // if (_data.Level >=  7 && GUI.Button300(300,  -20, Resources.CampaignLevels[ 7].Name)) SelectOpponent( 7);
        // if (_data.Level >=  6 && GUI.Button300(300,   20, Resources.CampaignLevels[ 6].Name)) SelectOpponent( 6);
        // if (_data.Level >=  5 && GUI.Button300(300,   60, Resources.CampaignLevels[ 5].Name)) SelectOpponent( 5);
        // if (_data.Level >=  4 && GUI.Button300(300,  100, Resources.CampaignLevels[ 4].Name)) SelectOpponent( 4);
        // if (_data.Level >=  3 && GUI.Button300(300,  140, Resources.CampaignLevels[ 3].Name)) SelectOpponent( 3);
        // if (_data.Level >=  2 && GUI.Button300(300,  180, Resources.CampaignLevels[ 2].Name)) SelectOpponent( 2);
        // if (_data.Level >=  1 && GUI.Button300(300,  220, Resources.CampaignLevels[ 1].Name)) SelectOpponent( 1);
        // if (_data.Level >=  0 && GUI.Button300(300,  260, Resources.CampaignLevels[ 0].Name)) SelectOpponent( 0);

        if (_selectedLevel != null)
        {
            if (_selectedFort != -1)
            {
                if (_fortValidityMessage == "")
                {
                    GUI.DrawTextLeft(-140, -60, $"{_selectedLevel.Name}\nFort cost: {_fort!.TotalCost} \nPrize: {_prize}");
                    if (GUI.Button300(-150, 20, "To Battle!"))
                    {
                        _data.Money -= _fort.TotalCost;
                        new BattleScene().Start(_selectedLevel, _fort, null, BattleOver);
                    }
                }
                else
                {
                    GUI.DrawTextLeft(-140, -60, $"{_selectedLevel.Name}\nFort cost: {_fort!.TotalCost} \nPrize: {_prize}\n{_fortValidityMessage}");
                    GUI.Button300(-150, 20, "To Battle!", false);
                }
            }
            else
            {
                GUI.DrawTextLeft(-140, -60, $"{_selectedLevel.Name}\nNo fort design selected! \nPrize: {_prize}");
                GUI.Button300(-150, 20, "To Battle!", false);
            }
        }
        
        if (_selectedFort != -1 && GUI.Button300(-600, -300, $"Edit {_fort!.Name}" )) new EditorScene().Start(Start, _fort, _data);
        
        if (GUI.Button300(-600, 260, "Quit"))
        {
            new MenuScene().Start();
        }
    }

    private void BattleOver(Team? winner)
    {
        _data.Battles++;
        if (winner?.IsPlayerControlled ?? false)
        {
            _outcomeText = $"You destroyed {_selectedLevel!.Name}!\n${_prize} prize earned.";
            _data.Money += _prize;
            // if (_data.Level == _selectedLevel)
            // {
            //     _data.Level++;
            //     _outcomeText += $"\nNew level and structure unlocked!";
            //     if (_data.Level == 15)
            //     {
            //         _outcomeText = "The capitol has fallen, you are the new bug emperor! \nCongratulations!";
            //     }
            // }
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
    }
    
    private void ListForts()
    {
        if (GUI.Button100(-600, 260, "<", _fortListPage > 1)) _fortListPage--;
        GUI.Button100(-500, 260, _fortListPage.ToString(), false); // this one's just a number display box
        if (GUI.Button100(-400, 260, ">", _fortListPage <= ((_activeDirectory == "" ? 0 : 1) + _directories.Length + _fortFiles.Length)/12)) _fortListPage++;
        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage - 1) * 12 + (_activeDirectory != "" ? -1 : 0);
            if (index >= _directories.Length + _fortFiles.Length + 1) break;
            if (index == -1) // This is the 'return to parent folder' button
            {
                if (GUI.Button300(-600, i * 40 - 240, "^  Return to Parent Folder  ^"))
                {
                    _selectedFort = -1;
                    _activeDirectory = "";
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length) // This is a directory
            {
                if (GUI.Button300(-600, i * 40 - 240,
                        $"/{Path.GetFileName(_directories[index])}/"))
                {
                    _selectedFort = -1;
                    _activeDirectory = Path.GetFileName(_directories[index]);
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length + _fortFiles.Length) // This is a fort
            {
                string fortPath = _fortFiles[index - _directories.Length].Substring(Directory.GetCurrentDirectory().Length);
                if (GUI.Button300(-600, i * 40 - 240,
                        Path.GetFileNameWithoutExtension(fortPath), _selectedFort != index))
                {
                    GameConsole.WriteLine("Loading " + Path.GetFileName(fortPath));
                    _fort = Resources.LoadFort(fortPath);
                    _fort.Name = Path.GetFileNameWithoutExtension(fortPath);
                    _fort.Comment = _fort.FortSummary();
                    // _fort.Path = fortPath;
                    _fortValidityMessage = _fort.IsValid(_data);
                    _selectedFort = index;
                }
            }
            else
            {
                if (GUI.Button300(-600, i * 40 - 240, "+  New Fort  +"))
                {
                    string path = "/forts/Campaign/" + _activeDirectory;
                    Fort f = new Fort(Resources.GetUnusedFortName(path), path);
                    new EditorScene().Start(Start, f, _data);
                }
            }
        }
    }

    private void SelectOpponent(Level select)
    {
        _selectedLevel = select;
        // _selectedLevel.UpdateCost();
        _prize = _selectedLevel.MoneyReward;
    }
}
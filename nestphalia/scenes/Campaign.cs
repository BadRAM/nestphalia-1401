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
    public CampaignSaveData Data;
    private int _selectedFort = -1;
    private Fort? _fort;
    private string _fortValidityMessage = "";
    private int _selectedLevel = -1;
    private string _outcomeText = "";
    private string _activeDirectory = "";
    private string[] _directories;
    private string[] _fortFiles;
    private int _fortListPage = 1;
    private double _prize;

    public CampaignScene()
    {
        // // todo: find a way to move this check into Campaign.cs
        if (File.Exists(Directory.GetCurrentDirectory() + "/campaign.sav"))
        {
            Data = CampaignSaveData.Load();
        }
        else
        {
            Data = new CampaignSaveData();
            Data.Save();
        }
    }

    public void Start()
    {
        _selectedLevel = -1;
        _selectedFort = -1;
        _prize = 0;
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        Data.Save();
        Resources.PlayMusicByName("hook_-_paranoya");
        _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
        _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
    }
    
    public override void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Four) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole moneyhacks
        {
            Data.Money += 1000;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Three) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unmoneyhacks
        {
            Data.Money -= 1000;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Five) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole progresshacks
        {
            Data.Level++;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Six) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unprogresshacks
        {
            Data.Level++;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            new MenuScene().Start();
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Screen.DrawBackground(Color.DarkBrown);
        
        GUI.DrawTextLeft(Screen.HCenter-200, Screen.VCenter-270, _outcomeText);

        GUI.DrawTextLeft(Screen.HCenter-200, Screen.VCenter-290, $"Bug Dollars: ${Data.Money}");
        
        ListForts();
        
        if (Data.Level >= 14 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, Resources.CampaignLevels[14].Name)) SelectOpponent(14);
        if (Data.Level >= 13 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-260, Resources.CampaignLevels[13].Name)) SelectOpponent(13);
        if (Data.Level >= 12 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-220, Resources.CampaignLevels[12].Name)) SelectOpponent(12);
        if (Data.Level >= 11 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-180, Resources.CampaignLevels[11].Name)) SelectOpponent(11);
        if (Data.Level >= 10 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-140, Resources.CampaignLevels[10].Name)) SelectOpponent(10);
        if (Data.Level >=  9 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-100, Resources.CampaignLevels[ 9].Name)) SelectOpponent( 9);
        if (Data.Level >=  8 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter- 60, Resources.CampaignLevels[ 8].Name)) SelectOpponent( 8);
        if (Data.Level >=  7 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter- 20, Resources.CampaignLevels[ 7].Name)) SelectOpponent( 7);
        if (Data.Level >=  6 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+ 20, Resources.CampaignLevels[ 6].Name)) SelectOpponent( 6);
        if (Data.Level >=  5 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+ 60, Resources.CampaignLevels[ 5].Name)) SelectOpponent( 5);
        if (Data.Level >=  4 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+100, Resources.CampaignLevels[ 4].Name)) SelectOpponent( 4);
        if (Data.Level >=  3 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+140, Resources.CampaignLevels[ 3].Name)) SelectOpponent( 3);
        if (Data.Level >=  2 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+180, Resources.CampaignLevels[ 2].Name)) SelectOpponent( 2);
        if (Data.Level >=  1 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+220, Resources.CampaignLevels[ 1].Name)) SelectOpponent( 1);
        if (Data.Level >=  0 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+260, Resources.CampaignLevels[ 0].Name)) SelectOpponent( 0);

        if (_selectedLevel != -1)
        {
            if (_selectedFort != -1)
            {
                if (_fortValidityMessage == "")
                {
                    GUI.DrawTextLeft(Screen.HCenter-140, Screen.VCenter-60, $"{Resources.CampaignLevels[_selectedLevel].Name}\nFort cost: {_fort.TotalCost} \nPrize: {_prize}");
                    if (Data.Money >= _selectedLevel * 250 && GUI.ButtonWide(Screen.HCenter-150, Screen.VCenter+20, "To Battle!"))
                    {
                        Data.Money -= _fort.TotalCost;
                        new BattleScene().Start(_fort, Resources.CampaignLevels[_selectedLevel], BattleOver);
                    }
                }
                else
                {
                    GUI.DrawTextLeft(Screen.HCenter-140, Screen.VCenter-60, $"{Resources.CampaignLevels[_selectedLevel].Name}\nFort cost: {_fort.TotalCost} \nPrize: {_prize}\n{_fortValidityMessage}");
                    GUI.ButtonWide(Screen.HCenter - 150, Screen.VCenter + 20, "To Battle!", false);
                }
            }
            else
            {
                GUI.DrawTextLeft(Screen.HCenter-140, Screen.VCenter-60, $"{Resources.CampaignLevels[_selectedLevel].Name}\nNo fort design selected! \nPrize: {_prize}");
                GUI.ButtonWide(Screen.HCenter - 150, Screen.VCenter + 20, "To Battle!", false);
            }
        }
        
        if (_selectedFort != -1 && GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter - 300, $"Edit {_fort.Name}" )) new EditorScene().Start(Start, _fort, Data);
        
        if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Quit"))
        {
            new MenuScene().Start();
        }
        
        Raylib.EndDrawing();
    }

    private void BattleOver(Team? winner)
    {
        Data.Battles++;
        if (winner?.IsPlayerControlled ?? false)
        {
            _outcomeText = $"You destroyed {Resources.CampaignLevels[_selectedLevel].Name}!\n${_prize} prize earned.";
            Data.Money += _prize;
            if (Data.Level == _selectedLevel)
            {
                Data.Level++;
                _outcomeText += $"\nNew level and structure unlocked!";
                if (Data.Level == 15)
                {
                    _outcomeText = "The capitol has fallen, you are the new bug emperor! \nCongratulations!";
                }
            }
        }
        else
        {
            _outcomeText = $"You were beaten back.\nyour workers salvage ${Math.Floor(_fort.TotalCost/2)} worth of materials.";
            Data.Money += Math.Floor(_fort.TotalCost / 2);
        }

        if (Data.Money < 2000 || Random.Shared.Next(20) == 0)
        {
            int bailout = Math.Max(2000 - (int)Data.Money, 500) + Random.Shared.Next(500);
            _outcomeText += $"\nYour scouts uncover an ancient hoard! +${bailout}";
            Data.Money += bailout;
        }
        
        Start();
    }
    
    private void ListForts()
    {
        if (GUI.ButtonNarrow(Screen.HCenter - 600, Screen.VCenter + 260, "<", _fortListPage > 1)) _fortListPage--;
            GUI.ButtonNarrow(Screen.HCenter - 500, Screen.VCenter + 260, _fortListPage.ToString(), false);
        if (GUI.ButtonNarrow(Screen.HCenter - 400, Screen.VCenter + 260, ">", _fortListPage <= ((_activeDirectory == "" ? 0 : 1) + _directories.Length + _fortFiles.Length)/12)) _fortListPage++;
        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_fortListPage - 1) * 12 + (_activeDirectory != "" ? -1 : 0);
            if (index >= _directories.Length + _fortFiles.Length + 1) break;
            if (index == -1) // This is the 'return to parent folder' button
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "^  Return to Parent Folder  ^"))
                {
                    _selectedFort = -1;
                    _activeDirectory = "";
                    _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                    _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
                }
            }
            else if (index < _directories.Length) // This is a directory
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240,
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
                string fortPath = _fortFiles[index - _directories.Length];
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240,
                        Path.GetFileNameWithoutExtension(fortPath), _selectedFort != index))
                {
                    Console.WriteLine("Loading " + Path.GetFileName(fortPath));
                    _fort = Resources.LoadFort(fortPath);
                    _fort.Name = Path.GetFileNameWithoutExtension(fortPath);
                    _fort.Comment = _fort.FortSummary();
                    _fort.Path = Path.GetDirectoryName(fortPath);
                    _fortValidityMessage = _fort.IsValid(Data);
                    _selectedFort = index;
                }
            }
            else
            {
                if (GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter + i * 40 - 240, "+  New Fort  +"))
                {
                    Fort f = new Fort();
                    f.Path = Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory;
                    f.Name = Resources.GetUnusedFortName(f.Path);
                    new EditorScene().Start(Start, f, Data);
                }
            }
        }
    }

    private void SelectOpponent(int select)
    {
        _selectedLevel = select;
        Resources.CampaignLevels[_selectedLevel].UpdateCost();
        _prize = Math.Floor(Resources.CampaignLevels[_selectedLevel].TotalCost * 1.5) + (_selectedLevel == 0 ? 2000 : 0);
    }
}
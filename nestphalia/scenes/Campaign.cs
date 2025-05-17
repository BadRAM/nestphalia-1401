using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;

namespace nestphalia;

public class Campaign
{
    [JsonInclude] public double Money = 2000;
    [JsonInclude] public double Battles;
    [JsonInclude] public int Level;
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


    public void Save()
    {
        string jsonString = JsonSerializer.Serialize(this, SourceGenerationContext.Default.Campaign);
        //Console.WriteLine($"JSON campaign looks like: {jsonString}");
        File.WriteAllText(Directory.GetCurrentDirectory() + "/campaign.sav", jsonString);
    }

    public static Campaign Load()
    {
        string jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + "/campaign.sav");
        return JsonSerializer.Deserialize<Campaign>(jsonString, SourceGenerationContext.Default.Campaign) ?? throw new NullReferenceException("Failed to deserialize campaign save file");
    }

    public void Start()
    {
        _selectedLevel = -1;
        _selectedFort = -1;
        _prize = 0;
        Program.CurrentScene = Scene.Campaign;
        Screen.RegenerateBackground();
        Save();
        Resources.PlayMusicByName("hook_-_paranoya");
        _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
        _fortFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _activeDirectory);
    }
    
    public void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Four) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole moneyhacks
        {
            Money += 1000;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Three) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unmoneyhacks
        {
            Money -= 1000;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Five) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole progresshacks
        {
            Level++;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Six) && Raylib.IsKeyDown(KeyboardKey.LeftShift)) // ye ole unprogresshacks
        {
            Level++;
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            MenuScene.Start();
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Screen.DrawBackground(Color.DarkBrown);
        
        GUI.DrawTextLeft(Screen.HCenter-200, Screen.VCenter-270, _outcomeText);

        GUI.DrawTextLeft(Screen.HCenter-200, Screen.VCenter-290, $"Bug Dollars: ${Money}");
        
        ListForts();
        
        if (Level >= 14 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-300, Resources.CampaignLevels[14].Name)) SelectOpponent(14);
        if (Level >= 13 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-260, Resources.CampaignLevels[13].Name)) SelectOpponent(13);
        if (Level >= 12 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-220, Resources.CampaignLevels[12].Name)) SelectOpponent(12);
        if (Level >= 11 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-180, Resources.CampaignLevels[11].Name)) SelectOpponent(11);
        if (Level >= 10 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-140, Resources.CampaignLevels[10].Name)) SelectOpponent(10);
        if (Level >=  9 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter-100, Resources.CampaignLevels[ 9].Name)) SelectOpponent( 9);
        if (Level >=  8 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter- 60, Resources.CampaignLevels[ 8].Name)) SelectOpponent( 8);
        if (Level >=  7 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter- 20, Resources.CampaignLevels[ 7].Name)) SelectOpponent( 7);
        if (Level >=  6 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+ 20, Resources.CampaignLevels[ 6].Name)) SelectOpponent( 6);
        if (Level >=  5 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+ 60, Resources.CampaignLevels[ 5].Name)) SelectOpponent( 5);
        if (Level >=  4 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+100, Resources.CampaignLevels[ 4].Name)) SelectOpponent( 4);
        if (Level >=  3 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+140, Resources.CampaignLevels[ 3].Name)) SelectOpponent( 3);
        if (Level >=  2 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+180, Resources.CampaignLevels[ 2].Name)) SelectOpponent( 2);
        if (Level >=  1 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+220, Resources.CampaignLevels[ 1].Name)) SelectOpponent( 1);
        if (Level >=  0 && GUI.ButtonWide(Screen.HCenter+300, Screen.VCenter+260, Resources.CampaignLevels[ 0].Name)) SelectOpponent( 0);

        if (_selectedLevel != -1)
        {
            if (_selectedFort != -1)
            {
                if (_fortValidityMessage == "")
                {
                    GUI.DrawTextLeft(Screen.HCenter-140, Screen.VCenter-60, $"{Resources.CampaignLevels[_selectedLevel].Name}\nFort cost: {_fort.TotalCost} \nPrize: {_prize}");
                    if (Money >= _selectedLevel * 250 && GUI.ButtonWide(Screen.HCenter-150, Screen.VCenter+20, "To Battle!"))
                    {
                        Money -= _fort.TotalCost;
                        BattleScene.Start(_fort, Resources.CampaignLevels[_selectedLevel], BattleOver);
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
        
        if (_selectedFort != -1 && GUI.ButtonWide(Screen.HCenter - 600, Screen.VCenter - 300, $"Edit {_fort.Name}" )) EditorScene.Start(_fort);
        
        if (GUI.ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Quit"))
        {
            MenuScene.Start();
        }
        
        Raylib.EndDrawing();
    }
    
    public void ReportBattleOutcome(bool win)
    {

    }

    private void BattleOver(Team? winner)
    {
        Battles++;
        if (winner?.IsPlayerControlled ?? false)
        {
            _outcomeText = $"You destroyed {Resources.CampaignLevels[_selectedLevel].Name}!\n${_prize} prize earned.";
            Money += _prize;
            if (Level == _selectedLevel)
            {
                Level++;
                _outcomeText += $"\nNew level and structure unlocked!";
                if (Level == 15)
                {
                    _outcomeText = "The capitol has fallen, you are the new bug emperor! \nCongratulations!";
                }
            }
        }
        else
        {
            _outcomeText = $"You were beaten back.\nyour workers salvage ${Math.Floor(_fort.TotalCost/2)} worth of materials.";
            Money += Math.Floor(_fort.TotalCost / 2);
        }

        if (Money < 2000 || Random.Shared.Next(20) == 0)
        {
            int bailout = Math.Max(2000 - (int)Money, 500) + Random.Shared.Next(500);
            _outcomeText += $"\nYour scouts uncover an ancient hoard! +${bailout}";
            Money += bailout;
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
                    _fortValidityMessage = _fort.IsValid(this);
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
                    EditorScene.Start(f);
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

    public int GetNestCap()
    {
        return Level * 2 + 10;
    }
}
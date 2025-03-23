using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;

namespace nestphalia;

public static class EditorScene
{
    private static bool _sandboxMode;
    private static StructureTemplate? _brush;
    private static EditorTool _toolActive;
    private static Fort _fort;
    private static string _saveMessage;
    private static string _fortStats;
    private static bool _newUtil;
    private static bool _newTower;
    private static bool _newNest;
    private static bool _nestCapped;
    private static bool _beaconCapped;
    private static bool _fortAlreadySaved;
    private static bool _sellAllConfirm;
    private static StructureTemplate.StructureClass _structureClass;
    private static Texture2D _bg;

    private enum EditorTool
    {
        Brush,
        Erase,
        PathTester
    }
    
    public static void Start(Fort? fortToLoad = null, bool creativeMode = false)
    {
        _bg = Resources.GetTextureByName("editor_bg");
        _brush = null;
        _toolActive = EditorTool.Erase;
        _saveMessage = "";
        _sandboxMode = creativeMode;
        _fort = fortToLoad ?? new Fort();
        _fortAlreadySaved = File.Exists(_fort.Path);
        Console.WriteLine($"checking if {Directory.GetCurrentDirectory() +  $"/forts/{_fort.Name}.fort"} exists. Answer: {_fortAlreadySaved.ToString()}");
        
        Program.CurrentScene = Scene.Editor;
        World.InitializeEditor();
        _fort.LoadToBoard(false);
        
        Screen.RegenerateBackground();
        
        
        _newUtil = false;
        _newTower = false;
        _newNest = false;
        
        if (!_sandboxMode)
        {
            foreach (StructureTemplate template in Assets.Structures)
            {
                if (template.LevelRequirement == Program.Campaign.Level)
                {
                    switch (template.Class)
                    {
                        case StructureTemplate.StructureClass.Utility:
                            _newUtil = true;
                            break;
                        case StructureTemplate.StructureClass.Tower:
                            _newTower = true;
                            break;
                        case StructureTemplate.StructureClass.Nest:
                            _newNest = true;
                            break;
                    }
                }
            }
        }

        World.Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter);
        
        Resources.PlayMusicByName("so_lets_get_killed");
    }
    
    public static void Update()
    {
        // ===== INPUT + UPDATE =====

        switch (_toolActive)
        {
            case EditorTool.Brush:
                BrushTool();
                break;
            case EditorTool.Erase:
                EraseTool();
                break;
            case EditorTool.PathTester:
                // Path test tool needs to happen during draw.
                // PathTestTool();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (IsMouseButtonPressed(MouseButton.Right))
        {
            Int2D tilePos = World.GetMouseTilePos();
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21)
            {
                _brush = World.GetTile(tilePos)?.Template;
                if (_brush == null)
                {
                    _toolActive = EditorTool.Erase;
                }
                else
                {
                    _toolActive = EditorTool.Brush;
                    _structureClass = _brush.Class;
                }
            }
        }

        if (IsKeyPressed(KeyboardKey.Escape))
        {
            if (_sandboxMode)
            {
                CustomBattleMenu.Start();
            }
            else
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }
        
        // lazy hack so resizing the window doesn't offset the viewport
        World.Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter); 
        
        // ===== DRAW =====
        
        BeginDrawing();
        ClearBackground(Color.Black);
        Screen.DrawBackground(Color.Gray);
        
        World.DrawFloor();


        
        // Draw gui background texture
        DrawTexture(_bg, Screen.HCenter - 608, Screen.VCenter - 308, Color.White);
        
        // Draw brush preview ghost
        if (CheckCollisionPointRec(GetMousePosition(), new Rectangle(Screen.HCenter-240, Screen.VCenter-232, 480, 480)))
        {
            BeginMode2D(World.Camera);
            Vector2 mousePos = World.GetTileCenter(World.GetMouseTilePos());
            if (_brush is TowerTemplate t)
            {
                DrawCircleLines((int)mousePos.X, (int)mousePos.Y, (int)t.Range, new Color(200, 50, 50, 255));
                //DrawCircle((int)mousePos.X, (int)mousePos.Y, (int)t.Range, new Color(200, 50, 50, 64));
            }
            if (_brush != null)
            {
                DrawTexture(_brush.Texture, (int)mousePos.X-12, (int)mousePos.Y-(_brush.Texture.Height - 12), new Color(128, 128, 255, 128));
            }
            EndMode2D();
        }
        
        World.Draw();
        
        if (_sandboxMode)
        {
            if (ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Exit")) CustomBattleMenu.Start();
            if (ButtonWide(Screen.HCenter-600, Screen.VCenter+180, "Save Changes", _fortAlreadySaved)) Resources.SaveFort(_fort.Name, _fort.Path);
        }
        else
        {
            if (ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Save and Exit"))
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }
        
        if (ButtonWide(Screen.HCenter-600, Screen.VCenter+220, "Save to new file")) SaveToNewFile();
        
        if (ButtonNarrow(Screen.HCenter+150, Screen.VCenter+260, "Sell", _toolActive != EditorTool.Erase))
        {
            _brush = null;
            _toolActive = EditorTool.Erase;
        }
        if (ButtonNarrow(Screen.HCenter+50, Screen.VCenter+260, "Path Preview", _toolActive != EditorTool.PathTester))
        {
            _brush = null;
            _toolActive = EditorTool.PathTester;
        }
        if (_sellAllConfirm && ButtonNarrow(Screen.HCenter-250, Screen.VCenter+260, "Cancel"))   _sellAllConfirm = false;
        else if (!_sellAllConfirm&& ButtonNarrow(Screen.HCenter-250, Screen.VCenter+260, "Sell All")) _sellAllConfirm = true;
        if (_sellAllConfirm && ButtonNarrow(Screen.HCenter-150, Screen.VCenter+260, "Confirm"))  SellAll();
        if (ButtonNarrow(Screen.HCenter+300, Screen.VCenter-300, (_newUtil ? "NEW! " : "") + "Utility", _structureClass != StructureTemplate.StructureClass.Utility)) _structureClass = StructureTemplate.StructureClass.Utility;
        if (ButtonNarrow(Screen.HCenter+400, Screen.VCenter-300, (_newTower ? "NEW! " : "") + "Tower", _structureClass != StructureTemplate.StructureClass.Tower)) _structureClass = StructureTemplate.StructureClass.Tower;
        if (ButtonNarrow(Screen.HCenter+500, Screen.VCenter-300, (_newNest ? "NEW! " : "") + "Nest", _structureClass != StructureTemplate.StructureClass.Nest)) _structureClass = StructureTemplate.StructureClass.Nest;
        
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            StructureTemplate s = Assets.Structures[i];
            if ((!_sandboxMode && s.LevelRequirement > Program.Campaign.Level) || s.Class != _structureClass) continue;
            string label = ((!_sandboxMode && s.LevelRequirement == Program.Campaign.Level) ? "NEW! " : "") + s.Name + " - $" + s.Price;
            if (ButtonWide(Screen.HCenter+300, Screen.VCenter + y * 40 - 250, label, _brush != s))
            {
                _brush = s;
                _toolActive = EditorTool.Brush;
            }
            DrawTexture(s.Texture, Screen.HCenter + 320, Screen.VCenter + y * 40 - 246, Color.White);
            y++;
        }
        
        if (!_sandboxMode)
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter+ 200, $"Bug Dollars: ${Program.Campaign.Money}");
        }
        
        if (_brush == null)
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter-290, "SELLING");
        }
        else
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter-290, $"Placing {_brush.GetStats()}");
        }
        
        DrawTextLeft(Screen.HCenter-590, Screen.VCenter+170, _saveMessage);
        DrawTextLeft(Screen.HCenter-590, Screen.VCenter+50, GetFortStats());
        
        if (_toolActive == EditorTool.PathTester)
        {
            PathTestTool();
        }
        
        EndDrawing();
    }

    private static void PathTestTool()
    {
        NavPath navPath = new NavPath(new Int2D(28, 11), World.GetMouseTilePos(), World.RightTeam);
        PathFinder.FindPath(navPath);
        
        BeginMode2D(World.Camera);
        
        Vector2 path = World.GetTileCenter(navPath.Start);
        foreach (Int2D i in navPath.Waypoints)
        {
            Vector2 v = World.GetTileCenter(i);
            if (i.X < 22)
            {
                DrawLineEx(path, v, 2, Color.Lime);
            }
            path = v;
        }

        // if (navPath.Waypoints.Count == 0)
        // {
        //     Vector2 v = World.GetTileCenter(navPath.Destination);
        //     DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
        // }
        
        EndMode2D();
    }

    private static void EraseTool()
    {
        if (IsMouseButtonDown(MouseButton.Left))
        {
            Int2D tilePos = World.GetMouseTilePos();
            
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21 
                && _brush != World.GetTile(tilePos)?.Template)
            {
                World.SetTile(null, World.LeftTeam, tilePos);
                if (!_sandboxMode)
                {
                    Program.Campaign.Money += World.GetTile(tilePos)?.Template.Price ?? 0;
                }
            }
        }
    }

    private static void BrushTool()
    {
        if (_brush == null) 
        {
            Console.WriteLine("Tried to use null brush!");
            _toolActive = EditorTool.Erase;
            return;
        }
        if (IsMouseButtonDown(MouseButton.Left))
        {
            Int2D tilePos = World.GetMouseTilePos();
            
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21 
                && _brush != World.GetTile(tilePos)?.Template)
            {
                if (_sandboxMode)
                {
                    World.SetTile(_brush, World.LeftTeam, tilePos);
                }
                else if ( _brush.Price <= Program.Campaign.Money && 
                          (_brush.Class != StructureTemplate.StructureClass.Nest || !_nestCapped || World.GetTile(tilePos) is Spawner) && 
                          (_brush is not ActiveAbilityBeaconTemplate || !_beaconCapped || World.GetTile(tilePos) is ActiveAbilityBeacon)
                        )
                {
                    if (World.GetTile(tilePos) != null)
                    {
                        Program.Campaign.Money += World.GetTile(tilePos).Template.Price;
                    }
                    World.SetTile(_brush, World.LeftTeam, tilePos);
                    Program.Campaign.Money -= _brush.Price;
                }
            }
        }
    }

    private static void SaveToNewFile()
    {
        int number = 1;
        while (true)
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + $"/forts/fort{number}.fort"))
            {
                _saveMessage = $"Saved fort as fort{number}.fort";
                _fort.Name = $"fort{number}";
                _fort.Path = Path.GetDirectoryName(_fort.Path) + "/" + _fort.Name + ".fort";
                _fortAlreadySaved = true;
                Console.WriteLine($"{Directory.GetCurrentDirectory()}/{_fort.Path}");
                Resources.SaveFort($"fort{number}", _fort.Path);
                break;
            }
            if (number >= 999)
            {
                _saveMessage = "Couldn't save!";
                break;
            }
            number++;
        }
    }

    private static void SellAll()
    {
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? t = World.GetTile(x, y);
                if (t == null) continue;
                if (!_sandboxMode) Program.Campaign.Money += t.Template.Price;
                World.SetTile(null, World.LeftTeam, x, y);
            }
        }
        
        _sellAllConfirm = false;
    }
    
    private static string GetFortStats()
    {
        int structureCount = 0;
        int turretCount = 0;
        int utilityCount = 0;
        int beaconCount = 0;
        int nestCount = 0;
        double totalCost = 0;
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? t = World.GetTile(x, y);
                if (t == null) continue;
                structureCount++;
                totalCost += t.Template.Price;
                if (t is ActiveAbilityBeacon) beaconCount++;
                else if (t.Template.Class == StructureTemplate.StructureClass.Utility) utilityCount++;
                else if (t.Template.Class == StructureTemplate.StructureClass.Tower) turretCount++;
                else if (t.Template.Class == StructureTemplate.StructureClass.Nest) nestCount++;
            }
        }
        
        if (!_sandboxMode)
        {
            _nestCapped = nestCount >= Program.Campaign.Level * 2 + 10;
            _beaconCapped = beaconCount >= 4;
        }
        
        return $"{_fort.Name}\n" +
               $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               nestCount + (_sandboxMode ? "" : "/"+(Program.Campaign.Level*2+10)) + " Nests\n" +
               beaconCount + (_sandboxMode ? "" : "/4") + " Stratagems\n" +
               $"{structureCount} Total\n" +
               $"{totalCost} Cost";
    }
}
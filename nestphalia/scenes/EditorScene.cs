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
    private static string _fortStats = "";
    private static bool _newUtil;
    private static bool _newTower;
    private static bool _newNest;
    private static double _price;
    private static int _nestCount;
    private static int _beaconCount;
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
        _sandboxMode = creativeMode;
        _fort = fortToLoad ?? new Fort();
        //Console.WriteLine($"checking if {Directory.GetCurrentDirectory() +  $"/forts/{_fort.Name}.fort"} exists. Answer: {_fortAlreadySaved.ToString()}");
        
        Program.CurrentScene = Scene.Editor;
        World.InitializeEditor();
        _fort.LoadToBoard(false);
        UpdateFortStats();
        
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
                // Path test tool happens during draw.
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

        if (IsKeyPressed(KeyboardKey.Escape)) ExitScene();
        
        // lazy hack so resizing the window doesn't offset the viewport
        World.Camera.Zoom = GetWindowScale().X;
        World.Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter) * GetWindowScale(); 
        
        // ===== DRAW =====
        
        BeginDrawing();
        ClearBackground(Color.Black);
        Screen.DrawBackground(Color.Gray);
        
        World.DrawFloor();
        
        // Draw gui background texture
        DrawTexture(_bg, Screen.HCenter - 604, Screen.VCenter - 304, Color.White);
        
        // Draw brush preview ghost
        if (CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(Screen.HCenter-240, Screen.VCenter-232, 480, 480)))
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
        
        _fort.Name = TextEntry(Screen.HCenter - 592, Screen.VCenter + 172, _fort.Name);
        if (ButtonWide(Screen.HCenter-592, Screen.VCenter+212, "Save Changes")) Resources.SaveFort(_fort.Name, _fort.Path);
        if (ButtonWide(Screen.HCenter - 592, Screen.VCenter + 252, "Exit")) ExitScene();
        
        // Bottom center buttons
        if (ButtonNarrow(Screen.HCenter+150, Screen.VCenter+258, "Sell", _toolActive != EditorTool.Erase))
        {
            _brush = null;
            _toolActive = EditorTool.Erase;
        }
        if (ButtonNarrow(Screen.HCenter+50, Screen.VCenter+258, "Path Preview", _toolActive != EditorTool.PathTester))
        {
            _brush = null;
            _toolActive = EditorTool.PathTester;
        }
        if (_sellAllConfirm && ButtonNarrow(Screen.HCenter-250, Screen.VCenter+258, "Cancel"))   _sellAllConfirm = false;
        else if (!_sellAllConfirm&& ButtonNarrow(Screen.HCenter-250, Screen.VCenter+258, "Sell All")) _sellAllConfirm = true;
        if (_sellAllConfirm && ButtonNarrow(Screen.HCenter-150, Screen.VCenter+258, "Confirm"))  SellAll();
        
        // Structure Category buttons
        if (ButtonNarrow(Screen.HCenter+292, Screen.VCenter-292, (_newUtil ? "NEW! " : "") + "Utility", _structureClass != StructureTemplate.StructureClass.Utility)) _structureClass = StructureTemplate.StructureClass.Utility;
        if (ButtonNarrow(Screen.HCenter+392, Screen.VCenter-292, (_newTower ? "NEW! " : "") + "Tower", _structureClass != StructureTemplate.StructureClass.Tower)) _structureClass = StructureTemplate.StructureClass.Tower;
        if (ButtonNarrow(Screen.HCenter+492, Screen.VCenter-292, (_newNest ? "NEW! " : "") + "Nest", _structureClass != StructureTemplate.StructureClass.Nest)) _structureClass = StructureTemplate.StructureClass.Nest;
        
        StructureList();
        
        DrawInfoPanel();
        
        if (_toolActive == EditorTool.PathTester)
        {
            PathTestTool();
        }
        
        if (!_sandboxMode)
        {
            int nestCap = Program.Campaign.GetNestCap();
            DrawTextLeft(Screen.HCenter - 260, Screen.VCenter - 290, $"Nests: {_nestCount}/{nestCap}", color: _nestCount > nestCap ? Color.Red : Color.White);
            DrawTextLeft(Screen.HCenter - 80, Screen.VCenter - 290, $"Cost: ${_price}/{Program.Campaign.Money} bug dollars", color: _price > Program.Campaign.Money ? Color.Red : Color.White);
            DrawTextLeft(Screen.HCenter + 160, Screen.VCenter - 290, $"Stratagems: {_beaconCount}/{4}", color: _beaconCount > 4 ? Color.Red : Color.White);
        }
        else
        {
            DrawTextLeft(Screen.HCenter - 260, Screen.VCenter - 290, $"Nests: {_nestCount}");
            DrawTextLeft(Screen.HCenter - 80, Screen.VCenter - 290, $"Cost: ${_price} bug dollars");
            DrawTextLeft(Screen.HCenter + 160, Screen.VCenter - 290, $"Stratagems: {_beaconCount}/{4}", color: _beaconCount > 4 ? Color.Red : Color.White);
        }

        EndDrawing();
    }

    private static void StructureList()
    {
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            StructureTemplate s = Assets.Structures[i];
            if ((!_sandboxMode && s.LevelRequirement > Program.Campaign.Level) || s.Class != _structureClass) continue;
            string label = ((!_sandboxMode && s.LevelRequirement == Program.Campaign.Level) ? "NEW! " : "") + s.Name +
                           " - $" + s.Price;
            if (ButtonWide(Screen.HCenter + 292, Screen.VCenter + y * 40 - 250, label, _brush != s))
            {
                _brush = s;
                _toolActive = EditorTool.Brush;
            }

            DrawTexture(s.Texture, Screen.HCenter + 312, Screen.VCenter + y * 40 - 246, Color.White);
            y++;
        }
    }

    private static void DrawInfoPanel()
    {
        string info;
        switch (_toolActive)
        {
            case EditorTool.Brush:
                info = _brush?.GetStats() ?? "";
                break;
            default:
                info = _fortStats;
                break;
        }

        DrawTextLeft(Screen.HCenter - 590, Screen.VCenter - 290, info);
    }

    private static void PathTestTool()
    {
        NavPath navPath = new NavPath("editor", new Int2D(28, 11), World.GetMouseTilePos(), World.RightTeam);
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
                UpdateFortStats();
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
                World.SetTile(_brush, World.LeftTeam, tilePos);
                UpdateFortStats();
            }
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
    
    private static void UpdateFortStats()
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

        _nestCount = nestCount;
        _beaconCount = beaconCount;
        _price = totalCost;
        _fortStats = $"{_fort.Name}\n" +
                     $"{turretCount} Towers\n" +
                     $"{utilityCount} Utility\n" +
                     nestCount + (_sandboxMode ? "" : "/"+(Program.Campaign.Level*2+10)) + " Nests\n" +
                     beaconCount + (_sandboxMode ? "" : "/4") + " Stratagems\n" +
                     $"{structureCount} Total\n" +
                     $"{totalCost} Cost";
    }

    private static void ExitScene()
    {
        if (_sandboxMode)
        {
            CustomBattleMenu.Start();
        }
        else
        {
            Program.Campaign.Start();
        }
    }
}
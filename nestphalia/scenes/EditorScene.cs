using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;

namespace nestphalia;

public class EditorScene : Scene
{
    private Action _startPrevScene;
    private CampaignSaveData? _data;
    private List<StructureTemplate> _buildableStructures = new List<StructureTemplate>();
    private StructureTemplate? _brush;
    private EditorTool _toolActive;
    private Fort _fort;
    private bool _sandboxMode;
    private bool _newUtil;
    private bool _newTower;
    private bool _newNest;
    private bool _sellAllConfirm;
    private StructureTemplate.StructureClass _structureClassSelected;
    private string _fortStats = "";
    private double _price;
    private int _nestCount;
    private int _beaconCount;
    private Texture2D _bg;

    private PathFinder pathFinder = new PathFinder();

    private enum EditorTool
    { 
        Brush,
        Erase,
        PathTester
    }
    
    public void Start(Action exitAction, Fort fortToLoad, CampaignSaveData? data = null)
    {
        _startPrevScene = exitAction; // exitAction is usually the start function of the scene that invoked the editor
        _fort = fortToLoad;
        _brush = null;
        _toolActive = EditorTool.Erase;
        _data = data;
        _sandboxMode = data == null;
        //Console.WriteLine($"checking if {Directory.GetCurrentDirectory() +  $"/forts/{_fort.Name}.fort"} exists. Answer: {_fortAlreadySaved.ToString()}");

        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        _bg = Resources.GetTextureByName("editor_bg");
        World.InitializeEditor(_fort);
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY);
        Resources.PlayMusicByName("so_lets_get_killed");
        
        UpdateFortStats();
        
        // Check if any categories have new unlocks
        _newUtil = false;
        _newTower = false;
        _newNest = false;
        
        if (!_sandboxMode)
        {
            foreach (string s in _data.UnlockedStructures)
            {
                _buildableStructures.Add(Assets.GetStructureByID(s));
            }
            
            _newUtil = _data.NewUtil;
            _newTower = _data.NewTower;
            _newNest = _data.NewNest;
        }
        else
        {
            foreach (StructureTemplate s in Assets.GetAllStructures())
            {
                _buildableStructures.Add(s);
            }
        }
    }
    
    public override void Update()
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
        
        if (Input.Pressed(MouseButton.Right))
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
                    _structureClassSelected = _brush.Class;
                }
            }
        }

        if (Input.Pressed(Input.InputAction.Exit)) _startPrevScene();
        
        // lazy hack so resizing the window doesn't offset the viewport
        World.Camera.Zoom = GetWindowScale().X;
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY) * GetWindowScale(); 
        
        // ===== DRAW =====
        
        BeginDrawing();
        ClearBackground(Color.Black);
        Screen.DrawBackground(Color.Gray);
        
        World.DrawFloor();
        
        // Draw gui background texture
        DrawTexture(_bg, Screen.CenterX - 604, Screen.CenterY - 304, Color.White);
        
        // Draw brush preview ghost
        if (CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(Screen.CenterX-240, Screen.CenterY-232, 480, 480)))
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
        
        _fort.Name = TextEntry(-592, 172, _fort.Name);
        if (Button300(-592, 212, "Save Changes")) Resources.SaveFort(_fort);
        if (Button300(-592, 252, "Exit")) _startPrevScene();
        
        // Bottom center buttons
        if (Button100(150, 258, "Sell", _toolActive != EditorTool.Erase))
        {
            _brush = null;
            _toolActive = EditorTool.Erase;
        }
        if (Button100(50, 258, "Path Preview", _toolActive != EditorTool.PathTester))
        {
            _brush = null;
            _toolActive = EditorTool.PathTester;
        }
        if (_sellAllConfirm && Button100(-250, 258, "Cancel"))   _sellAllConfirm = false;
        else if (!_sellAllConfirm&& Button100(-250, 258, "Sell All")) _sellAllConfirm = true;
        if (_sellAllConfirm && Button100(-150, 258, "Confirm"))  SellAll();
        
        // Structure Category buttons
        if (Button100(292, -292, (_newUtil ? "NEW! " : "") + "Utility", _structureClassSelected != StructureTemplate.StructureClass.Utility)) _structureClassSelected = StructureTemplate.StructureClass.Utility;
        if (Button100(392, -292, (_newTower ? "NEW! " : "") + "Tower", _structureClassSelected != StructureTemplate.StructureClass.Tower)) _structureClassSelected = StructureTemplate.StructureClass.Tower;
        if (Button100(492, -292, (_newNest ? "NEW! " : "") + "Nest", _structureClassSelected != StructureTemplate.StructureClass.Nest)) _structureClassSelected = StructureTemplate.StructureClass.Nest;
        
        StructureList();
        
        DrawInfoPanel();
        
        if (_toolActive == EditorTool.PathTester)
        {
            PathTestTool();
        }
        
        if (!_sandboxMode)
        {
            int nestCap = _data.GetNestCap();
            DrawTextLeft(-260, -290, $"Nests: {_nestCount}/{nestCap}", color: _nestCount > nestCap ? Color.Red : Color.White);
            DrawTextLeft(-80,  -290, $"Cost: ${_price}/{_data.Money} bug dollars", color: _price > _data.Money ? Color.Red : Color.White);
            DrawTextLeft( 160, -290, $"Stratagems: {_beaconCount}/{4}", color: _beaconCount > 4 ? Color.Red : Color.White);
        }
        else
        {
            DrawTextLeft(-260, -290, $"Nests: {_nestCount}");
            DrawTextLeft(-80,  -290, $"Cost: ${_price} bug dollars");
            DrawTextLeft( 160, -290, $"Stratagems: {_beaconCount}/{4}", color: _beaconCount > 4 ? Color.Red : Color.White);
        }
    }

    private void StructureList()
    {
        int y = 0;
        for (int i = 0; i < _buildableStructures.Count; i++)
        {
            StructureTemplate s = _buildableStructures[i];
            if ((!_sandboxMode && s.LevelRequirement > _data.Level) || s.Class != _structureClassSelected) continue;
            string label = ((!_sandboxMode && s.LevelRequirement == _data.Level) ? "NEW! " : "") + s.Name +
                           " - $" + s.Price;
            if (Button300(292, y * 40 - 250, label, _brush != s))
            {
                _brush = s;
                _toolActive = EditorTool.Brush;
            }

            DrawTexture(s.Texture, Screen.CenterX + 312, Screen.CenterY + y * 40 - 246, Color.White);
            y++;
        }
    }

    private void DrawInfoPanel()
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

        DrawTextLeft(-590, -290, info);
    }

    private void PathTestTool()
    {
        NavPath navPath = new NavPath("editor", new Int2D(28, 11), World.GetMouseTilePos(), World.RightTeam);
        pathFinder.FindPath(navPath);
        
        BeginMode2D(World.Camera);
        
        Vector2 path = World.GetTileCenter(navPath.Start);
        foreach (Int2D i in navPath.Points)
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
    
    private void EraseTool()
    {
        if (Input.Held(MouseButton.Left))
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

    private void BrushTool()
    {
        if (_brush == null) 
        {
            GameConsole.WriteLine("Tried to use null brush!");
            _toolActive = EditorTool.Erase;
            return;
        }
        if (Input.Held(MouseButton.Left))
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

    private void SellAll()
    {
        for (int x = 0; x < World.BoardWidth; ++x)
        for (int y = 0; y < World.BoardHeight; ++y)
        {
            Structure? t = World.GetTile(x, y);
            if (t == null) continue;
            if (!_sandboxMode) _data.Money += t.Template.Price;
            World.SetTile(null, World.LeftTeam, x, y);
        }
        
        _sellAllConfirm = false;
    }
    
    private void UpdateFortStats()
    {
        int structureCount = 0;
        int turretCount = 0;
        int utilityCount = 0;
        int beaconCount = 0;
        int nestCount = 0;
        double totalCost = 0;
        
        for (int x = 0; x < World.BoardWidth; ++x)
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

        _nestCount = nestCount;
        _beaconCount = beaconCount;
        _price = totalCost;
        _fortStats = $"{_fort.Name}\n" +
                     $"{turretCount} Towers\n" +
                     $"{utilityCount} Utility\n" +
                     nestCount + (_sandboxMode ? "" : "/"+(_data.Level*2+10)) + " Nests\n" +
                     beaconCount + (_sandboxMode ? "" : "/4") + " Stratagems\n" +
                     $"{structureCount} Total\n" +
                     $"{totalCost} Cost";
    }
}
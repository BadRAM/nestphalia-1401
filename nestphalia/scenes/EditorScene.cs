using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;

namespace nestphalia;

public class EditorScene : Scene
{
    private Action<Fort> _exitAction;
    private CampaignSaveData? _data;
    private List<StructureTemplate> _buildableStructures;
    private StructureTemplate? _brush;
    private EditorTool _toolActive;
    private Fort _fort;
    private bool _sandboxMode;
    private bool _newBasic;
    private bool _newUtil;
    private bool _newTower;
    private bool _newNest;
    private bool _sellAllConfirm;
    private StructureTemplate.StructureClass _structureClassSelected;
    private string _fortStats = "";
    private double _price;
    private int _nestCount;
    private int _beaconCount;
    private StretchyTexture _panelTex;
    private Texture2D _bg;
    private StructureTemplate? _toolTipStructure;
    private string _name = "";
    private bool _unsaved;
    private Int2D _mouseTilePos = Int2D.Zero;
    
    private PathFinder pathFinder = new PathFinder();

    private enum EditorTool
    { 
        Brush,
        Erase,
        PathTester
    }
    
    public void Start(Action<Fort> exitAction, Fort fortToLoad, CampaignSaveData? data = null)
    {
        _exitAction = exitAction; // exitAction is usually the start function of the scene that invoked the editor
        _fort = fortToLoad;
        _brush = null;
        _toolActive = EditorTool.Erase;
        _data = data;
        _sandboxMode = data == null;
        _unsaved = false;
        _name = _fort.Name;
        
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        _panelTex = Assets.Get<StretchyTexture>("stretch_default");
        _bg = Resources.GetTextureByName("editor_bg");
        World.InitializeEditor(Assets.Get<Level>("level_fortedit"), _fort);
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY);
        // Resources.PlayMusicByName("so_lets_get_killed");
        Resources.PlayMusicByName("nd_editor_live");
        
        UpdateFortStats();
        
        // Check if any categories have new unlocks
        _newUtil = false;
        _newTower = false;
        _newNest = false;
        
        _buildableStructures = new List<StructureTemplate>();
        if (!_sandboxMode)
        {
            foreach (string structure in _data.Unlocks)
            {
                if (!Assets.Exists<StructureTemplate>(structure))
                {
                    GameConsole.WriteLine("Editor couldn't find structure: " + structure);
                    continue;
                }
                StructureTemplate s = Assets.Get<StructureTemplate>(structure);
                _buildableStructures.Add(s);
            }
            foreach (string unlock in _data.NewUnlocks)
            {
                if (!Assets.Exists<StructureTemplate>(unlock))
                {
                    GameConsole.WriteLine("Editor couldn't find structure: " + unlock);
                    continue;
                }
                switch (Assets.Get<StructureTemplate>(unlock).Class)
                {
                    case StructureTemplate.StructureClass.Basic:
                        _newBasic = true;
                        break;
                    case StructureTemplate.StructureClass.Utility:
                        _newUtil = true;
                        break;
                    case StructureTemplate.StructureClass.Defense:
                        _newTower = true;
                        break;
                    case StructureTemplate.StructureClass.Nest:
                        _newNest = true;
                        break;
                }
            }
        }
        else
        {
            foreach (StructureTemplate s in Assets.GetAll<StructureTemplate>())
            {
                if (s.Class != StructureTemplate.StructureClass.Secret)
                {
                    _buildableStructures.Add(s);
                }
            }
        }
        _buildableStructures = _buildableStructures.OrderBy(o => o.Price).ToList();
    }
    
    public override void Update()
    {
        // ===== INPUT + UPDATE =====
        _mouseTilePos = World.GetMouseTilePos();
        
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
            if (MouseIsInBounds())
            {
                _brush = World.GetTile(_mouseTilePos)?.Template;
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
        
        // lazy hack so resizing the window doesn't offset the viewport
        World.Camera.Zoom = GetWindowScale().X;
        World.Camera.Offset = new Vector2(Screen.CenterX + 192, Screen.CenterY - 64) * GetWindowScale(); 
        
        // ===== DRAW =====
        
        Screen.BeginDrawing();
        ClearBackground(Color.Black);
        _toolTipStructure = null;
        Screen.DrawBackground(Color.Gray);
        
        World.DrawFloor();
        
        // Draw gui background texture
        DrawTexture(_bg, Screen.CenterX - 484, Screen.CenterY - 364, Color.White);
        
        // Draw brush preview ghost
        if (MouseIsInBounds())
        {
            Screen.SetCamera(World.Camera);
            Vector2 mousePos = World.GetTileCenter(World.GetMouseTilePos());
            if (_brush is TowerTemplate t)
            {
                DrawCircleLines((int)mousePos.X, (int)mousePos.Y, (int)t.Range, new Color(200, 50, 50, 255));
            }
            if (_brush != null)
            {
                _brush.Draw(World.GetTileCenter(World.GetMouseTilePos()), new Color(128, 128, 255, 128));
            }
            Screen.SetCamera();
        }
        
        World.Draw();

        _name = TextEntry(290, 194, _name);
        if (_fort.Name != _name)
        {
            _fort.Name = _name;
            _unsaved = true;
        }
        if (Button90(290, 238, "Save", enabled:_unsaved)) Save();
        if (Button90(380, 238, "Save As")) SaveAs();
        if (Button180(290, 276, "Load")) Load();
        if (Button180(290, 314, "Exit") || Input.Pressed(Input.InputAction.Exit)) Exit();
        
        // Bottom center buttons
        if (Button90(-64, -358, "Sell", _toolActive != EditorTool.Erase))
        {
            _brush = null;
            _toolActive = EditorTool.Erase;
        }
        if (Button90(28, -358, "Path Test", _toolActive != EditorTool.PathTester))
        {
            _brush = null;
            _toolActive = EditorTool.PathTester;
        }
        if (_sellAllConfirm && Button90(120, -358, "Cancel"))   _sellAllConfirm = false;
        else if (!_sellAllConfirm&& Button90(120, -358, "Sell All")) _sellAllConfirm = true;
        if (_sellAllConfirm && Button90(212, -358, "Confirm"))  SellAll();
        
        // Structure Category buttons
        if (Button90(-470, -350, _newBasic  ? "! Basic !" : "Basic",     _structureClassSelected != StructureTemplate.StructureClass.Basic)) _structureClassSelected = StructureTemplate.StructureClass.Basic;
        if (Button90(-380, -350, _newUtil  ? "! Utility !" : "Utility", _structureClassSelected != StructureTemplate.StructureClass.Utility)) _structureClassSelected = StructureTemplate.StructureClass.Utility;
        if (Button90(-290, -350, _newTower ? "! Defense !" : "Defense", _structureClassSelected != StructureTemplate.StructureClass.Defense))   _structureClassSelected = StructureTemplate.StructureClass.Defense;
        if (Button90(-200, -350, _newNest  ? "! Nest !" : "Nest",       _structureClassSelected != StructureTemplate.StructureClass.Nest))    _structureClassSelected = StructureTemplate.StructureClass.Nest;
        
        StructureList();
        
        DrawInfoPanel();
        
        if (_toolActive == EditorTool.PathTester)
        {
            PathTestTool();
        }
        
        DrawToolTip();
    }

    private void Save()
    {
        if (_fort.Path == "")
        {
            SaveAs();
        }
        _fort.SaveBoard();
        _fort.SaveToDisc();
        _unsaved = false;
    }
    
    private void SaveAs()
    {
        PopupManager.Start(new AlertPopup("Error!", "\"Save As\" has not yet been implemented.\nFort folder will be opened.", "OK",
        () =>
        {
            Utils.OpenFolder(Path.GetDirectoryName(_fort.Path));
        }));
        GameConsole.WriteLine("Todo: Implement \"save as\" function");
    }
    
    private void Load()
    {
        PopupManager.Start(new FortPickerPopup(Path.GetDirectoryName(_fort.Path), 
        fort => 
        {
            Start(_exitAction, fort, _data);
        },
        path =>
        {
            Fort f = new Fort(path);
            Start(_exitAction, f, _data);
        }));
    }
    
    private void Exit()
    {
        if (_unsaved)
        {
            PopupManager.Start(new YesNoPopup("Alert", "You have unsaved changes!", "Save", "Discard", yes =>
            {
                if (yes)
                {
                    Save();
                }
                _exitAction.Invoke(_fort);
            }));
        }
        else
        {
            _exitAction.Invoke(_fort);
        }
    }

    private void StructureList()
    {
        int y = 0;
        for (int i = 0; i < _buildableStructures.Count; i++)
        {
            StructureTemplate s = _buildableStructures[i];
            if (s.Class != _structureClassSelected) continue;
            string label = s.Name + " - $" + s.Price;
            if (Button360(-470, y * 38 - 310, label, _brush != s))
            {
                _brush = s;
                _toolActive = EditorTool.Brush;
            }

            if (CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(Screen.CenterX - 470, Screen.CenterY + y * 38 - 310, 360, 36)))
            {
                _toolTipStructure = s;
            }

            s.Draw(new Vector2(Screen.CenterX - 450, Screen.CenterY + y * 38 - 290), Color.White);
            if (s is SpawnerTemplate spawner)
            {
                spawner.Minion.Asset.DrawPreview(Screen.CenterX - 410, Screen.CenterY + y * 38 - 292, Color.Red);
            }
            y++;
        }
    }

    private void DrawInfoPanel()
    {
        DrawTextLeft(-94, 204, "Name:\nTowers:\nUtility:\nNests:\nActive:\nTotal:\nCost:");
        DrawTextLeft(-30, 204, _fortStats);
    }

    private void DrawToolTip()
    {
        if (_toolTipStructure == null) return;

        string tip = WrapText(_toolTipStructure.GetStats(), 270);

        Rectangle rect = new Rectangle(GetScaledMousePosition() + Vector2.One * 12, MeasureText(tip) + Vector2.One * 4);

        rect.Position = Vector2.Clamp(rect.Position, Vector2.Zero, Screen.BottomRight - rect.Size);
        
        DrawStretchyTexture(_panelTex, rect, anchor:Screen.TopLeft);
        DrawTextLeft(rect.Position + Vector2.One * 2, tip, anchor:Screen.TopLeft);
    }

    private void PathTestTool()
    {
        NavPath navPath = new NavPath("editor", new Int2D(21, 11), World.GetMouseTilePos(), World.RightTeam);
        pathFinder.FindPath(navPath);
        
        Screen.SetCamera(World.Camera);
        
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
        
        Screen.SetCamera();
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
                _unsaved = true;
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
                _unsaved = true;
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
        _unsaved = true;
        UpdateFortStats();
    }
    
    private void UpdateFortStats()
    {
        int structureCount = 0;
        int turretCount = 0;
        int basicCount = 0;
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
            else if (t.Template.Class == StructureTemplate.StructureClass.Basic) basicCount++;
            else if (t.Template.Class == StructureTemplate.StructureClass.Utility) utilityCount++;
            else if (t.Template.Class == StructureTemplate.StructureClass.Defense) turretCount++;
            else if (t.Template.Class == StructureTemplate.StructureClass.Nest) nestCount++;
        }

        _nestCount = nestCount;
        _beaconCount = beaconCount;
        _price = totalCost;
        _fortStats = $"{_fort.Name}\n" +
                     $"{turretCount}\n" +
                     $"{utilityCount}\n" +
                     nestCount + (_sandboxMode ? "" : " / "+(_data.GetNestCap())) + "\n" +
                     beaconCount + " / 4\n" +
                     $"{structureCount}\n" +
                     $"{totalCost}" + (_sandboxMode ? "" : " / "+_data.Money);
    }

    private bool MouseIsInBounds()
    {
        return _mouseTilePos.X >= 1 && _mouseTilePos.X < 21 && _mouseTilePos.Y >= 1 && _mouseTilePos.Y < 21;
    }
}
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using static nestphalia.GUI;
using Formatting = Newtonsoft.Json.Formatting;

namespace nestphalia;

public class LevelEditorScene : Scene
{
    private Level _level;
    private string _idBuffer = "level_id";
    private string _fullLevelData = "";
    private string _levelBuffer = "";
    private string _scriptBuffer = "";
    private string _levelInfo = "";

    private List<FloorTileTemplate> _floors = new List<FloorTileTemplate>();
    private List<StructureTemplate> _structures = new List<StructureTemplate>();
    private List<string> _floorScatters = new List<string>();
    private List<Texture2D> _floorScatterTex = new List<Texture2D>();

    private LevelEditorTool _toolActive;
    private List<FloorTileTemplate> _selectedFloors = new List<FloorTileTemplate>();
    private StructureTemplate? _selectedStructure = null;
    private string _tooltip = "";
    private string _selectedScatter = "";
    private FloorScatter? _grabScatter = null;
    private List<string> _loadableLevels = new List<string>();
    private StretchyTexture _panelBg;
    // private int _brushRadius = 2;

    private enum LevelEditorTool
    {
        FloorBrush,
        StructureBrush,
        ScatterBrush
    }
    
    public void Start(Level? loadLevel = null)
    {
        if (loadLevel != null)
        {
            _level = loadLevel;
        }
        else
        {
            _level = new Level(JObject.Parse($@"{{""ID"": ""level""}}"));
        }
        World.InitializeLevelEditor(_level);
        Screen.DebugMode = false;
        
        _floors.AddRange(Assets.GetAll<FloorTileTemplate>());
        _floors = _floors.OrderBy(o => o.ID).ToList();
        
        _structures.AddRange(Assets.GetAll<StructureTemplate>());
        _structures = _structures.OrderBy(o => o.ID).ToList();

        foreach (FloorScatterTexList list in Assets.GetAll<FloorScatterTexList>())
        {
            foreach (string texture in list.Textures)
            {
                if(!_floorScatters.Contains(texture)) _floorScatters.Add(texture);
            }
        }

        foreach (string floorScatter in _floorScatters)
        {
            _floorScatterTex.Add(Resources.GetTextureByName(floorScatter));
        }
        
        _panelBg = Assets.Get<StretchyTexture>("stretch_default");
        
        Program.CurrentScene = this;
        Load();
    }
    
    public override void Update()
    {
        if (Input.Pressed(Input.InputAction.Exit))
        {
            Screen.DebugMode = true;
            new MenuScene().Start();
        }

        if (Input.Pressed(KeyboardKey.Q))
        {
            if (_toolActive == LevelEditorTool.StructureBrush)
            {
                _selectedStructure = World.GetTile(World.GetMouseTilePos())?.Template;
            }

            if (_toolActive == LevelEditorTool.FloorBrush)
            {
                Int2D m = World.GetMouseTilePos();
                FloorTileTemplate picked = World.GetFloorTile(m.X, m.Y).Template;
                if (!_selectedFloors.Remove(picked))
                {
                    _selectedFloors.Add(picked);
                }
            }
        }
        
        if (Input.Held(MouseButton.Right))
        {
            World.Camera.Offset += Raylib.GetMouseDelta();
        }
        
        switch (_toolActive)
        {
            case LevelEditorTool.FloorBrush:
                FloorBrush();
                break;
            case LevelEditorTool.ScatterBrush:
                ScatterBrush();
                break;
            case LevelEditorTool.StructureBrush:
                StructureBrush();
                break; 
        }
        
        // ===== DRAW =====
        
        Screen.BeginDrawing();
        Raylib.ClearBackground(Color.DarkGray);
        
        World.DrawFloor();
        World.Draw();
        
        // Draw outline of player's fort position
        Screen.SetCamera(World.Camera);
        foreach (Level.FortSpawnZone spawnZone in _level.FortSpawnZones)
        {
            Rectangle playerRect = new Rectangle(World.GetTileCenter(spawnZone.Offset()) + Vector2.One * -12, Vector2.One * 24 * 20);
            Raylib.DrawRectangleLinesEx(playerRect, 2, Raylib.ColorAlpha(Color.Blue, 0.8f));
        }
        Screen.SetCamera();
        
        Raylib.DrawRectangle(0, Screen.TopY, 300, Screen.BottomY, Raylib.ColorAlpha(Color.Black, 0.5f));
        
        if (Button90(5, 2, "Floor", anchor: Screen.TopLeft))
        {
            _toolActive = LevelEditorTool.FloorBrush;
            _selectedFloors.Clear();
        }
        if (Button90(105, 2, "Scatter", anchor: Screen.TopLeft))
        {
            _toolActive = LevelEditorTool.ScatterBrush;
            _selectedScatter = "";
        }
        if (Button90(205, 0, "Structure", anchor: Screen.TopLeft))
        {
            _toolActive = LevelEditorTool.StructureBrush;
            _selectedStructure = null;
        }

        _idBuffer = TextEntry(0, 300, _idBuffer, anchor: Screen.TopLeft);
        if (Button100(0, 340, "Save", anchor: Screen.TopLeft)) Save();
        if (Button100(100, 340, "Folder", anchor: Screen.TopLeft)) Utils.OpenFolder(Resources.Dir + @"\resources\levels");
        if (Button100(200, 340, "Load", anchor: Screen.TopLeft)) ShowLoadPopup();
        BigTextCopyPad(0, 380, "Full Level Data", _fullLevelData, anchor: Screen.TopLeft, allowPaste:false);
        
        _levelBuffer = BigTextCopyPad(15, 460, "Level Metadata", _levelBuffer, anchor: Screen.TopLeft);
        _scriptBuffer = BigTextCopyPad(15, 500, "Script", _scriptBuffer, anchor: Screen.TopLeft);
        _tooltip = "";
        
        if (_toolActive == LevelEditorTool.FloorBrush)
        {
            for (int i = 0; i < _floors.Count; i++)
            {
                bool selected = _selectedFloors.Contains(_floors[i]);
                ButtonState butt = TileButtonPro((2) + (i % 10) * 28, 40 + (i / 10) * 28, _floors[i].Texture, selected, anchor: Screen.TopLeft);
                if (butt == ButtonState.Pressed)
                {
                    if (selected)
                    {
                        _selectedFloors.Remove(_floors[i]);
                    }
                    else
                    {
                        _selectedFloors.Add(_floors[i]);
                    }
                }
                if (butt == ButtonState.Hovered)
                {
                    _tooltip = WrapText(_floors[i].ID, 270);
                }
            }
            
            if (Button100(0, 260, "Fill", enabled:_selectedFloors.Count != 0, anchor: Screen.TopLeft))
            {
                for (int x = 0; x < _level.WorldSize.X; x++)
                for (int y = 0; y < _level.WorldSize.Y; y++)
                {
                    World.SetFloorTile(_selectedFloors[Random.Shared.Next(_selectedFloors.Count)], x, y);
                }
            }
        }
        if (_toolActive == LevelEditorTool.ScatterBrush)
        {
            for (int i = 0; i < _floorScatters.Count; i++)
            {
                bool selected = _selectedScatter == _floorScatters[i];
                ButtonState butt = ImageButtonPro((2) + (i % 5) * 56, 40 + (i / 5) * 56, Resources.GetTextureByName(_floorScatters[i]), new Vector2(48, 48), selected, anchor: Screen.TopLeft);
                if (butt == ButtonState.Pressed)
                {
                    _selectedScatter = _floorScatters[i];
                }
                if (butt == ButtonState.Hovered)
                {
                    _tooltip = WrapText(_floorScatters[i], 270);
                }
            }
        }
        if (_toolActive == LevelEditorTool.StructureBrush)
        {
            for (int i = 0; i < _structures.Count; i++)
            {
                bool selected = _selectedStructure == _structures[i];
                ButtonState butt = StructureButtonPro((2) + (i % 10) * 28, 40 + (i / 10) * 28, _structures[i], selected, anchor: Screen.TopLeft);
                if (butt == ButtonState.Pressed)
                {
                    _selectedStructure = _structures[i];
                }
                if (butt == ButtonState.Hovered)
                {
                    _tooltip = WrapText(_structures[i].GetStats(), 270);
                }
            }
            if (Button100(0, 260, "Erase", anchor: Screen.TopLeft)) _selectedStructure = null;
        }
        DrawToolTip();
        DrawTextLeft(0, 550, _levelInfo, anchor:Screen.TopLeft);
    }

    private void ScatterBrush()
    {
        if (!Input.Held(MouseButton.Left)) _grabScatter = null;
        if (_grabScatter == null)
        {
            foreach (FloorScatter scatter in World.FloorScatters)
            {
                if (Raylib.CheckCollisionPointRec(World.GetMousePos() - GetScaledMouseDelta(), scatter.Rect()))
                {
                    _grabScatter = scatter;
                    break;
                }
            }
        }
        if (_grabScatter != null)
        {
            SetCursorLook(MouseCursor.ResizeAll);
            if (Input.Held(MouseButton.Left))
            {
                if (Input.Held(KeyboardKey.F))
                {
                    _grabScatter.Rotation += GetScaledMouseDelta().X;
                }
                else if (Input.Held(KeyboardKey.LeftShift))
                {
                    _grabScatter.Position = World.GetTileCenter(World.GetMouseTilePos()) - Vector2.One * 12;
                }
                else
                {
                    _grabScatter.Position += GetScaledMouseDelta();
                }

                if (Input.Pressed(KeyboardKey.R))
                {
                    if (_grabScatter.Rotation % 90 == 0)
                    {
                        _grabScatter.Rotation += 90;
                    }
                    else
                    {
                        _grabScatter.Rotation = (int)Math.Round((_grabScatter.Rotation / (double)90), MidpointRounding.AwayFromZero) * 90;
                    }
                }

                if (Input.Pressed(KeyboardKey.E))
                {
                    World.FloorScatters.Remove(_grabScatter);
                    _grabScatter = null;
                }
            }
        }
        else
        {
            if (_selectedScatter != "" && Input.Pressed(MouseButton.Left))
            {
                World.FloorScatters.Add(new FloorScatter(_selectedScatter, World.GetMousePos(), 0));
            }
        }
    }

    private void FloorBrush()
    {
        if (Input.Held(MouseButton.Left) && Raylib.GetMousePosition().X > 300)
        {
            Int2D tilePos = World.GetMouseTilePos();
            if (tilePos.X >= 0 && tilePos.X < _level.WorldSize.X && tilePos.Y >= 0 && tilePos.Y < _level.WorldSize.Y) 
            {
                if (_selectedFloors.Count > 0)
                {
                    World.SetFloorTile(_selectedFloors[Random.Shared.Next(_selectedFloors.Count)], tilePos.X, tilePos.Y);
                }
                else
                {
                    World.SetFloorTile(Assets.Get<FloorTileTemplate>("floor_empty"), tilePos.X, tilePos.Y);
                }
            }
        }
    }
    
    private void StructureBrush()
    {
        if (Input.Held(MouseButton.Left) && Raylib.GetMousePosition().X > 300)
        {
            Int2D tilePos = World.GetMouseTilePos();
            if (tilePos.X >= 0 && tilePos.X < _level.WorldSize.X && tilePos.Y >= 1 && tilePos.Y < _level.WorldSize.Y) 
            {
                World.SetTile(_selectedStructure, World.RightTeam, tilePos);
            }
        }
    }
    
    private void DrawToolTip()
    {
        if (_tooltip == "") return;
        Rectangle rect = new Rectangle(GetScaledMousePosition() + Vector2.One * 12, MeasureText(_tooltip) + Vector2.One * 4);
        rect.Position = Vector2.Clamp(rect.Position, Vector2.Zero, Screen.BottomRight - rect.Size);
        DrawStretchyTexture(_panelBg, rect, anchor:Screen.TopLeft);
        DrawTextLeft(rect.Position + Vector2.One * 2, _tooltip, anchor:Screen.TopLeft);

    }
    
    private void Save()
    {
        JObject j = JObject.Parse(_levelBuffer);
        j.Add("ID", _idBuffer);
        _level = new Level(j);
        _level.LoadFromBoard();
        _level.Script = _scriptBuffer;

        for (int x = 0; x < _level.WorldSize.X; x++)
        for (int y = 0; y < _level.WorldSize.Y; y++)
        {
            _level.FloorTiles[x, y] = World.GetFloorTile(x, y).Template.ID;
            _level.Structures[x, y] = World.GetTile(x, y)?.Template.ID ?? "";
        }
        
        File.WriteAllText(SelectedSavePath(), JsonConvert.SerializeObject(_level, Formatting.Indented));

        if (!Assets.Exists(_level.ID))
        {
            Assets.IndexAsset(_level);
        }
        Assets.UpdateAsset(_level);
        
        Load();
    }
    
    private void Load()
    {
        if (File.Exists(SelectedSavePath()))
        {
            _level = new Level(JObject.Parse(File.ReadAllText(SelectedSavePath())));
            GameConsole.WriteLine($"loaded {SelectedSavePath()}");
        }
        
        World.InitializeLevelEditor(_level);
        
        JObject j = JObject.FromObject(_level);
        
        _fullLevelData = j.ToString();
        _fullLevelData = _fullLevelData.Replace("\r\n", "\n");

        _scriptBuffer = j.Value<string>("Script") ?? "";
        _scriptBuffer = _scriptBuffer.Replace("\r\n", "\n");

        _levelInfo = _level.GetLevelStats();

        j.Remove("ID");
        j.Remove("Script");
        j.Remove("FloorTiles");
        j.Remove("Structures");

        _levelBuffer = j.ToString();
        _levelBuffer = _levelBuffer.Replace("\r\n", "\n");

    }

    private void ShowLoadPopup()
    {
        _loadableLevels = Directory.GetFiles(Resources.Dir + "/resources/levels/").ToList();
        for (int i = 0; i < _loadableLevels.Count; i++)
        {
            _loadableLevels[i] = Path.GetFileNameWithoutExtension(_loadableLevels[i]);
        }
        _loadableLevels.Sort();
        
        PopupManager.Start(new PickerPopup("Select level to load", _loadableLevels.ToArray(), LoadFromList));
    }

    private void LoadFromList(int index)
    {
        _idBuffer = _loadableLevels[index];
        Load();
    }

    private string SelectedSavePath()
    {
        return Resources.Dir + "/resources/levels/" + _idBuffer + ".json";
    }
}
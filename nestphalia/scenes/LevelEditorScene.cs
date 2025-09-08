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
    private string _levelBuffer = "";
    private string _scriptBuffer = "";

    private List<FloorTileTemplate> _floors = new List<FloorTileTemplate>();
    private List<StructureTemplate> _structures = new List<StructureTemplate>();

    private LevelEditorTool _toolActive;
    private List<FloorTileTemplate> _selectedFloors = new List<FloorTileTemplate>();
    private StructureTemplate? _selectedStructure = null;
    private List<string> _loadableLevels = new List<string>();
    // private int _brushRadius = 2;

    private enum LevelEditorTool
    {
        FloorBrush,
        StructureBrush
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
        
        _floors.AddRange(Assets.GetAll<FloorTileTemplate>());
        _floors = _floors.OrderBy(o => o.ID).ToList();
        
        _structures.AddRange(Assets.GetAll<StructureTemplate>());
        _structures = _structures.OrderBy(o => o.ID).ToList();
        
        Program.CurrentScene = this;
        Load();
    }
    
    public override void Update()
    {
        if (Input.Pressed(Input.InputAction.Exit))
        {
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
        
        if (Button100(0, 0, "Floor", anchor: Screen.TopLeft))
        {
            _toolActive = LevelEditorTool.FloorBrush;
            _selectedFloors.Clear();
        }
        if (Button100(200, 0, "Structure", anchor: Screen.TopLeft))
        {
            _toolActive = LevelEditorTool.StructureBrush;
            _selectedStructure = null;
        }

        _idBuffer = TextEntry(0, 300, _idBuffer, anchor: Screen.TopLeft);
        if (Button100(0, 340, "Save", anchor: Screen.TopLeft)) Save();
        if (Button100(100, 340, "Folder", anchor: Screen.TopLeft)) Utils.OpenFolder(@"\resources\levels");
        if (Button100(200, 340, "Load", anchor: Screen.TopLeft)) ShowLoadPopup();
        BigTextCopyPad(0, 380, "Full Level Data", JObject.FromObject(_level).ToString(), anchor: Screen.TopLeft);
        
        _levelBuffer = BigTextCopyPad(0, 460, "Level Metadata", _levelBuffer, anchor: Screen.TopLeft);
        _scriptBuffer = BigTextCopyPad(0, 500, "Script", _scriptBuffer, anchor: Screen.TopLeft);

        if (_toolActive == LevelEditorTool.FloorBrush)
        {
            for (int i = 0; i < _floors.Count; i++)
            {
                bool selected = _selectedFloors.Contains(_floors[i]);
                if (TileButton((2) + (i%10)*28, 40 + (i/10)*28, _floors[i].Texture, selected, anchor: Screen.TopLeft))
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
            }

            if (Button100(0, 260, "Fill", anchor: Screen.TopLeft))
            {
                for (int x = 0; x < _level.WorldSize.X; x++)
                for (int y = 0; y < _level.WorldSize.Y; y++)
                {
                    World.SetFloorTile(_selectedFloors[Random.Shared.Next(_selectedFloors.Count)], x, y);
                }
            }
        }
        if (_toolActive == LevelEditorTool.StructureBrush)
        {
            for (int i = 0; i < _structures.Count; i++)
            {
                bool selected = _selectedStructure == _structures[i];
                if (TileButton((2) + (i%10)*28, 40 + (i/10)*28, _structures[i].Texture, selected, anchor: Screen.TopLeft))
                {
                    _selectedStructure = _structures[i];
                }
            }
            if (Button100(0, 260, "Erase", anchor: Screen.TopLeft)) _selectedStructure = null;
            if (_selectedStructure != null)
            {
                DrawTextLeft(4, 240, _selectedStructure.ID);
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

        _scriptBuffer = j.Value<string>("Script") ?? "";

        j.Remove("ID");
        j.Remove("Script");
        j.Remove("FloorTiles");
        j.Remove("Structures");

        _levelBuffer = j.ToString();
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
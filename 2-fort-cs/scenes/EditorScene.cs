using System.Numerics;
using ZeroElectric.Vinculum;
using static _2_fort_cs.GUI;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class EditorScene
{
    private static bool _sandboxMode = false;
    private static StructureTemplate? _brush;
    private static Fort _fort;
    private static string _saveMessage;
    private static string _fortStats;
    private static bool _newUtil;
    private static bool _newTower;
    private static bool _newNest;
    private static bool _nestCapped;
    private static StructureTemplate.StructureClass _structureClass;
    
    public static void Start(Fort? fortToLoad = null, bool creativeMode = false)
    {
        _saveMessage = "";
        _sandboxMode = creativeMode;
        _fort = fortToLoad ?? new Fort();
        
        Program.CurrentScene = Scene.Editor;
        World.InitializeEditor();
        World.Camera.offset = new Vector2(Screen.HCenter, Screen.VCenter);
        _fort.LoadToBoard(false);
        
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
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----
        
        World.Camera.offset = new Vector2(Screen.HCenter-264, Screen.VCenter-264);

        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            Int2D tilePos = World.GetMouseTilePos();
            
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21 
                && _brush != World.GetTile(tilePos)?.Template)
            {
                if (_sandboxMode)
                {
                    World.SetTile(_brush, World.LeftTeam, tilePos);
                }
                else
                {
                    if (_brush == null || (_brush.Price < Program.Campaign.Money && (_brush.Class != StructureTemplate.StructureClass.Nest || !_nestCapped)))
                    {
                        if (World.GetTile(tilePos) != null)
                        {
                            Program.Campaign.Money += World.GetTile(tilePos).Template.Price;
                        }
                        World.SetTile(_brush, World.LeftTeam, tilePos);
                        if (_brush != null)
                        {
                            Program.Campaign.Money -= _brush.Price;
                        }
                    }
                }
            }
        }
        
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Int2D tilePos = World.GetMouseTilePos();
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21)
            {
                _brush = World.GetTile(tilePos)?.Template;
            }
        }
        
        BeginDrawing();
        ClearBackground(BLACK);
        World.Draw();

        if (_sandboxMode)
        {
            if (ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Exit")) MenuScene.Start();
        }
        else
        {
            if (ButtonWide(Screen.HCenter-600, Screen.VCenter+260, "Save and Exit"))
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }
        
        if (ButtonWide(Screen.HCenter-600, Screen.VCenter+220, "Save to file"))
        {
            int number = 1;
            while (true)
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + $"/forts/fort{number}.fort"))
                {
                    Resources.SaveFort($"forts/fort{number}");
                    _saveMessage = $"Saved fort as fort{number}.fort";
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

        if (ButtonNarrow(Screen.HCenter+150, Screen.VCenter+260, "Erase", _brush != null)) _brush = null;
        if (ButtonNarrow(Screen.HCenter+300, Screen.VCenter-300, (_newUtil ? "NEW! " : "") + "Utility", _structureClass != StructureTemplate.StructureClass.Utility)) _structureClass = StructureTemplate.StructureClass.Utility;
        if (ButtonNarrow(Screen.HCenter+400, Screen.VCenter-300, (_newTower ? "NEW! " : "") + "Tower", _structureClass != StructureTemplate.StructureClass.Tower)) _structureClass = StructureTemplate.StructureClass.Tower;
        if (ButtonNarrow(Screen.HCenter+500, Screen.VCenter-300, (_newNest ? "NEW! " : "") + "Nest", _structureClass != StructureTemplate.StructureClass.Nest)) _structureClass = StructureTemplate.StructureClass.Nest;

        //Console.WriteLine(RayGui.GuiTextBox(new Rectangle(10, 620, 400, 200), "Fort Name:", 12, true));
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            StructureTemplate s = Assets.Structures[i];
            if ((!_sandboxMode && s.LevelRequirement > Program.Campaign.Level) || s.Class != _structureClass) continue;
            string label = ((!_sandboxMode && s.LevelRequirement == Program.Campaign.Level) ? "NEW! " : "") + s.Name + " - $" + s.Price;
            if (ButtonWide(Screen.HCenter+300, Screen.VCenter + y * 40 - 250, label, _brush != s))
            {
                _brush = s;
            }
            DrawTexture(s.Texture, Screen.HCenter + 320, Screen.VCenter + y * 40 - 246, WHITE);
            y++;
        }

        if (!_sandboxMode)
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter+ 200, $"Bug Dollars: ${Program.Campaign.Money}");
        }
        
        if (_brush == null)
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter-290, "ERASING");
        }
        else
        {
            DrawTextLeft(Screen.HCenter-590, Screen.VCenter-290, $"Placing {_brush.GetDescription()}");
        }



        if (CheckCollisionPointRec(GetMousePosition(), new Rectangle(Screen.HCenter-264, Screen.VCenter-264, 528, 528)))
        {
            BeginMode2D(World.Camera);
            Vector2 mousePos = World.GetTileCenter(World.GetMouseTilePos());
            if (_brush is TurretTemplate t)
            {
                DrawCircleLines((int)mousePos.X, (int)mousePos.Y, (int)t.Range, new Color(200, 50, 50, 255));
                DrawCircle((int)mousePos.X, (int)mousePos.Y, (int)t.Range, new Color(200, 50, 50, 64));
            }
            if (_brush != null)
            {
                DrawTexture(_brush.Texture, (int)mousePos.X-12, (int)mousePos.Y-(_brush.Texture.height - 12), new Color(128, 128, 255, 128));
            }
            EndMode2D();
        }
        
        DrawTextLeft(Screen.HCenter-590, Screen.VCenter+180, _saveMessage);
        DrawTextLeft(Screen.HCenter-590, Screen.VCenter+50, GetFortStats());
        
        EndDrawing();
    }

    private static string GetFortStats()
    {
        int structureCount = 0;
        int turretCount = 0;
        int utilityCount = 0;
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
                if (t.Template.Class == StructureTemplate.StructureClass.Utility) utilityCount++;
                if (t.Template.Class == StructureTemplate.StructureClass.Tower) turretCount++;
                if (t.Template.Class == StructureTemplate.StructureClass.Nest) nestCount++;
            }
        }

        if (!_sandboxMode)
        {
            _nestCapped = nestCount >= Program.Campaign.Level * 2 + 10;
        }
        
        return $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               nestCount + (_sandboxMode ? "" : "/"+(Program.Campaign.Level*2+10)) + " Nests\n" +
               $"{structureCount} Total\n" +
               $"{totalCost} Cost";
    }
}
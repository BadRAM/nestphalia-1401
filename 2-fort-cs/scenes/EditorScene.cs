using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class EditorScene
{
    private static bool _creativeMode = false;
    private static StructureTemplate? _brush;
    private static Fort _fort;
    private static string _saveMessage;
    private static string _fortStats;
    
    public static void Start(Fort? fortToLoad = null, bool creativeMode = false)
    {
        _creativeMode = creativeMode;
        _fort = fortToLoad ?? new Fort();
        
        Program.CurrentScene = Scene.Editor;
        World.InitializeEditor();
        World.Camera.offset = new Vector2(Screen.HCenter, Screen.VCenter);
        _fort.LoadToBoard(false);
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----
        
        World.Camera.offset = new Vector2(Screen.HCenter-300, Screen.VCenter - 300);

        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            Int2D tilePos = World.GetMouseTilePos();
            
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21 
                && _brush != World.GetTile(tilePos)?.Template)
            {
                if (_creativeMode)
                {
                    World.SetTile(_brush, World.LeftTeam, tilePos);
                }
                else
                {
                    if (_brush == null || _brush.Price < Program.Campaign.Money)
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
        ClearBackground(Raylib.BLACK);
        World.Draw();

        if (_creativeMode)
        {
            if (RayGui.GuiButton(new Rectangle(Screen.HCenter-600, Screen.VCenter+250, 280, 50), "Exit") != 0) MenuScene.Start();
        }
        else
        {
            if (RayGui.GuiButton(new Rectangle(Screen.HCenter-600, Screen.VCenter+250, 280, 50), "Save and Exit") != 0)
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }
        
        if (RayGui.GuiButton(new Rectangle(Screen.HCenter-300, Screen.VCenter+250, 280, 50), "Save to file") != 0)
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

        if (RayGui.GuiButton(new Rectangle(Screen.HCenter+300, Screen.VCenter-300, 300, 36), "Erase") != 0) _brush = null;

        //Console.WriteLine(RayGui.GuiTextBox(new Rectangle(10, 620, 400, 200), "Fort Name:", 12, true));
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            if (!_creativeMode && Assets.Structures[i].LevelRequirement > Program.Campaign.Level) continue;
            if (RayGui.GuiButton(new Rectangle(Screen.HCenter+300, Screen.VCenter + y * 40 - 250, 300, 36), Assets.Structures[i].Name) != 0)
            {
                _brush = Assets.Structures[i];
            }
            y++;
        }

        if (!_creativeMode)
        {
            DrawTextEx(Resources.Font, $"Bug Dollars: ${Program.Campaign.Money}", new Vector2(Screen.HCenter-590, Screen.VCenter+ 200), 12, 1, WHITE);
        }

        if (_brush == null)
        {
            DrawTextEx(Resources.Font, $"ERASING", new Vector2(Screen.HCenter-590, Screen.VCenter-290), 12, 1, WHITE);
        }
        else
        {
            DrawTextEx(Resources.Font, $"Placing {_brush.GetDescription()}", new Vector2(Screen.HCenter-590, Screen.VCenter-290), 12, 1, WHITE);
        }
        
        DrawTextEx(Resources.Font, _saveMessage, new Vector2(Screen.HCenter-590, Screen.VCenter+180), 12, 1, WHITE);
        DrawTextEx(Resources.Font, GetFortStats(), new Vector2(Screen.HCenter-590, Screen.VCenter+50), 12, 1, WHITE);
        
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
                if (t is Turret) turretCount++;
                if (t is Door) utilityCount++;
                if (t is Spawner) nestCount++;
            }
        }
        
        return $"{turretCount} Towers\n" +
               $"{utilityCount} Utility\n" +
               $"{nestCount} Nests\n" +
               $"{structureCount} Total\n" +
               $"{totalCost} Cost";
    }
}
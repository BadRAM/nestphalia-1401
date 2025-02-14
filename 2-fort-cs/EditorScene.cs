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
        World.Initialize(true);
        World.Camera.offset = new Vector2(300, 0);
        _fort.LoadToBoard();
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----
        // if (IsKeyPressed(KeyboardKey.KEY_ONE))
        // {
        //     _brush = Assets.Tiles[0];
        // }
        // if (IsKeyPressed(KeyboardKey.KEY_TWO))
        // {
        //     _brush = Assets.Tiles[2];
        // }
        // if (IsKeyPressed(KeyboardKey.KEY_THREE))
        // {
        //     _brush = Assets.Tiles[3];
        // }
        // if (IsKeyPressed(KeyboardKey.KEY_FOUR))
        // {
        //     _brush = Assets.Tiles[4];
        // }
        // if (IsKeyPressed(KeyboardKey.KEY_FIVE))
        // {
        //     _brush = Assets.Tiles[0];
        // }
	        
        // if (IsKeyPressed(KeyboardKey.KEY_F))
        // {
        //     World.Flip();
        // }
	       //  
        // if (IsKeyPressed(KeyboardKey.KEY_S))
        // {
        //     Resources.SaveFort("fortname");
        // }
        if (IsKeyPressed(KeyboardKey.KEY_L))
        {
            
        }
	        
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            Int2D tilePos = World.GetMouseTilePos();
            
            if (tilePos.X >= 1 && tilePos.X < 21 && tilePos.Y >= 1 && tilePos.Y < 21 
                && _brush != World.GetTile(tilePos)?.Template)
            {
                if (_creativeMode)
                {
                    World.SetTile(_brush, tilePos);
                }
                else
                {
                    if (_brush == null || _brush.Price < Program.Campaign.Money)
                    {
                        if (World.GetTile(tilePos) != null)
                        {
                            Program.Campaign.Money += World.GetTile(tilePos).Template.Price;
                        }
                        World.SetTile(_brush, tilePos);
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
            if (RayGui.GuiButton(new Rectangle(10, 550, 280, 50), "Exit") != 0) MenuScene.Start();
        }
        else
        {
            if (RayGui.GuiButton(new Rectangle(10, 550, 280, 50), "Save and Exit") != 0)
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }
        
        if (RayGui.GuiButton(new Rectangle(300, 550, 280, 50), "Save to file") != 0)
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

        if (RayGui.GuiButton(new Rectangle(890, 10, 300, 36), "Erase") != 0) _brush = null;

        //Console.WriteLine(RayGui.GuiTextBox(new Rectangle(10, 620, 400, 200), "Fort Name:", 12, true));
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            if (!_creativeMode && Assets.Structures[i].LevelRequirement > Program.Campaign.Level) continue;
            if (RayGui.GuiButton(new Rectangle(890, y * 40 + 60, 300, 36), Assets.Structures[i].Name) != 0)
            {
                _brush = Assets.Structures[i];
            }
            y++;
        }

        if (!_creativeMode)
        {
            DrawText($"Bug Dollars: ${Program.Campaign.Money}", 10, 520, 10, WHITE);
        }

        if (_brush == null)
        {
            DrawText($"ERASING", 10, 10, 10, WHITE);
        }
        else
        {
            DrawText($"Placing {_brush.GetDescription()}", 10, 10, 10, WHITE);
        }
        
        DrawText(_saveMessage, 10, 480, 10, WHITE);
        DrawText(GetFortStats(), 10, 350, 10, WHITE);
        
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
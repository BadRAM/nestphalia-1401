using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class EditorScene
{
    private static bool _creativeMode = false;
    private static StructureTemplate? _brush;
    private static Fort _fort;
    
    public static void Start(Fort? fortToLoad = null, bool creativeMode = false)
    {
        _creativeMode = creativeMode;
        _fort = fortToLoad ?? new Fort();
        
        Program.CurrentScene = Scene.Editor;
        World.Initialize(true);
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
            Int2D tilePos = World.PosToTilePos(GetMousePosition());
            
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
            Int2D tilePos = World.PosToTilePos(GetMousePosition());
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
            if (RayGui.GuiButton(new Rectangle(10, 620, 400, 200), "Save") != 0) Resources.SaveFort("creativeFort");
        }
        else
        {
            if (RayGui.GuiButton(new Rectangle(10, 620, 400, 200), "Save and Exit") != 0)
            {
                _fort.SaveBoard();
                Program.Campaign.Start();
            }
        }

        if (RayGui.GuiButton(new Rectangle(650, 10, 500, 50), "Erase") != 0) _brush = null;

        //Console.WriteLine(RayGui.GuiTextBox(new Rectangle(10, 620, 400, 200), "Fort Name:", 12, true));
        int y = 0;
        for (int i = 0; i < Assets.Structures.Count; i++)
        {
            if (!_creativeMode && Assets.Structures[i].LevelRequirement > Program.Campaign.Level) continue;
            if (RayGui.GuiButton(new Rectangle(650, y * 50 + 60, 500, 50), Assets.Structures[i].Name) != 0)
            {
                _brush = Assets.Structures[i];
            }
            y++;
        }

        if (!_creativeMode)
        {
            DrawText($"Bug Dollars: ${Program.Campaign.Money}", 200, 600, 10, WHITE);
        }

        if (_brush == null)
        {
            DrawText($"ERASING", 10, 600, 10, WHITE);
        }
        else
        {
            DrawText($"Placing {_brush.Name}", 10, 600, 10, WHITE);
        }

        
        EndDrawing();
    }
}
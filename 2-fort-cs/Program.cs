using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace _2_fort_cs;

class Program
{
	private static int[,] _grid = new int[48,22];
	
    public static void Main()
    {
        InitWindow(1152, 864, "2-fort");
        
        // Load a texture from the resources directory
        Texture2D wabbit = LoadTexture("resources/wabbit_alpha.png");
        Texture2D wall = LoadTexture("resources/wall.png");
        Texture2D floor1 = LoadTexture("resources/floor1.png");
        Texture2D floor2 = LoadTexture("resources/floor2.png");

        while (!WindowShouldClose())
        {
	        if (IsMouseButtonPressed(MouseButton.Left))
			{
				flip_tile();
			}
	        
            BeginDrawing();
            ClearBackground(Color.Black);


			for (int x = 0; x < _grid.GetLength(0); ++x)
			{
				for (int y = 0; y < _grid.GetLength(1); ++y)
				{
					if (_grid[x,y] != 0)
					{
						DrawTexture(wall, x*24, y*24, Color.White);
					}
					else
					{
						DrawTexture((x%2 != y%2) ? floor1 : floor2, x*24, y*24, Color.White);
					}
				}
			}
			
			DrawText("Hello, world!", 12, 12, 20, Color.Black);
			DrawTexture(wabbit, 400, 200, Color.White);

            EndDrawing();
        }

        UnloadTexture(wabbit);
        UnloadTexture(wall);
        UnloadTexture(floor1);
        UnloadTexture(floor2);
        
        CloseWindow();
    }
    
	static void flip_tile()
	{
		Vector2 mouse_pos = GetMousePosition();
		int x = (int) mouse_pos.X      / 24;
		int y = (int)(mouse_pos.Y - 8) / 24;
		if (x >= _grid.GetLength(0)) return;
		if (y >= _grid.GetLength(1)) return;
		_grid[x,y] = _grid[x,y] == 1 ? 0 : 1;
	}
}

// #include "raylib.h"
// #include "resource_dir.h"	// utility header for SearchAndSetResourceDir
//
// #define GRID_SIZE_X 48
// #define GRID_SIZE_Y 22
//
// int grid[GRID_SIZE_X][GRID_SIZE_Y];
//

//
// int main ()
// {
//
//
// 	for (int x = 0; x < GRID_SIZE_X; ++x)
// 	{
// 		for (int y = 0; y < GRID_SIZE_Y; ++y)
// 		{
// 			grid[x][y] = 0;
// 		}
// 	}
// 	grid[5][5] = 1;
// 	grid[5][6] = 1;
// 	grid[5][7] = 1;
// 	grid[6][5] = 1;
// 	grid[6][7] = 1;
// 	grid[7][5] = 1;
// 	grid[7][6] = 1;
// 	grid[7][7] = 1;
//
//
// 	// Tell the window to use vsync and work on high DPI displays
// 	SetConfigFlags(FLAG_VSYNC_HINT);
//
// 	// Create the window and OpenGL context
// 	InitWindow(1152, 864, "2-fort");
//
// 	// Utility function from resource_dir.h to find the resources folder and set it as the current working directory so we can load from it
// 	SearchAndSetResourceDir("resources");
//
//
// 	
// 	// game loop
// 	while (!WindowShouldClose())		// run the loop untill the user presses ESCAPE or presses the Close button on the window
// 	{

//
//
// 		// drawing
// 		BeginDrawing();
//
// 		// Setup the back buffer for drawing (clear color and depth buffers)
// 		ClearBackground(BLACK);
//
// 		// draw some text using the default font
// 		DrawText("2 fort", 200,200,20,WHITE);
//
// 		// draw our texture to the screen
//
// 		
// 		// end the frame and get ready for the next one  (display frame, poll input, etc...)
// 		EndDrawing();
// 	}
//
// 	// cleanup
//
// 	// destroy the window and cleanup the OpenGL context
// 	CloseWindow();
// 	return 0;
// }

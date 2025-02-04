#include "raylib.h"
#include "resource_dir.h"	// utility header for SearchAndSetResourceDir

#define GRID_SIZE_X 48
#define GRID_SIZE_Y 22

int grid[GRID_SIZE_X][GRID_SIZE_Y];

void flip_tile()
{
	Vector2 mouse_pos = GetMousePosition();
	int x = mouse_pos.x / 24;
	int y = (mouse_pos.y - 8) / 24;
	if (x >= GRID_SIZE_X) return;
	if (y >= GRID_SIZE_Y) return;
	grid[x][y] = !grid[x][y];
}

int main ()
{


	for (int x = 0; x < GRID_SIZE_X; ++x)
	{
		for (int y = 0; y < GRID_SIZE_Y; ++y)
		{
			grid[x][y] = 0;
		}
	}
	grid[5][5] = 1;
	grid[5][6] = 1;
	grid[5][7] = 1;
	grid[6][5] = 1;
	grid[6][7] = 1;
	grid[7][5] = 1;
	grid[7][6] = 1;
	grid[7][7] = 1;


	// Tell the window to use vsync and work on high DPI displays
	SetConfigFlags(FLAG_VSYNC_HINT);

	// Create the window and OpenGL context
	InitWindow(1152, 864, "2-fort");

	// Utility function from resource_dir.h to find the resources folder and set it as the current working directory so we can load from it
	SearchAndSetResourceDir("resources");

	// Load a texture from the resources directory
	Texture wabbit = LoadTexture("wabbit_alpha.png");
	Texture wall = LoadTexture("wall.png");
	Texture floor1 = LoadTexture("floor1.png");
	Texture floor2 = LoadTexture("floor2.png");
	
	// game loop
	while (!WindowShouldClose())		// run the loop untill the user presses ESCAPE or presses the Close button on the window
	{
		if (IsMouseButtonPressed(MOUSE_BUTTON_LEFT))
		{
			flip_tile();
		}


		// drawing
		BeginDrawing();

		// Setup the back buffer for drawing (clear color and depth buffers)
		ClearBackground(BLACK);

		// draw some text using the default font
		DrawText("2 fort", 200,200,20,WHITE);

		// draw our texture to the screen
		DrawTexture(wabbit, 400, 200, WHITE);

		for (int x = 0; x < GRID_SIZE_X; ++x)
		{
			for (int y = 0; y < GRID_SIZE_Y; ++y)
			{
				if (grid[x][y])
				{
					DrawTexture(wall, x*24, y*24, WHITE);
				}
				else
				{
					DrawTexture((x%2 != y%2) ? floor1 : floor2, x*24, y*24, WHITE);
				}
			}
		}
		
		// end the frame and get ready for the next one  (display frame, poll input, etc...)
		EndDrawing();
	}

	// cleanup
	// unload our texture so it can be cleaned up
	UnloadTexture(wabbit);

	// destroy the window and cleanup the OpenGL context
	CloseWindow();
	return 0;
}

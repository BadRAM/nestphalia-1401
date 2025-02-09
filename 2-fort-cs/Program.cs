using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace _2_fort_cs;

static class Program
{
	private static TileTemplate brush;
	private static bool Paused;
	
    public static void Main()
    {
        InitWindow(1152, 864, "2-fort");
        SetTargetFPS(60);
        
        // Load a texture from the resources directory
        Resources.Load();
        Assets.Load();
        brush = Assets.Tiles[0];
        
        World.Initialize();
        Resources.LoadFort();
        World.Flip();
        Resources.LoadFort();
        
        MenuScene.Start();
        
        /*
        while (!WindowShouldClose())
        {
	        /*
	        // ----- INPUT + GUI PHASE -----
	        if (IsKeyPressed(KeyboardKey.One))
	        {
		        brush = Assets.Tiles[0];
	        }
	        if (IsKeyPressed(KeyboardKey.Two))
	        {
		        brush = Assets.Tiles[2];
	        }
	        if (IsKeyPressed(KeyboardKey.Three))
	        {
		        brush = Assets.Tiles[3];
	        }
	        if (IsKeyPressed(KeyboardKey.Four))
	        {
		        brush = Assets.Tiles[4];
	        }
	        if (IsKeyPressed(KeyboardKey.Five))
	        {
		        brush = Assets.Tiles[0];
	        }
	        
	        if (IsKeyPressed(KeyboardKey.F))
	        {
		        World.Flip();
	        }
	        
	        if (IsKeyPressed(KeyboardKey.S))
	        {
		        Resources.SaveFort();
	        }
	        if (IsKeyPressed(KeyboardKey.L))
	        {
		        Resources.LoadFort();
	        }
	        
	        if (IsMouseButtonDown(MouseButton.Left))
	        {
		        Int2D tilePos = World.PosToTilePos(GetMousePosition());
		        if (brush != World.GetTile(tilePos).Template)
		        {
			        World.SetTile(brush, tilePos);
		        }
	        }
	        
	        if (IsMouseButtonDown(MouseButton.Right))
	        {
		        Vector2 t = GetMousePosition();
		        foreach (Minion m in World.Minions)
		        {
			        m.SetTarget(t);
		        }
	        }
			
	        if (IsKeyPressed(KeyboardKey.Space))
	        {
		        SetTargetFPS(300);
	        }
			
	        if (IsKeyReleased(KeyboardKey.Space))
	        {
		        SetTargetFPS(60);
	        }
			
	        if (IsKeyPressed(KeyboardKey.P))
	        {
		        Paused = !Paused;
	        }
	        
	        
	        // ----- WORLD UPDATE PHASE -----
	        if (!Paused)
	        {
		        World.Update();
	        }
	        
	        
	        // ----- DRAW PHASE -----
            BeginDrawing();
            ClearBackground(Color.Black);
			
            World.Draw();
			
            //DrawText($"Minion 0's wherabouts: X={World.Minions[0].Position.X} Y={World.Minions[0].Position.Y}", 12, 12, 20, Color.White);
            DrawText($"FPS: {GetFPS()}", 12, 16, 20, Color.White);
            DrawText($"Wave: {World.Wave}", 12, 32, 20, Color.White);
            DrawText($"Minions: {World.Minions.Count}", 12, 48, 20, Color.White);
            DrawText($"Projectiles: {World.Projectiles.Count}", 12, 64, 20, Color.White);
            
			//DrawTexture(Resources.wabbit, 400, 200, Color.White);
			
            EndDrawing();
            
        }
        */

	    Resources.Unload();
        
        CloseWindow();
    }
}
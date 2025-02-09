using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public class BattleScene
{
    public Fort PlayerFort;
    public Fort EnemyFort;

    public void Start()
    {
        if (PlayerFort == null || EnemyFort == null)
        {
            Console.WriteLine("Null fort!");
            return;
        }

        World.Initialize();
        EnemyFort.Load();
        World.Flip();
        PlayerFort.Load();

        while (!WindowShouldClose())
        {
            // ----- INPUT + GUI PHASE -----
	        
	        if (IsKeyPressed(KeyboardKey.KEY_F))
	        {
		        World.Flip();
	        }
	        
	        if (IsKeyPressed(KeyboardKey.KEY_S))
	        {
		        Resources.SaveFort();
	        }
	        if (IsKeyPressed(KeyboardKey.KEY_L))
	        {
		        Resources.LoadFort();
	        }

	        if (IsKeyReleased(KeyboardKey.KEY_SPACE))
	        {
		        SetTargetFPS(60);
	        }

	        // if (IsKeyPressed(KeyboardKey.P))
	        // {
		       //  Paused = !Paused;
	        // }
	        
	        
	        // ----- WORLD UPDATE PHASE -----
	        //if (!Paused)
	        // {
		        World.Update();
	        // }
	        
	        
	        // ----- DRAW PHASE -----
            BeginDrawing();
            ClearBackground(BLACK);
			
            World.Draw();
			
            //DrawText($"Minion 0's wherabouts: X={World.Minions[0].Position.X} Y={World.Minions[0].Position.Y}", 12, 12, 20, Color.White);
            DrawText($"FPS: {GetFPS()}", 12, 16, 20, WHITE);
            DrawText($"Wave: {World.Wave}", 12, 32, 20, WHITE);
            DrawText($"Minions: {World.Minions.Count}", 12, 48, 20, WHITE);
            DrawText($"Projectiles: {World.Projectiles.Count}", 12, 64, 20, WHITE);
            
			//DrawTexture(Resources.wabbit, 400, 200, Color.White);
			
            EndDrawing();
        }
    }
}
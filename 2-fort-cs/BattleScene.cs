using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class BattleScene
{
    // public static Fort PlayerFort;
    // public static Fort EnemyFort;

    public static void Start(Fort playerFort, Fort enemyFort)
    {
        if (playerFort == null || enemyFort == null)
        {
            Console.WriteLine("Null fort!");
            return;
        }

        World.Initialize();
        enemyFort.Load();
        World.Flip();
        playerFort.Load();

        Program.CurrentScene = Scene.Battle;
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----
	        
	        
        // ----- WORLD UPDATE PHASE -----

        World.Update();
        
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(BLACK);
			
        World.Draw();
			
        //DrawText($"Minion 0's wherabouts: X={World.Minions[0].Position.X} Y={World.Minions[0].Position.Y}", 12, 12, 20, Color.White);
        DrawText($"FPS: {GetFPS()}", 12, 16, 20, WHITE);
        DrawText($"Wave: {World.Wave}", 12, 32, 20, WHITE);
        DrawText($"Minions: {World.Minions.Count}", 12, 48, 20, WHITE);
        // DrawText($"Projectiles: {World.Projectiles.Count}", 12, 64, 20, WHITE);
            
        //DrawTexture(Resources.wabbit, 400, 200, Color.White);
			
        EndDrawing();
    }
}
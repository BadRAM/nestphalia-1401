using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class BattleScene
{
    // public static Fort PlayerFort;
    // public static Fort EnemyFort;
    public static bool Pause;
    public static bool CustomBattle;

    public static void Start(Fort playerFort, Fort enemyFort)
    {
        if (playerFort == null || enemyFort == null)
        {
            Console.WriteLine("Null fort!");
            return;
        }

        World.Initialize(false);
        enemyFort.LoadToBoard();
        World.Flip();
        playerFort.LoadToBoard();

        Program.CurrentScene = Scene.Battle;
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----

        if (IsKeyPressed(KeyboardKey.KEY_P))
        {
            Pause = !Pause;
            Time.TimeScale = Pause ? 0 : 1;
        }
        
        // ----- WORLD UPDATE PHASE -----

        if (!Pause)
        {
            World.Update();
        }
        
        TeamName winner = CheckWinner();
        if (winner != TeamName.None)
        {
            if (CustomBattle)
            {
                CustomBattleMenu.Start();
                CustomBattleMenu.OutcomeMessage =
                    (winner == TeamName.Player
                        ? CustomBattleMenu.PlayerFort?.Name ?? ""
                        : CustomBattleMenu.EnemyFort?.Name ?? "") + " won the battle!";
            }
            else
            {
                Console.WriteLine($"{winner.ToString()} wins the battle!");
                Program.Campaign.ReportBattleOutcome(winner == TeamName.Player);
                Program.Campaign.Start();
            }
        }
        
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(BLACK);
        
        World.Draw();
        
        //DrawText($"Minion 0's wherabouts: X={World.Minions[0].Position.X} Y={World.Minions[0].Position.Y}", 12, 12, 20, Color.White);
        DrawText($"FPS: {GetFPS()}", 12, 16, 20, WHITE);
        DrawText($"Wave: {World.Wave}", 12, 32, 20, WHITE);
        DrawText($"Minions: {World.Minions.Count}", 12, 48, 20, WHITE);
        DrawText($"Path Queue Length: {PathFinder.GetQueueLength()}", 12, 64, 20, WHITE);
        // DrawText($"Projectiles: {World.Projectiles.Count}", 12, 64, 20, WHITE);
        if (Pause) DrawText("PAUSED", 520, 250, 40, WHITE);
        
        EndDrawing();
    }

    private static TeamName CheckWinner()
    {
        bool playerDead = true;
        bool enemyDead = true;
        
        for (int x = 0; x < World.BoardWidth; x++)
        {
            for (int y = 0; y < World.BoardHeight; y++)
            {
                if (World.GetTile(x,y) is Spawner)
                {
                    if (x < 24)
                    {
                        playerDead = false;
                    }
                    else
                    {
                        enemyDead = false;
                    }
                }
            }
        }

        if (playerDead) return TeamName.Enemy;
        if (enemyDead) return TeamName.Player;
        return TeamName.None;
    }
}
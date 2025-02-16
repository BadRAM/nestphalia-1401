using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class BattleScene
{
    // public static Fort PlayerFort;
    // public static Fort EnemyFort;
    public static bool Pause;
    public static bool CustomBattle;

    public static void Start(Fort leftFort, Fort rightFort)
    {
        if (leftFort == null || rightFort == null)
        {
            Console.WriteLine("Null fort!");
            return;
        }

        World.InitializeBattle(leftFort, rightFort);
        
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

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            World.Camera.offset.X -= 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            World.Camera.offset.X += 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            World.Camera.offset.Y -= 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            World.Camera.offset.Y += 4;
        }
        
        if (IsKeyPressed(KeyboardKey.KEY_F))
        {
            SetTargetFPS(10000);
        }
        if (IsKeyReleased(KeyboardKey.KEY_F))
        {
            SetTargetFPS(60);
        }
        
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            World.Camera.offset += GetMouseDelta();
        }

        
        // Snipped from raylib example
        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            // Get the world point that is under the mouse
            Vector2 mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), World.Camera);

            // Set the offset to where the mouse is
            World.Camera.offset = GetMousePosition();

            // Set the target to match, so that the camera maps the world space point 
            // under the cursor to the screen space point under the cursor at any zoom
            World.Camera.target = mouseWorldPos;

            // Zoom increment
            // float scaleFactor = 1.0f + 0.25f;
            // if (wheel < 0) scaleFactor = 1.0f/scaleFactor;
            World.Camera.zoom = Math.Clamp(World.Camera.zoom + wheel * 0.5f, 0.5f, 4f);
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
        // DrawText($"Path Queue Length: {PathFinder.GetQueueLength()}", 12, 64, 20, WHITE);
        DrawText($"Zoom: {World.Camera.zoom}", 12, 64, 20, WHITE);
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
using System.Numerics;
using ZeroElectric.Vinculum;
using static _2_fort_cs.GUI;
using static ZeroElectric.Vinculum.Raylib;

namespace _2_fort_cs;

public static class BattleScene
{
    // public static Fort PlayerFort;
    // public static Fort EnemyFort;
    public static bool Pause;
    public static bool CustomBattle;
    private static int _skips; 

    public static void Start(Fort leftFort, Fort rightFort)
    {
        Pause = false;
        
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

            _skips = 0;
            if (IsKeyDown(KeyboardKey.KEY_F))
            {
                double startTime = GetTime();
                while (GetTime() - startTime < 0.016)
                {
                    Time.UpdateTime();
                    World.Update();
                    _skips++;
                }
            }
        }
        
        TeamName winner = CheckWinner();
        if (winner != TeamName.None)
        {
            SetTargetFPS(60);

            if (CustomBattle)
            {
                CustomBattleMenu.Start();
                CustomBattleMenu.OutcomeMessage =
                    (winner == TeamName.Player
                        ? CustomBattleMenu.LeftFort?.Name ?? ""
                        : CustomBattleMenu.RightFort?.Name ?? "") + " won the battle!";
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
        
        DrawTextLeft(12, 16, $"FPS: {GetFPS()}");
        DrawTextLeft(12, 32, $"Wave: {World.Wave}");
        DrawTextLeft(12, 48, $"Bugs: {World.Minions.Count}");
        DrawTextLeft(12, 64, $"Sprites: {World.Sprites.Count}");
        DrawTextLeft(12, 80, $"Zoom: {World.Camera.zoom}");
        
        if (IsKeyDown(KeyboardKey.KEY_F))
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{_skips+1}X SPEED", 48);
        }
        
        if (Pause)
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter, "PAUSED", 48);
            if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Quit"))
            {
                SetTargetFPS(60);

                if (CustomBattle)
                {
                    CustomBattleMenu.Start();
                    CustomBattleMenu.OutcomeMessage = "";
                }
                else
                {
                    Program.Campaign.ReportBattleOutcome(winner == TeamName.Enemy);
                    Program.Campaign.Start();
                }
            }
            //DrawTextEx(Resources.Font, "PAUSED", new Vector2(Screen.HCenter, Screen.VCenter), 48, 4, WHITE);
        }
        
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
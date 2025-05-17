using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class BattleScene
{
    public static Fort LeftFort;
    public static Fort RightFort;
    public static bool Pause;
    public static bool CustomBattle;
    public static Team? Winner;
    private static int _skips;
    private static bool _pathFinderDebug;
    private static bool _debug;
    public static Stopwatch SwTotal;
    public static Stopwatch SwPathfinding;
    public static Stopwatch SwCollision;
    public static Stopwatch SwDraw;
    

    public static void Start(Fort leftFort, Fort rightFort, bool leftIsPlayer = true, bool rightIsPlayer = false, bool deterministic = false)
    {
        Winner = null;
        
        LeftFort = leftFort;
        RightFort = rightFort;
        
        Pause = false;
        Time.TimeScale = 1;
        
        if (LeftFort == null || RightFort == null)
        {
            Console.WriteLine("Null fort!");
            return;
        }

        World.InitializeBattle(leftFort, rightFort, leftIsPlayer, rightIsPlayer, deterministic);
        
        Program.CurrentScene = Scene.Battle;
        
        Resources.PlayMusicByName("jesper-kyd-highlands");
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----

        if (IsKeyPressed(KeyboardKey.P) || IsKeyPressed(KeyboardKey.Escape))
        {
            Pause = !Pause;
            Time.TimeScale = Pause ? 0 : 1;
        }

        if (IsKeyPressed(KeyboardKey.F))
        {
            SetMasterVolume(0.25f);
        }
        if (IsKeyReleased(KeyboardKey.F))
        {
            SetMasterVolume(1f);
        }

        if (IsKeyDown(KeyboardKey.A))
        {
            World.Camera.Offset.X += 4;
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            World.Camera.Offset.X -= 4;
        }
        if (IsKeyDown(KeyboardKey.W))
        {
            World.Camera.Offset.Y += 4;
        }
        if (IsKeyDown(KeyboardKey.S))
        {
            World.Camera.Offset.Y -= 4;
        }

        if (IsKeyPressed(KeyboardKey.Q))
        {
            _pathFinderDebug = !_pathFinderDebug;
        }
        
        if (IsKeyPressed(KeyboardKey.F3))
        {
            World.DrawDebugInfo = !World.DrawDebugInfo;
        }
        
        if (IsMouseButtonDown(MouseButton.Right))
        {
            World.Camera.Offset += GetMouseDelta();
        }
        
        
        // Snipped from raylib example
        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            // Get the world point that is under the mouse
            Vector2 mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), World.Camera);

            // Set the offset to where the mouse is
            World.Camera.Offset = GetMousePosition();

            // Set the target to match, so that the camera maps the world space point 
            // under the cursor to the screen space point under the cursor at any zoom
            World.Camera.Target = mouseWorldPos;

            // Zoom increment
            // float scaleFactor = 1.0f + 0.25f;
            // if (wheel < 0) scaleFactor = 1.0f/scaleFactor;
            World.Camera.Zoom = Math.Clamp(World.Camera.Zoom + wheel * 0.5f, 0.5f, 4f);
        }
        
        // ----- WORLD UPDATE PHASE -----

        if (!Pause && Winner == null)
        {
            double startTime = GetTime();
            
            World.Update();

            _skips = 0;
            if (IsKeyDown(KeyboardKey.F))
            {
                while ((GetTime() - startTime) + ((GetTime() - startTime) / (_skips + 1)) < 0.016)
                {
                    Time.UpdateTime();
                    World.Update();
                    _skips++;
                }
            }
        }
        
        Winner = CheckWinner();
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(new Color(16, 8, 4, 255));
        
        World.DrawFloor();
        World.Draw();
        
        // DrawTextLeft(6, 16, $"FPS: {GetFPS()}");
        // DrawTextLeft(6, 32, $"Wave: {World.Wave}");
        // DrawTextLeft(6, 48, $"Bugs: {World.Minions.Count}");
        // DrawTextLeft(6, 64, $"Sprites: {World.Sprites.Count}");
        // DrawTextLeft(6, 64, $"Zoom: {World.Camera.Zoom}");
        // DrawTextLeft(6, 80, $"Tile {World.GetMouseTilePos().ToString()}");
        // DrawTextLeft(6, 80, $"PathQueue: {PathFinder.GetQueueLength()}");
        if (_pathFinderDebug) PathFinder.DrawDebug();
        
        if (Winner != null)
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter-48, "BATTLE OVER!", 48);
            DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{Winner.Name} is victorious!", 48);
            if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Return"))
            {
                SetMasterVolume(1f);

                if (CustomBattle)
                {
                    CustomBattleMenu.Start();
                    CustomBattleMenu.OutcomeMessage = Winner.Name + " won the battle!";
                }
                else
                {
                    Program.Campaign.ReportBattleOutcome(Winner.IsPlayerControlled);
                    Program.Campaign.Start();
                }
            }
        }
        else if (Pause)
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter, "PAUSED", 48);
            if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Quit"))
            {
                if (CustomBattle)
                {
                    CustomBattleMenu.Start();
                    CustomBattleMenu.OutcomeMessage = "";
                }
                else
                {
                    Program.Campaign.ReportBattleOutcome(false);
                    Program.Campaign.Start();
                }
            }
        }
        else if (IsKeyDown(KeyboardKey.F))
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{_skips+1}X SPEED", 48);
        }
        EndDrawing();
    }

    private static Team? CheckWinner()
    {
        if (World.LeftTeam.GetHealth() <= 0) return World.RightTeam;
        if (World.RightTeam.GetHealth() <= 0) return World.LeftTeam;
        return null;
    }
}
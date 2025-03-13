using System.Diagnostics;
using System.Numerics;
using ZeroElectric.Vinculum;
using static nestphalia.GUI;
using static ZeroElectric.Vinculum.Raylib;

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
    

    public static void Start(Fort leftFort, Fort rightFort, bool leftIsPlayer = true, bool rightIsPlayer = false)
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

        World.InitializeBattle(leftFort, rightFort, leftIsPlayer, rightIsPlayer);
        
        Program.CurrentScene = Scene.Battle;
        
        Resources.PlayMusicByName("jesper-kyd-highlands");
    }

    public static void Update()
    {
        // ----- INPUT + GUI PHASE -----

        if (IsKeyPressed(KeyboardKey.KEY_P) || IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            Pause = !Pause;
            Time.TimeScale = Pause ? 0 : 1;
        }

        if (IsKeyPressed(KeyboardKey.KEY_F))
        {
            SetMasterVolume(Program.Muted ? 0 : 0.25f);
        }
        if (IsKeyReleased(KeyboardKey.KEY_F))
        {
            SetMasterVolume(Program.Muted ? 0 : 1f);
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            World.Camera.offset.X += 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            World.Camera.offset.X -= 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            World.Camera.offset.Y += 4;
        }
        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            World.Camera.offset.Y -= 4;
        }

        if (IsKeyPressed(KeyboardKey.KEY_Q))
        {
            _pathFinderDebug = !_pathFinderDebug;
        }
        
        if (IsKeyPressed(KeyboardKey.KEY_F3))
        {
            World.DrawDebugInfo = !World.DrawDebugInfo;
        }
        
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
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

        if (!Pause && Winner == null)
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
        
        Winner = CheckWinner();
        
        // ----- DRAW PHASE -----
        BeginDrawing();
        ClearBackground(new Color(16, 8, 4, 255));
        
        World.DrawFloor();
        World.Draw();
        
        DrawTextLeft(6, 16, $"FPS: {GetFPS()}");
        DrawTextLeft(6, 32, $"Wave: {World.Wave}");
        DrawTextLeft(6, 48, $"Bugs: {World.Minions.Count}");
        // DrawTextLeft(6, 64, $"Sprites: {World.Sprites.Count}");
        DrawTextLeft(6, 64, $"Zoom: {World.Camera.zoom}");
        // DrawTextLeft(6, 80, $"PathQueue: {PathFinder.GetQueueLength()}");
        if (_pathFinderDebug) PathFinder.DrawDebug();
        
        if (Winner != null)
        {
            DrawRectangle(0,0,Screen.Left,Screen.Bottom,new Color(0,0,0,128));
            DrawTextCentered(Screen.HCenter, Screen.VCenter-48, "BATTLE OVER!", 48);
            DrawTextCentered(Screen.HCenter, Screen.VCenter, $"{Winner.Name} is victorious!", 48);
            if (ButtonNarrow(Screen.HCenter-50, Screen.VCenter + 30, "Return"))
            {
                SetMasterVolume(Program.Muted ? 0 : 1f);

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
        else if (IsKeyDown(KeyboardKey.KEY_F))
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
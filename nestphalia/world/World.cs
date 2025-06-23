using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class GameWorld
{
    public static GameWorld Shared = new GameWorld();
}

public static class World
{
    public static int BoardWidth = 48;
    public static int BoardHeight = 22;
    private static FloorTile[,] _floor = new FloorTile[BoardWidth, BoardHeight];
    private static Structure?[,] _board = new Structure?[BoardWidth, BoardHeight];
    public static List<Minion> Minions = new List<Minion>();
    public static List<Minion>[,] MinionGrid = new List<Minion>[BoardWidth, BoardHeight];
    public static List<Minion> MinionsToRemove = new List<Minion>();
    public static List<Projectile> Projectiles = new List<Projectile>();
    public static List<Projectile> ProjectilesToRemove = new List<Projectile>();
    public static List<ISprite> Sprites = new List<ISprite>();
    private static Random _random = new Random();
    private static double WaveDuration = 20;
    private static bool PreWave;
    private static double PreWaveOffset = 1.6;
    private static double FirstWaveTime = 0;
    public static int Wave = 0;
    public static Camera2D Camera;
    public static Team LeftTeam;
    public static Team RightTeam;
    private static bool _battleStarted;
    private static SoundResource _waveStartSoundEffect;
    private static bool _battleOver;
    
    public static bool DrawDebugInfo;
    private static Stopwatch _swFrame = new Stopwatch();
    private static Stopwatch _swDraw = new Stopwatch();
    private static Stopwatch _swUpdate = new Stopwatch();
    private static Stopwatch _swUpdateMinionGrid = new Stopwatch();
    private static Stopwatch _swUpdateTeams = new Stopwatch();
    private static Stopwatch _swUpdatePathfinder = new Stopwatch();
    private static Stopwatch _swUpdateBoard = new Stopwatch();
    private static Stopwatch _swUpdateMinions = new Stopwatch();
    private static Stopwatch _swUpdateMinionsCollide = new Stopwatch();
    private static Stopwatch _swUpdateMinionsPostCollide = new Stopwatch();
    private static Stopwatch _swUpdateProjectiles = new Stopwatch();
#if DEBUG
    private class EntityTracker
    {
        public Stopwatch SW = new Stopwatch();
        public int Count;
    }
    private static Dictionary<string, EntityTracker> _swEntitiesByID = new Dictionary<string, EntityTracker>();
#endif
    // private static int _totalCollideChecks;
    private static string _debugString = "";
    
    public static event EventHandler<Int2D> StructureChanged = delegate {};
    
    public class StructureChangedEventArgs(int x, int y) : EventArgs
    {
        public Int2D Pos { get; set; } = new(x, y);
    }
    
    private static void Initialize()
    {
        Time.Reset();
        Camera.Target = new Vector2(BoardWidth * 12, BoardHeight * 12);
        Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter) * GUI.GetWindowScale();
        Camera.Rotation = 0;
        Camera.Zoom = 1;
        Minions.Clear();
        Projectiles.Clear();
        Sprites.Clear();
        #if DEBUG
        _swEntitiesByID.Clear();
        #endif
        FirstWaveTime = Time.Scaled;
        Wave = 0;
        _battleStarted = false;
        PreWave = false;
        _waveStartSoundEffect = Resources.GetSoundByName("start");
        _battleOver = false;
        DrawDebugInfo = false;
        
        LeftTeam = new Team("Player", false, Color.Blue);
        RightTeam = new Team("Enemy", true, Color.Red);

        for (int x = 0; x < BoardWidth; x++)
        for (int y = 0; y < BoardHeight; y++)
        {
            MinionGrid[x, y] = new List<Minion>();
        }
        
        foreach (List<Minion> minionList in MinionGrid)
        {
            minionList.Clear();
        }
    }

    public static void InitializeEditor(Fort fortToLoad)
    {
        Initialize();
        Camera.Target = new Vector2(22 * 12, BoardHeight * 12 + 8);
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        for (int y = 0; y < BoardHeight; y++)
        {
            if (x > 21)
            {
                _floor[x, y] = Assets.BlankFloor.Instantiate(x, y);
            }
            else if (x == 0 || x == 21 || y == 0 || y == 21)
            {
                _floor[x, y] = Assets.GetFloorTileByID("floor_2").Instantiate(x, y);
            }
            else
            {
                _floor[x,y] = (x%2 != y%2) ? Assets.GetFloorTileByID("floor_1").Instantiate(x,y) : Assets.GetFloorTileByID("floor_2").Instantiate(x,y);
            }
            _board[x,y] = null;
        }
        
        fortToLoad.LoadToBoard(new Int2D(1, 1), false);
        LeftTeam.Initialize();
        RightTeam.Initialize();
    }

    public static void InitializePreview()
    {
        Initialize();
        Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter+50) * GUI.GetWindowScale();
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        for (int y = 0; y < BoardHeight; y++)
        {
            _floor[x,y] = (x%2 != y%2) ? Assets.GetFloorTileByID("floor_1").Instantiate(x,y) : Assets.GetFloorTileByID("floor_2").Instantiate(x,y);
            _board[x,y] = null;
        }
    }
    
    public static void InitializeBattle(Level level, Fort leftFort, Fort? rightFort, bool leftIsPlayer, bool rightIsPlayer, bool deterministic)
    {
        Initialize();
        
        // TODO: let levels override seed
        
        _random = deterministic ? new Random(123) : new Random();
        
        // TODO: Allow levels to change world size

        // BoardWidth = level.WorldSize.X;
        // BoardHeight = level.WorldSize.Y;
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        for (int y = 0; y < BoardHeight; y++)
        {
            _floor[x,y] = (x%2 != y%2) ? Assets.GetFloorTileByID("floor_1").Instantiate(x,y) : Assets.GetFloorTileByID("floor_2").Instantiate(x,y);
            _board[x,y] = null;
        }
        
        level.LoadToBoard();
        
        // Load forts to board
        leftFort.LoadToBoard(new Int2D(1,1), false);
        rightFort?.LoadToBoard(new Int2D(26, 1), true);
        
        // Tell teams they can generate fear and hate
        LeftTeam.Name = leftFort.Name;
        LeftTeam.IsPlayerControlled = leftIsPlayer;
        LeftTeam.Initialize();
        RightTeam.Name = level.Name;
        RightTeam.IsPlayerControlled = rightIsPlayer;
        RightTeam.Color = level.EnemyColor;
        GameConsole.WriteLine($"Rightteam color set to: {RightTeam.Color.ToString()}");
        RightTeam.Initialize();
        
        Determinator.Start(deterministic);

        _battleStarted = true;
    }

    public static void InitializeLevelEditor(Level level)
    {
        Initialize();
        
        for (int x = 0; x < BoardWidth; x++)
        for (int y = 0; y < BoardHeight; y++)
        {
            _floor[x, y] = Assets.GetFloorTileByID(level.FloorTiles[x,y])?.Instantiate(x, y) ?? Assets.BlankFloor.Instantiate(x,y);
            _board[x, y] = Assets.GetStructureByID(level.Structures[x, y])?.Instantiate(RightTeam, x, y);
        }
    }
    
    public static void Update()
    {
        if (true) // set to false for total time breakdown
        {
            _swUpdate.Reset();
            _swUpdateMinionGrid.Reset();
            _swUpdateTeams.Reset();
            _swUpdateBoard.Reset();
            _swUpdateMinions.Reset();
            _swUpdateProjectiles.Reset();
            _swUpdateMinionsCollide.Reset();
            _swUpdateMinionsPostCollide.Reset();
            _swUpdatePathfinder.Reset();
        }
        
        _swUpdate.Start();
        _debugString = "";
        
        _swUpdateMinionGrid.Start();
        foreach (List<Minion> gridSquare in MinionGrid)
        {
            gridSquare.Clear();
        }
        foreach (Minion minion in Minions)
        {
            Int2D pos = PosToTilePos(minion.Position);
            MinionGrid[pos.X, pos.Y].Add(minion);
        }
        _swUpdateMinionGrid.Stop();
        
        UpdateWaveTimer();
        
        // Reset the entity timers
        #if DEBUG
        foreach (var e in _swEntitiesByID)
        {
            e.Value.SW.Reset();
            e.Value.Count = 0;
        }
        #endif

        _swUpdateTeams.Start();
        LeftTeam.Update();
        RightTeam.Update();
        _swUpdateTeams.Stop();
        
        _swUpdateBoard.Start();
        for (int x = 0; x < BoardWidth; ++x)
        for (int y = 0; y < BoardHeight; ++y)
        {
            if (_board[x,y] == null) continue;
            #if DEBUG
            string key = _board[x,y].Template.ID;
            if (!_swEntitiesByID.ContainsKey(key)) _swEntitiesByID.Add(key, new EntityTracker());
            _swEntitiesByID[key].SW.Start();
            #endif
            _board[x,y].Update();
            #if DEBUG
            _swEntitiesByID[key].SW.Stop();
            _swEntitiesByID[key].Count++;
            #endif
        }
        _swUpdateBoard.Stop();
        
        _swUpdateMinions.Start();
        for (int index = 0; index < Minions.Count; index++)
        {
            #if DEBUG
            string key = Minions[index].Template.ID;
            if (!_swEntitiesByID.ContainsKey(key)) _swEntitiesByID.Add(key, new EntityTracker());
            _swEntitiesByID[key].SW.Start();
            #endif
            Minions[index].Update();
            #if DEBUG
            _swEntitiesByID[key].SW.Stop();
            _swEntitiesByID[key].Count++;
            #endif
        }

        foreach (Minion m in MinionsToRemove)
        {
            Minions.Remove(m);
            Sprites.Remove(m);
        }
        MinionsToRemove.Clear();
        _swUpdateMinions.Stop();
        
        _swUpdateProjectiles.Start();
        for (int index = 0; index < Projectiles.Count; index++)
        {
        #if DEBUG
            string key = Projectiles[index].Template.ID;
            if (!_swEntitiesByID.ContainsKey(key)) _swEntitiesByID.Add(key, new EntityTracker());
            _swEntitiesByID[key].SW.Start();
        #endif
            Projectiles[index].Update();
        #if DEBUG
            _swEntitiesByID[key].SW.Stop();
            _swEntitiesByID[key].Count++;
        #endif
        }

        foreach (Projectile p in ProjectilesToRemove)
        {
            Projectiles.Remove(p);
            Sprites.Remove(p);
        }
        ProjectilesToRemove.Clear();
        _swUpdateProjectiles.Stop();
        
        _swUpdatePathfinder.Start();
        // LeftTeam.ServeQueue(10);
        // RightTeam.ServeQueue(10);
        Task pathfinderTask = Task.Run(() => { LeftTeam.ServeQueue(10); RightTeam.ServeQueue(10); });
        // Task leftPathfinderTask = Task.Run(() => LeftTeam.ServeQueue(10));
        // Task rightPathfinderTask = Task.Run(() => RightTeam.ServeQueue(10));
        _swUpdatePathfinder.Stop();
        
        _swUpdateMinionsCollide.Start();
        DoMinionCollision();
        _swUpdateMinionsCollide.Stop();
        
        _swUpdateMinionsPostCollide.Start();
        foreach (Minion m in Minions)
        {
            m.ApplyPush();
            Physics.CollideTerrain(m);
            // m.CollideTerrain();
            // m.ApplyPush();
        }
        _swUpdateMinionsPostCollide.Stop();
        
        _swUpdatePathfinder.Start();
        pathfinderTask.Wait();
        // rightPathfinderTask.Wait();
        _swUpdatePathfinder.Stop();
        
        Determinator.Update();
        
        _swUpdate.Stop();
    }

    private static void UpdateWaveTimer()
    {
        // Update wave timer
        if (!_battleOver)
        {
            if (PreWave)
            {
                if (Time.Scaled - (FirstWaveTime + PreWaveOffset) > WaveDuration * Wave)
                {
                    Wave++;
                    PreWave = false;
                    for (int x = 0; x < BoardWidth; ++x)
                    for (int y = 0; y < BoardHeight; ++y)
                    {
                        _board[x, y]?.WaveEffect();
                    }
                }
            }
            else
            {
                if (Time.Scaled - FirstWaveTime > WaveDuration * Wave)
                {
                    _waveStartSoundEffect.Play();

                    PreWave = true;
                    for (int x = 0; x < BoardWidth; ++x)
                    for (int y = 0; y < BoardHeight; ++y)
                    {
                        _board[x, y]?.PreWaveEffect();
                    }
                }
            }
        }
    }

    private static void DoMinionCollision()
    {
        // _totalCollideChecks = 0;
        for (int x = 0; x < BoardWidth; x++) // Iterate MinionGrid Columns
        for (int y = 0; y < BoardHeight; y++) // Iterate MinionGrid Rows
        {
            for (int i = 0; i < MinionGrid[x, y].Count; i++) // Iterate Minions in cell at x,y
            {
                for (int j = i+1; j < MinionGrid[x, y].Count; j++)
                {
                    Physics.CollideMinion(MinionGrid[x, y][i], MinionGrid[x, y][j]);
                    // _totalCollideChecks++;
                }

                if (x < BoardWidth - 1)
                {
                    if (y > 0)
                    {
                        for (int j = 0; j < MinionGrid[x + 1, y - 1].Count; j++)
                        {
                            Physics.CollideMinion(MinionGrid[x, y][i], MinionGrid[x + 1, y - 1][j]);
                            // _totalCollideChecks++;
                        }
                    }

                    for (int j = 0; j < MinionGrid[x + 1, y].Count; j++)
                    {
                        Physics.CollideMinion(MinionGrid[x, y][i], MinionGrid[x + 1, y][j]);
                        // _totalCollideChecks++;
                    }

                    if (y < BoardHeight - 1)
                    {
                        for (int j = 0; j < MinionGrid[x + 1, y + 1].Count; j++)
                        {
                            Physics.CollideMinion(MinionGrid[x, y][i], MinionGrid[x + 1, y + 1][j]);
                            // _totalCollideChecks++;
                        }
                    }
                }

                if (y < BoardHeight - 1)
                {
                    for (int j = 0; j < MinionGrid[x, y + 1].Count; j++)
                    {
                        Physics.CollideMinion(MinionGrid[x, y][i], MinionGrid[x, y + 1][j]);
                        // _totalCollideChecks++;
                    }
                }
            }
        }
    }

    public static void DrawFloor()
    {
        Raylib.BeginMode2D(Camera);
        
        for (int x = 0; x < BoardWidth; ++x)
        for (int y = 0; y < BoardHeight; ++y)
        {
            _floor[x,y].Draw(x*24, y*24);
        }
        
        Raylib.EndMode2D();
    }

    public static void Draw()
    {
        _swDraw.Restart();
        Raylib.BeginMode2D(Camera);

        Sprites = Sprites.OrderBy(o => o.GetDrawOrder()).ToList();

        foreach (ISprite s in Sprites)
        {
            s.Draw();
        }
        
        Raylib.EndMode2D();

        if (_battleStarted)
        {
            LeftTeam.Draw();
            RightTeam.Draw();
        }
        _swDraw.Stop();
        
        if (DrawDebugInfo) DrawDebug();
        _swFrame.Restart();
    }

    public static void DrawDebug()
    {
        // ReSharper disable once InconsistentNaming
        double totalSWTime = _swFrame.Elapsed.TotalSeconds;

        string tileTypeTotals = "";
        
        #if DEBUG
        List<KeyValuePair<string, EntityTracker>> tileTypeStopwatches = _swEntitiesByID.ToList();
        tileTypeStopwatches = tileTypeStopwatches.OrderByDescending(o => o.Value.SW.Elapsed).ToList();
        
        foreach (var t in tileTypeStopwatches)
        {
            if (t.Value.Count == 0) continue;
            tileTypeTotals += $"{t.Key} : {t.Value.SW.Elapsed.TotalMicroseconds.ToString("N5")} / {t.Value.Count} = {(t.Value.SW.Elapsed.TotalMicroseconds / t.Value.Count).ToString("N5")}\n";
        }
        #endif
        
        GUI.DrawTextLeft(10, 10,
            $"FPS: {Raylib.GetFPS()}\n" +
            $"Wave: {Wave}\n" +
            $"Bugs: {Minions.Count}\n" +
            $"Sprites: {Sprites.Count}\n" +
            $"Zoom: {Camera.Zoom}\n" +
            $"Tile {GetMouseTilePos().ToString()}\n" +
            $"Fate: {(Determinator.Fate == Determinator.FateModes.Guarding ? "SET IN STONE" : Determinator.battleName)}\n" +
            // $"Total collision checks: {_totalCollideChecks/1000}k\n" +
            // $"ms/1k checks: {((_swUpdateMinionsCollide.Elapsed.TotalMilliseconds * 1000) / _totalCollideChecks).ToString("N4")}\n\n" +
            $"{tileTypeTotals}\n" +
            $"{_debugString}", guiSpace:false);
        
        int totalWidth = 1000;
        int x = Screen.HCenter-500;
        
        int width = (int)(totalWidth * _swUpdate.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 0, width, 10, Color.Red);
        x += width;
        width = (int)(totalWidth * _swDraw.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 0, width, 10, Color.Green);
        x += width;
        width = totalWidth - x;
        Raylib.DrawRectangle(x, 0, width, 10, Color.Gray);

        totalSWTime = _swUpdate.Elapsed.TotalSeconds;
        x = Screen.HCenter - 500;
        width = (int)(totalWidth * _swUpdateMinionGrid.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.Top+10, width, 20, Color.Red);
        GUI.DrawTextLeft(x, Screen.Top+10, $"MinionGrid {(int)(100 * _swUpdateMinionGrid.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateMinionGrid.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdateTeams.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.Top+10, width, 20, Color.SkyBlue);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Teams {(int)(100 * _swUpdateTeams.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateTeams.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdateBoard.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Purple);
        GUI.DrawTextLeft(x, 10, $"Board {(int)(100 * _swUpdateBoard.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateBoard.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdateMinions.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Yellow);
        GUI.DrawTextLeft(x, 10, $"Minions {(int)(100 * _swUpdateMinions.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateMinions.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdateProjectiles.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Blue);
        GUI.DrawTextLeft(x, 10, $"Proj {(int)(100 * _swUpdateProjectiles.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateProjectiles.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdateMinionsCollide.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Red);
        GUI.DrawTextLeft(x, 10, $"Phys {(int)(100 * _swUpdateMinionsCollide.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateMinionsCollide.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;        
        width = (int)(totalWidth * _swUpdateMinionsPostCollide.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Maroon);
        GUI.DrawTextLeft(x, 10, $"PostPhys {(int)(100 * _swUpdateMinionsPostCollide.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateMinionsPostCollide.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = (int)(totalWidth * _swUpdatePathfinder.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, 10, width, 20, Color.Orange);
        GUI.DrawTextLeft(x, 10, $"Path {(int)(100 * _swUpdatePathfinder.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdatePathfinder.Elapsed.TotalMilliseconds):N3}ms", guiSpace:false);
        x += width;
        width = totalWidth - x;
        Raylib.DrawRectangle(x, 10, width, 20, Color.Gray);
    }
    
    public static void RegisterMinion(Minion minion)
    {
        Minions.Add(minion);
        Sprites.Add(minion);
    }
    
    public static void SetTile(StructureTemplate? tile, Team team, int x, int y)
    {
        if (_board[x, y] != null)
        {
            Sprites.Remove(_board[x, y]!);
        }
        _board[x,y] = tile?.Instantiate(team, x, y);
        if (_board[x, y] != null)
        {
            Sprites.Add(_board[x,y]!);
        }
        
        StructureChanged.Invoke(null, new Int2D(x,y));
    }
    
    public static void SetTile(StructureTemplate? tile, Team team, Int2D tilePos)
    {
        SetTile(tile, team, tilePos.X, tilePos.Y);
    }

    public static void DestroyTile(int x, int y)
    {
        if (_board[x, y] != null)
        {
            Sprites.Remove(_board[x, y]!);
            _board[x,y] = new Rubble(_board[x, y]!.Template, _board[x, y]!.Team, x, y);
            Sprites.Add(_board[x,y]!);
        }
        
        StructureChanged.Invoke(null, new Int2D(x,y));
    }

    public static Structure? GetTile(Int2D tilePos)
    {
        if (tilePos.X < 0 || tilePos.X >= BoardWidth || tilePos.Y < 0 || tilePos.Y >= BoardHeight) return null;
        return _board[tilePos.X,tilePos.Y];
    }
    
    public static Structure? GetTile(int x, int y)
    {
        if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight) return null;
        return _board[x,y];
    }

    public static Team GetOtherTeam(Team team)
    {
        return team == LeftTeam ? RightTeam : LeftTeam;
    }

    // Returns a list of all minions in the square of tiles described by the arguments
    // Radius 1 scans 1 tile, radius 2 scans 9 tiles, etc...
    public static List<Minion> GetMinionsInRegion(Int2D center, int radius)
    {
        radius--;
        List<Minion> minions = new List<Minion>();
        for (int x = Math.Max(center.X - radius, 0); x <= Math.Min(center.X + radius, BoardWidth-1); x++)
        for (int y = Math.Max(center.Y - radius, 0); y <= Math.Min(center.Y + radius, BoardHeight-1); y++)
        {
            minions.AddRange(MinionGrid[x,y]);
        }
        return minions;
    }

    public static Int2D PosToTilePos(Vector2 position)
    {
        int x = (int) position.X      / 24;
        int y = (int)(position.Y - 8) / 24;
        x = Math.Clamp(x, 0, BoardWidth - 1);
        y = Math.Clamp(y, 0, BoardHeight - 1);
        return new Int2D(x, y);
    }
    public static Int2D PosToTilePos(Vector3 position)
    {
        int x = (int) position.X      / 24;
        int y = (int)(position.Y - 8) / 24;
        x = Math.Clamp(x, 0, BoardWidth - 1);
        y = Math.Clamp(y, 0, BoardHeight - 1);
        return new Int2D(x, y);
    }

    public static Int2D GetMouseTilePos()
    {
        return PosToTilePos(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), Camera));
    }
    
    public static Rectangle GetTileBounds(int x, int y)
    {
        Rectangle bounds = new Rectangle();
        bounds.X = x * 24;
        bounds.Y = y * 24 + 8;
        bounds.Width = 24;
        bounds.Height = 24;
        return bounds;
    }

    public static Structure? GetTileAtPos(Vector2 position)
    {
        return GetTile(PosToTilePos(position));
    }
    
    public static Structure? GetTileAtPos(Vector3 position)
    {
        return GetTile(PosToTilePos(position));
    }

    public static Vector2 GetTileCenter(int x, int y)
    {
        return new Vector2(x * 24 + 12, y * 24 + 20);
    }
    
    public static Vector2 GetTileCenter(Int2D tilePos)
    {
        return GetTileCenter(tilePos.X, tilePos.Y);
    }
    
    public static int RandomInt(int max)
    {
        //Determinator.Stacks += Environment.StackTrace + "\n\n";
        return _random.Next(max);
    }

    public static int RandomInt(int min, int max)
    {
        //Determinator.Stacks += Environment.StackTrace + "\n\n";
        return _random.Next(min, max);
    }

    public static int WeightedRandom(int max)
    {
        //Determinator.Stacks += Environment.StackTrace + "\n\n";
        return _random.WeightedRandom(max);
    }

    public static double RandomDouble()
    {
        //Determinator.Stacks += Environment.StackTrace + "\n\n";
        return _random.NextDouble();
    }

    public static void EndBattle()
    {
        _battleOver = true;
    }

    public static bool IsBattleOver()
    {
        return _battleOver;
    }

    public static void SetFloorTile(FloorTileTemplate tile, int x, int y)
    {
        _floor[x, y] = tile.Instantiate(x,y);
    }

    public static FloorTile GetFloorTile(int x, int y)
    {
        return _floor[x, y];
    }
}
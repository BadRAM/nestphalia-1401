using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public static class World
{
    public const int BoardWidth = 48;
    public const int BoardHeight = 22;
    private static FloorTile[,] _floor = new FloorTile[BoardWidth, BoardHeight];
    private static Structure?[,] _board = new Structure?[BoardWidth, BoardHeight];
    public static List<Minion> Minions = new List<Minion>();
    public static List<Minion>[,] MinionGrid = new List<Minion>[BoardWidth, BoardHeight];
    public static List<Minion> MinionsToRemove = new List<Minion>();
    public static List<Projectile> Projectiles = new List<Projectile>();
    public static List<Projectile> ProjectilesToRemove = new List<Projectile>();
    public static List<ISprite> Sprites = new List<ISprite>();
    public static Random Random = new Random();
    public static double WaveDuration = 20;
    public static bool PreWave;
    public static double PreWaveOffset = 1.6;
    public static double FirstWaveTime = 0;
    public static int Wave = 0;
    public static Camera2D Camera;
    public static Team LeftTeam;
    public static Team RightTeam;
    private static bool _battleStarted;
    private static SoundResource _waveStartSoundEffect;
    private static bool _battleOver;
    
    public static bool DrawDebugInfo;
    private static Stopwatch _swFrame = new Stopwatch();
    private static Stopwatch _swUpdate = new Stopwatch();
    private static Stopwatch _swUpdateTeams = new Stopwatch();
    private static Stopwatch _swUpdatePathfinder = new Stopwatch();
    private static Stopwatch _swUpdateBoard = new Stopwatch();
    private static Stopwatch _swUpdateMinions = new Stopwatch();
    private static Stopwatch _swUpdateMinionsCollide = new Stopwatch();
    private static Stopwatch _swUpdateProjectiles = new Stopwatch();
    
    private static Stopwatch _swDraw = new Stopwatch();
    private static Stopwatch _swDrawSort = new Stopwatch();
    private static Stopwatch _swDrawSprites = new Stopwatch();

    public static event EventHandler<Int2D> StructureChanged = delegate {};
    
    public class StructureChangedEventArgs(int x, int y) : EventArgs
    {
        public Int2D Pos { get; set; } = new(x, y);
    }
    
    private static void Initialize()
    {
        Camera.Target = new Vector2(BoardWidth * 12, BoardHeight * 12);
        Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter) * GUI.GetWindowScale();
        Camera.Rotation = 0;
        Camera.Zoom = GUI.GetWindowScale().X;
        Minions.Clear();
        Projectiles.Clear();
        Sprites.Clear();
        PathFinder.ClearQueue();
        FirstWaveTime = Time.Scaled;
        Wave = 0;
        _battleStarted = false;
        PreWave = false;
        _waveStartSoundEffect = Resources.GetSoundByName("start");
        _battleOver = false;
        DrawDebugInfo = false;
        
        LeftTeam = new Team("Player", false, Color.Blue);
        LeftTeam.IsPlayerControlled = true;
        RightTeam = new Team("Enemy", true, Color.Red);

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                MinionGrid[x, y] = new List<Minion>();
            }
        }
        foreach (List<Minion> minionList in MinionGrid)
        {
            minionList.Clear();
        }
    }

    public static void InitializeEditor()
    {
        Initialize();
        Camera.Target = new Vector2(22 * 12, BoardHeight * 12 + 8);
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (x > 21)
                {
                    _floor[x, y] = Assets.FloorTiles[2].Instantiate(x, y);
                }
                else if (x == 0 || x == 21 || y == 0 || y == 21)
                {
                    _floor[x, y] = Assets.FloorTiles[1].Instantiate(x, y);
                }
                else
                {
                    _floor[x,y] = (x%2 != y%2) ? Assets.FloorTiles[0].Instantiate(x,y) : Assets.FloorTiles[1].Instantiate(x,y);
                }
                _board[x,y] = null;
            }
        }
    }

    public static void InitializePreview()
    {
        Initialize();
        Camera.Zoom = 0.5f * GUI.GetWindowScale().X;
        Camera.Offset = new Vector2(Screen.HCenter, Screen.VCenter+50) * GUI.GetWindowScale();
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                _floor[x,y] = (x%2 != y%2) ? Assets.FloorTiles[0].Instantiate(x,y) : Assets.FloorTiles[1].Instantiate(x,y);
                _board[x,y] = null;
            }
        }
    }
    
    public static void InitializeBattle(Fort leftFort, Fort rightFort, bool leftIsPlayer, bool rightIsPlayer)
    {
        Initialize();
        
        // set up floor tile checkerboard
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                _floor[x,y] = (x%2 != y%2) ? Assets.FloorTiles[0].Instantiate(x,y) : Assets.FloorTiles[1].Instantiate(x,y);
                _board[x,y] = null;
            }
        }
        
        // Load forts to board
        leftFort.LoadToBoard(false);
        rightFort.LoadToBoard(true);
        
        // Tell teams they can generate fear and hate
        LeftTeam.Name = leftFort.Name;
        LeftTeam.IsPlayerControlled = leftIsPlayer;
        LeftTeam.Initialize();
        RightTeam.Name = rightFort.Name;
        RightTeam.IsPlayerControlled = rightIsPlayer;
        RightTeam.Initialize();

        _battleStarted = true;
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
        return _board[tilePos.X,tilePos.Y];
    }
    
    public static Structure? GetTile(int x, int y)
    {
        return _board[x,y];
    }

    public static Team GetOtherTeam(Team team)
    {
        return team == LeftTeam ? RightTeam : LeftTeam;
    }
    
    public static void Update()
    {
        _swUpdate.Restart();
        
        _swUpdateMinionsCollide.Restart();
        foreach (List<Minion> gridSquare in MinionGrid)
        {
            gridSquare.Clear();
        }
        foreach (Minion minion in Minions)
        {
            Int2D pos = PosToTilePos(minion.Position);
            MinionGrid[pos.X, pos.Y].Add(minion);
        }
        _swUpdateMinionsCollide.Stop();
        
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
                    {
                        for (int y = 0; y < BoardHeight; ++y)
                        {
                            _board[x, y]?.WaveEffect();
                        }
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
                    {
                        for (int y = 0; y < BoardHeight; ++y)
                        {
                            _board[x, y]?.PreWaveEffect();
                        }
                    }
                }
            }
        }

        _swUpdateTeams.Restart();
        LeftTeam.Update();
        RightTeam.Update();
        _swUpdateTeams.Stop();
        
        _swUpdatePathfinder.Restart();
        PathFinder.ServeQueue(10);
        _swUpdatePathfinder.Stop();
        
        _swUpdateBoard.Restart();
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _board[x, y]?.Update();
            }
        }
        _swUpdateBoard.Stop();
        
        _swUpdateMinions.Restart();
        for (int index = 0; index < Minions.Count; index++)
        {
            Minions[index].Update();
        }
        _swUpdateMinions.Stop();

        _swUpdateMinionsCollide.Start();
        DoMinionCollision();
        foreach (Minion m in Minions)
        {
            m.CollideTerrain();
            m.LateUpdate();
        }
        _swUpdateMinionsCollide.Stop();

        _swUpdateMinions.Start();
        foreach (Minion m in MinionsToRemove)
        {
            Minions.Remove(m);
            Sprites.Remove(m);
        }
        MinionsToRemove.Clear();
        _swUpdateMinions.Stop();

        _swUpdateProjectiles.Restart();
        for (int index = 0; index < Projectiles.Count; index++)
        {
            Projectile p = Projectiles[index];
            p.Update();
        }

        foreach (Projectile p in ProjectilesToRemove)
        {
            Projectiles.Remove(p);
            Sprites.Remove(p);
        }
        ProjectilesToRemove.Clear();
        _swUpdateProjectiles.Stop();
        
        _swUpdate.Stop();
    }

    private static void DoMinionCollision()
    {
        for (int x = 0; x < BoardWidth; x++) // Iterate MinionGrid Rows
        {
            for (int y = 0; y < BoardHeight; y++) // Iterate MinionGrid Columns
            {
                for (int i = 0; i < MinionGrid[x, y].Count; i++) // Iterate Minions in cell at x,y
                {
                    for (int j = i+1; j < MinionGrid[x, y].Count; j++)
                    {
                        MinionGrid[x, y][i].CollideMinion(MinionGrid[x, y][j]);
                    }

                    if (x < BoardWidth - 1)
                    {
                        if (y > 0)
                        {
                            for (int j = 0; j < MinionGrid[x + 1, y - 1].Count; j++)
                            {
                                MinionGrid[x, y][i].CollideMinion(MinionGrid[x + 1, y - 1][j]);
                            }
                        }

                        for (int j = 0; j < MinionGrid[x + 1, y].Count; j++)
                        {
                            MinionGrid[x, y][i].CollideMinion(MinionGrid[x + 1, y][j]);
                        }

                        if (y < BoardHeight - 1)
                        {
                            for (int j = 0; j < MinionGrid[x + 1, y + 1].Count; j++)
                            {
                                MinionGrid[x, y][i].CollideMinion(MinionGrid[x + 1, y + 1][j]);
                            }
                        }
                    }

                    if (y < BoardHeight - 1)
                    {
                        for (int j = 0; j < MinionGrid[x, y + 1].Count; j++)
                        {
                            MinionGrid[x, y][i].CollideMinion(MinionGrid[x, y + 1][j]);
                        }
                    }
                }
            }
        }
    }

    public static void DrawFloor()
    {
        Raylib.BeginMode2D(Camera);
        
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _floor[x,y].Draw(x*24, y*24);
                // // Hate/fear debug
                //Raylib.DrawCircle((int)pos.X, (int)pos.Y, (int)(LeftTeam.GetHateFor(x,y)/20), Raylib.RED);
                //Raylib.DrawCircle((int)pos.X, (int)pos.Y, (int)LeftTeam.GetFearOf(x,y), Raylib.BLUE);
            }
        }
        
        Raylib.EndMode2D();
    }

    public static void Draw()
    {
        _swDraw.Restart();
        Raylib.BeginMode2D(Camera);

        Sprites = Sprites.OrderBy(o => o.Z).ToList();

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
        // Raylib.BeginMode2D(World.Camera);
        // for (int i = 0; i < World.BoardWidth; i++)
        // {
        //     for (int j = 0; j < World.BoardHeight; j++)
        //     {
        //         if (_nodeGrid[i, j] != null && _nodeGrid[i, j].PrevNode != null)
        //         {
        //             int t = (int)(_nodeGrid[i, j].Weight - _nodeGrid[i, j].PrevNode.Weight);
        //             Color c = new Color(t, 255-t, 0, 200);
        //             Raylib.DrawLineV(World.GetTileCenter(i, j), World.GetTileCenter(_nodeGrid[i, j].PrevNode.Pos), c);
        //         }
        //         else
        //         {
        //             Raylib.DrawCircleV(World.GetTileCenter(i,j), 4, Raylib.RED);
        //         }
        //     }
        // }
        // Raylib.EndMode2D();
        
        // ReSharper disable once InconsistentNaming
        double totalSWTime = _swFrame.Elapsed.TotalSeconds;
        //long totalSWTime = _swMisc.ElapsedMilliseconds + _swFindNext.ElapsedMilliseconds + _swAddNodes.ElapsedMilliseconds + _swNewNode.ElapsedMilliseconds + _swRegisterNode.ElapsedMilliseconds;
        // if (totalSWTime == 0) return;

        // GUI.DrawTextLeft(Screen.HCenter + 350, Screen.VCenter - 250,
        //     $"Avg Pathing Time: {(1000 * _swTotalTime.Elapsed.TotalSeconds / _totalPaths).ToString("N3")}ms\n" +
        //     $"{_totalPaths} paths totalling {_swTotalTime.ElapsedMilliseconds}ms\n" +
        //     $"Average nodes per path: {_totalNodes / _totalPaths}\n" +
        //     $"Time in pathloop: {totalSWTime}ms\n" +
        //     $"Misc: {_swMisc.ElapsedMilliseconds}ms\n" +
        //     $"FindNext: {_swFindNext.ElapsedMilliseconds}ms\n" +
        //     $"AddNodes: {_swAddNodes.ElapsedMilliseconds}ms\n" +
        //     $"NewNode: {_swNewNode.ElapsedMilliseconds}ms\n" +
        //     $"RegisterNode: {_swRegisterNode.ElapsedMilliseconds}ms");
        
        int totalWidth = 1000;
        int x = Screen.HCenter-500;
        
        int width = (int)(totalWidth * _swUpdate.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.Top, width, 10, Color.Red);
        x += width;
        width = (int)(totalWidth * _swDraw.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.Top, width, 10, Color.Green);
        x += width;
        width = totalWidth - x;
        Raylib.DrawRectangle(x, Screen.Top, width, 10, Color.Gray);

        totalSWTime = _swUpdate.Elapsed.TotalSeconds;
        x = Screen.HCenter - 500;
        width = (int)(totalWidth * _swUpdateTeams.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.SkyBlue);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Teams {(int)(100 * _swUpdateTeams.Elapsed.TotalSeconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swUpdatePathfinder.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.Orange);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Path {(int)(100 * _swUpdatePathfinder.Elapsed.TotalSeconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swUpdateBoard.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.Purple);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Board {(int)(100 * _swUpdateBoard.Elapsed.TotalSeconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swUpdateMinions.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.Yellow);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Minions {(int)(100 * _swUpdateMinions.Elapsed.TotalSeconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swUpdateMinionsCollide.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.Red);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Phys {(int)(100 * _swUpdateMinionsCollide.Elapsed.TotalSeconds / totalSWTime)}%, {(_swUpdateMinionsCollide.Elapsed.TotalSeconds * 1000).ToString("N4")}ms");
        x += width;
        width = (int)(totalWidth * _swUpdateProjectiles.Elapsed.TotalSeconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-290, width, 20, Color.Blue);
        GUI.DrawTextLeft(x, Screen.Top+10, $"Proj {(int)(100 * _swUpdateProjectiles.Elapsed.TotalSeconds / totalSWTime)}%");
        x += width;
        width = totalWidth - x;
        Raylib.DrawRectangle(x, Screen.Top+10, width, 20, Color.Gray);
    }

    // Returns a list of all minions in the square of tiles described by the arguments
    public static List<Minion> GetMinionsInRegion(Int2D center, int radius)
    {
        radius--;
        List<Minion> minions = new List<Minion>();
        for (int x = center.X - radius; x < center.X + radius; x++)
        {
            if (x < 0 || x >= BoardWidth) continue;
            for (int y = center.Y - radius; y < center.Y + radius; y++)
            {
                if (y < 0 || y >= BoardHeight) continue;
                foreach (Minion m in MinionGrid[x,y])
                {
                    minions.Add(m);
                }
            }
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

    public static Vector2 GetTileCenter(int x, int y)
    {
        return new Vector2(x * 24 + 12, y * 24 + 20);
    }
    
    public static Vector2 GetTileCenter(Int2D tilePos)
    {
        return GetTileCenter(tilePos.X, tilePos.Y);
    }
}

public record struct Int2D
{
    public static readonly Int2D Zero  = new Int2D(0, 0);
    public static readonly Int2D Up    = new Int2D(0, -1);
    public static readonly Int2D Down  = new Int2D(0, 1);
    public static readonly Int2D Left  = new Int2D(-1, 0);
    public static readonly Int2D Right = new Int2D(1, 0);
    public int X;
    public int Y;

    public Int2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public override String ToString()
    {
        return $"{X},{Y}";
    }
    
    public static Int2D operator +(Int2D a, Int2D b) {
        return new Int2D 
        {
            X = a.X + b.X,
            Y = a.Y + b.Y
        };
    }
}
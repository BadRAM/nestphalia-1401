using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class World
{
    public const int BoardWidth = 48;
    public const int BoardHeight = 22;
    private static FloorTile[,] _floor = new FloorTile[BoardWidth, BoardHeight];
    private static Structure?[,] _board = new Structure?[BoardWidth, BoardHeight];
    public static List<Minion> Minions = new List<Minion>();
    public static List<Minion> MinionsToRemove = new List<Minion>();
    public static List<Projectile> Projectiles = new List<Projectile>();
    public static List<Projectile> ProjectilesToRemove = new List<Projectile>();
    public static List<ISprite> Sprites = new List<ISprite>();
    public static double WaveDuration = 20;
    public static bool PreWave;
    public static double PreWaveOffset = 1;
    public static double FirstWaveTime = 0;
    public static int Wave = 0;
    public static Camera2D Camera;
    public static Team LeftTeam;
    public static Team RightTeam;
    
    private static void Initialize()
    {
        Camera.target = new Vector2(0, 0);
        Camera.offset = new Vector2(0, 0);
        Camera.rotation = 0;
        Camera.zoom = 1;
        Minions.Clear();
        Projectiles.Clear();
        Sprites.Clear();
        FirstWaveTime = Time.Scaled;
        Wave = 0;
        
        LeftTeam = new Team("Player", false, Raylib.BLUE);
        RightTeam = new Team("Enemy", true, Raylib.RED);
    }

    public static void InitializeEditor()
    {
        Initialize();
        
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
    
    public static void InitializeBattle(Fort leftFort, Fort rightFort)
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
        LeftTeam.Initialize();
        RightTeam.Initialize();
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
    }
    
    public static void SetTile(StructureTemplate? tile, Team team, Int2D tilePos)
    {
        SetTile(tile, team, tilePos.X, tilePos.Y);
    }

    public static Structure? GetTile(Int2D tilePos)
    {
        return _board[tilePos.X,tilePos.Y];
    }
    
    public static Structure? GetTile(int x, int y)
    {
        return _board[x,y];
    }
    
    public static void Update()
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
        
        PathFinder.ServeQueue(10);
        
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _board[x, y]?.Update();
            }
        }
        
        Minions = Minions.OrderBy(o=>o.Position.X).ToList();
        for (int index = 0; index < Minions.Count; index++)
        {
            Minions[index].Update();
        }

        for (int i = 0; i < Minions.Count; i++)
        {
            Minions[i].PlanCollision(i);
        }
        foreach (Minion m in Minions)
        {
            m.LateUpdate();
        }
        foreach (Minion m in MinionsToRemove)
        {
            Minions.Remove(m);
            Sprites.Remove(m);
        }
        MinionsToRemove.Clear();
        
        foreach (Projectile p in Projectiles)
        {
            p.Update();
        }
        foreach (Projectile p in ProjectilesToRemove)
        {
            Projectiles.Remove(p);
            Sprites.Remove(p);
        }
        ProjectilesToRemove.Clear();
    }

    public static void Draw()
    {
        Raylib.BeginMode2D(Camera);
        
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _floor[x,y].Draw(x*24, y*24);
                //_board[x,y]?.Draw(x*24, y*24);
                //Vector2 pos = GetTileCenter(x, y);
                //Raylib.DrawCircle((int)pos.X, (int)pos.Y, (int)(LeftTeam.GetHateFor(x,y)/20), Raylib.RED);
                //Raylib.DrawCircle((int)pos.X, (int)pos.Y, (int)LeftTeam.GetFearOf(x,y), Raylib.BLUE);
            }
        }
        
        // foreach (Minion m in Minions)
        // {
        //     m.Draw();
        // }
        //
        // foreach (Projectile p in Projectiles)
        // {
        //     p.Draw();
        // }

        Sprites = Sprites.OrderBy(o => o.Z).ToList();

        foreach (ISprite s in Sprites)
        {
            s.Draw();
        }
        
        Raylib.EndMode2D();
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

    // public static void Flip()
    // {
    //     Structure?[,] boardBuf = new Structure?[BoardWidth, BoardHeight];
    //     for (int x = 0; x < BoardWidth; ++x)
    //     {
    //         for (int y = 0; y < BoardHeight; ++y)
    //         {
    //             boardBuf[x,y] = _board[BoardWidth-(x+1),y]?.Template.Instantiate(x,y);
    //         }
    //     }
    //     Array.Copy(boardBuf, _board, boardBuf.Length);
    // }
}

public enum TeamName
{
    Player,
    Enemy,
    Neutral,
    None
}

public record struct Int2D
{
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
}
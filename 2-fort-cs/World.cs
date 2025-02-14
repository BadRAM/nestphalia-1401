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
    public static double WaveDuration = 20;
    public static bool PreWave;
    public static double PreWaveOffset = 1;
    public static double FirstWaveTime = 0;
    public static int Wave = 0;
    public static Camera2D Camera;
    
    public static void Initialize(bool sandbox)
    {
        Camera.target = new Vector2(0, 0);
        Camera.offset = new Vector2(0, 0);
        Camera.rotation = 0;
        Camera.zoom = 1;
        Minions.Clear();
        Projectiles.Clear();
        FirstWaveTime = Time.Scaled;
        Wave = 0;

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                if (sandbox && x > 21)
                {
                    _floor[x, y] = Assets.FloorTiles[2].Instantiate(x, y);
                }
                else if (sandbox && (x == 0 || x == 21 || y == 0 || y == 21))
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
        
        // for (int i = 0; i < 1; i++)
        // {
        //     Assets.Minions[0].Instantiate(new Vector2(i * 16, 192), TeamName.Neutral);
        // }
        //
        // PathFinder pathFinder = new PathFinder(Minions[0]);
        // pathFinder.FindPath(new Int2D(40, 20));
    }
    
    public static void SetTile(StructureTemplate? tile, int x, int y)
    {
        _board[x,y] = tile?.Instantiate(x, y);
    }
    
    public static void SetTile(StructureTemplate? tile, Int2D tilePos)
    {
        SetTile(tile, tilePos.X, tilePos.Y);
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
        foreach (Minion m in Minions)
        {
            m.Update();
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
        }
        MinionsToRemove.Clear();
        
        foreach (Projectile p in Projectiles)
        {
            p.Update();
        }
        foreach (Projectile p in ProjectilesToRemove)
        {
            Projectiles.Remove(p);
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
                _board[x,y]?.Draw(x*24, y*24);
            }
        }
        
        foreach (Minion m in Minions)
        {
            m.Draw();
        }
        
        foreach (Projectile p in Projectiles)
        {
            p.Draw();
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

    public static void Flip()
    {
        Structure?[,] boardBuf = new Structure?[BoardWidth, BoardHeight];
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                boardBuf[x,y] = _board[BoardWidth-(x+1),y]?.Template.Instantiate(x,y);
            }
        }
        Array.Copy(boardBuf, _board, boardBuf.Length);
    }
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
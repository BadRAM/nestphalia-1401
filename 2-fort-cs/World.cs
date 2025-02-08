using System.Numerics;
using Raylib_cs;

namespace _2_fort_cs;

public static class World
{
    public const int BoardWidth = 48;
    public const int BoardHeight = 22;
    private static Tile[,] _board = new Tile[BoardWidth, BoardHeight];
    public static List<Minion> Minions = new List<Minion>();
    public static List<Minion> MinionsToRemove = new List<Minion>();
    public static List<Projectile> Projectiles = new List<Projectile>();
    public static List<Projectile> ProjectilesToRemove = new List<Projectile>();
    public static float WaveDuration = 10f;
    public static float FirstWaveTime = 0f;
    public static int Wave = 0;

    public static void Initialize()
    {
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                _board[x,y] = (x%2 != y%2) ? Assets.Tiles[0].Instantiate(x,y) : Assets.Tiles[1].Instantiate(x,y);
            }
        }
        
        for (int i = 0; i < 4; i++)
        {
            Assets.Minions[0].Instantiate(new Vector2(i * 16, 192), TeamName.Neutral);
        }
    }
    
    public static void SetTile(TileTemplate tile, Int2D tilePos)
    {
        SetTile(tile, tilePos.X, tilePos.Y);
    }
    
    public static void SetTile(TileTemplate tile, int x, int y)
    {
        if (_board[x,y] != null)
        {
            _board[x,y].Destroy();
        }
        _board[x,y] = tile.Instantiate(x, y);
    }

    public static Tile GetTile(Int2D tilePos)
    {
        return _board[tilePos.X,tilePos.Y];
    }
    
    public static Tile GetTile(int x, int y)
    {
        return _board[x,y];
    }
    
    public static void Update()
    {
        if (Raylib.GetTime() - FirstWaveTime > WaveDuration * Wave)
        {
            Wave++;
            
            for (int x = 0; x < BoardWidth; ++x)
            {
                for (int y = 0; y < BoardHeight; ++y)
                {
                    _board[x, y].WaveEffect();
                }
            }
        }
        
        
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _board[x, y].Update();
            }
        }
        
        foreach (Minion m in Minions)
        {
            m.Update();
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
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                _board[x, y].Draw(x*24, y*24);
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
        
        
    }

    public static Int2D PosToTilePos(Vector2 position)
    {
        int x = (int) position.X      / 24;
        int y = (int)(position.Y - 8) / 24;
        x = Math.Clamp(x, 0, BoardWidth - 1);
        y = Math.Clamp(y, 0, BoardHeight - 1);
        return new Int2D(x, y);
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

    public static Tile GetTileAtPos(Vector2 position)
    {
        return GetTile(PosToTilePos(position));
    }

    public static Vector2 GetTileCenter(int x, int y)
    {
        return new Vector2(x * 24 + 12, y * 24 + 20);
    }

    public static void Flip()
    {
        Tile[,] boardBuf = new Tile[BoardWidth, BoardHeight];
        for (int x = 0; x < BoardWidth; ++x)
        {
            for (int y = 0; y < BoardHeight; ++y)
            {
                boardBuf[x,y] = _board[BoardWidth-(x+1),y];
            }
        }
        Array.Copy(boardBuf, _board, boardBuf.Length);
    }
}

public enum TeamName
{
    Player,
    Enemy,
    Neutral
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
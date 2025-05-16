using System.Numerics;

namespace nestphalia;

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

    public bool EqualsCoords(int x, int y)
    {
        return x == X && y == Y;
    }
}
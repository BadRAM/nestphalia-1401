using System.Numerics;

namespace _2_fort_cs;

public static class Utils
{
    public static Vector2 MoveTowards(this Vector2 start, Vector2 target, double maxDistanceDelta)
    {
        Vector2 delta = target - start;
        if (delta.Length() < maxDistanceDelta)
        {
            return target;
        }
        Vector2 ret = start + delta.Normalized() * (float)maxDistanceDelta;
        return ret;
    }

    public static Vector2 Normalized(this Vector2 vector)
    {
        return Vector2.Normalize(vector);
    }

    public static int WeightedRandom(int max)
    {
        return Math.Min(Random.Shared.Next(max), Random.Shared.Next(max));
    }
}

// Todo: Maybe this will be more performant if it implements IComparable?
public class Sortable<T>
{
    public double Order;
    public T? Value;

    public Sortable(double order, T? value)
    {
        Order = order;
        Value = value;
    }
}
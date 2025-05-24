using System.Numerics;

namespace nestphalia;

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

    public static int WeightedRandom(this Random random, int max)
    {
        return Math.Min(random.Next(max), random.Next(max));
    }
    
    public static void InsertSorted<T>(this List<T> @this, T item) where T: IComparable<T>
    {
        if (@this.Count == 0)
        {
            @this.Add(item);
            return;
        }
        if (@this[@this.Count-1].CompareTo(item) <= 0)
        {
            @this.Add(item);
            return;
        }
        if (@this[0].CompareTo(item) >= 0)
        {
            @this.Insert(0, item);
            return;
        }
        int index = @this.BinarySearch(item);
        if (index < 0) 
            index = ~index;
        @this.Insert(index, item);
    }
}

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
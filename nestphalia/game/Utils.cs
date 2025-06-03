using System.Numerics;

namespace nestphalia;

public static class Utils
{
    public static float MoveTowards(this float start, float target, float maxDistanceDelta)
    {
        if (target - start < maxDistanceDelta)
        {
            return target;
        } 
        return start > target ? start - maxDistanceDelta : start + maxDistanceDelta;
    }
    
    public static double MoveTowards(this double start, double target, double maxDistanceDelta)
    {
        if (target - start < maxDistanceDelta)
        {
            return target;
        } 
        return start > target ? start - maxDistanceDelta : start + maxDistanceDelta;
    }
    
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
    
    public static Vector3 MoveTowards(this Vector3 start, Vector3 target, double maxDistanceDelta)
    {
        Vector3 delta = target - start;
        if (delta.Length() < maxDistanceDelta)
        {
            return target;
        }
        Vector3 ret = start + delta.Normalized() * (float)maxDistanceDelta;
        return ret;
    }
    
    public static Vector3 MoveTowardsXY(this Vector3 start, Vector2 target, double maxDistanceDelta)
    {
        Vector2 delta = target - start.XY();
        if (delta.Length() < maxDistanceDelta)
        {
            return target.XYZ();
        }
        Vector3 ret = (start.XY() + delta.Normalized() * (float)maxDistanceDelta).XYZ(start.Z);
        return ret;
    }
    
    public static Vector3 XYZ(this Vector2 vec)
    {
        return new Vector3(vec.X, vec.Y, 0);
    }
    
    public static Vector3 XYZ(this Vector2 vec, float Z)
    {
        return new Vector3(vec.X, vec.Y, Z);
    }

    public static Vector2 XY(this Vector3 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }

    // Returns the perspective adjusted position for drawing a sprite
    public static Vector2 XYZ2D(this Vector3 vec)
    {
        return new Vector2(vec.X, vec.Y - vec.Z);
    }

    public static Vector2 Normalized(this Vector2 vector)
    {
        return Vector2.Normalize(vector);
    }
    
    public static Vector3 Normalized(this Vector3 vector)
    {
        return Vector3.Normalize(vector);
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
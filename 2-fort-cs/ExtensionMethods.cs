using System.Numerics;

namespace _2_fort_cs;

public static class ExtensionMethods
{
    public static Vector2 MoveTowards(this Vector2 start, Vector2 target, float maxDistanceDelta)
    {
        Vector2 delta = target - start;
        if (delta.Length() < maxDistanceDelta)
        {
            return target;
        }
        Vector2 ret = start + (delta.Normalized() * maxDistanceDelta);
        return ret;
    }
    
    // public static float MoveTowards(this float start, float target, float maxDistanceDelta)
    // {
    //     
    //     if (start > target)
    //     {
    //         
    //     }
    //     if (MathF.Abs(start - target) < maxDistanceDelta)
    //     return 
    // }

    public static Vector2 Normalized(this Vector2 vector)
    {
        return Vector2.Normalize(vector);
    }
}
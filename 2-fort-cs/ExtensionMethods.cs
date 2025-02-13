using System.Numerics;

namespace _2_fort_cs;

public static class ExtensionMethods
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
}
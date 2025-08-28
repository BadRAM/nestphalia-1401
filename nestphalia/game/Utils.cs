using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace nestphalia;

public static class Utils
{
    public static void OpenFolder(string path)
    {
        path = path.MakePathAbsolute();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using Process fileOpener = new Process();
            fileOpener.StartInfo.FileName = "explorer";
            fileOpener.StartInfo.Arguments = path;
            fileOpener.Start();
            return;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                using Process dbusShowItemsProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dbus-send",
                        Arguments = "--print-reply --dest=org.freedesktop.FileManager1 /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://"+ path +"\" string:\"\"",
                        UseShellExecute = true
                    }
                };
                dbusShowItemsProcess.Start();
    
                if (dbusShowItemsProcess.ExitCode == 0)
                {            
                    // The dbus invocation can fail for a variety of reasons:
                    // - dbus is not available
                    // - no programs implement the service,
                    // - ...
                    return;
                }
            }
            catch (Exception e)
            {
                GameConsole.WriteLine(e.ToString());
            }
        }
    }

    public static Vector2 TopLeft(this Rectangle rect) { return rect.Position; }
    public static Vector2 Top(this Rectangle rect) { return new Vector2(rect.X + rect.Width/2, rect.Y) ; }
    public static Vector2 TopRight(this Rectangle rect) { return new Vector2(rect.X + rect.Width, rect.Y) ; }
    public static Vector2 Left(this Rectangle rect) { return new Vector2(rect.X, rect.Y + rect.Height/2) ; }
    public static Vector2 Center(this Rectangle rect) { return new Vector2(rect.X + rect.Width/2, rect.Y + rect.Height/2) ; }
    public static Vector2 Right(this Rectangle rect) { return new Vector2(rect.X + rect.Width, rect.Y + rect.Height/2) ; }
    public static Vector2 BottomLeft(this Rectangle rect) { return new Vector2(rect.X, rect.Y + rect.Height) ; }
    public static Vector2 Bottom(this Rectangle rect) { return new Vector2(rect.X + rect.Width/2, rect.Y + rect.Height) ; }
    public static Vector2 BottomRight(this Rectangle rect) { return new Vector2(rect.X + rect.Width, rect.Y + rect.Height) ; }

    
    public static float MoveTowards(this float start, float target, float maxDistanceDelta)
    {
        if (Math.Abs(target - start) < maxDistanceDelta)
        {
            return target;
        } 
        return (start > target) ? (start - maxDistanceDelta) : (start + maxDistanceDelta);
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
    
    public static string MakePathRelative(this string path)
    {
        if (path.Contains(Directory.GetCurrentDirectory()))
        {
            return path.Substring(Directory.GetCurrentDirectory().Length);
        }
        else if (path.Contains(Resources.Dir))
        {
            return path.Substring(Resources.Dir.Length);
        }
        else
        {
            // GameConsole.WriteLine($"{path} is not absolute or is not inside current directory");
            return path;
        }
    }

    public static string MakePathAbsolute(this string path)
    {
        if (!path.Contains(Directory.GetCurrentDirectory()))
        {
            return Directory.GetCurrentDirectory() + path;
        }
        else if (!path.Contains(Resources.Dir))
        {
            return Resources.Dir + path;
        }
        else
        {
            // GameConsole.WriteLine($"{path} is already absolute");
            return path;
        }
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
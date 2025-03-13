using System.Diagnostics;
using System.Numerics;
using ZeroElectric.Vinculum;

namespace nestphalia;

public static class PathFinder
{
    private static Queue<NavPath> _pathQueue = new Queue<NavPath>();

    private static PathNode?[,] _nodeGrid = new PathNode[World.BoardWidth,World.BoardHeight];
    private static List<PathNode> _nodesToConsider = new List<PathNode>();
    private static int _nodesToConsiderOffset = 0;
    
    private static Stopwatch _swTotalTime = new Stopwatch();
    private static int _totalPaths = 0;
    // private static long _totalNodes = 0;
    // private static Stopwatch _swMisc = new Stopwatch();
    // private static Stopwatch _swFindNext = new Stopwatch();
    // private static Stopwatch _swAddNodes = new Stopwatch();
    // private static Stopwatch _swRegisterNode = new Stopwatch();
    // private static Stopwatch _swNewNode = new Stopwatch();
    
    
    private class PathNode : IComparable<PathNode>
    {
        public Int2D Pos;
        public double Weight;
        //public float TotalWeight;
        public PathNode? PrevNode;
        public bool Set = false;
        
        public PathNode(Int2D pos)
        {
            Pos = pos;
        }

        public int CompareTo(PathNode? other)
        {
            return Weight.CompareTo(other?.Weight);
        }
    }

    public static void RequestPath(NavPath navPath)
    {
        _pathQueue.Enqueue(navPath);
    }

    public static void ServeQueue(int max)
    {
        double startTime = Raylib.GetTime();
        for (int i = 0; i < max; i++)
        {
            if (_pathQueue.Count == 0)  return;
            FindPath(_pathQueue.Dequeue());
            if (i == max-1)
            {
                Console.WriteLine($"Max path calc in {(Raylib.GetTime() - startTime) * 1000}ms, Queue length: {GetQueueLength()}");
            }
        }
    }

    public static void FindPath(NavPath navPath)
    {
        
        // Guards
        if (navPath.Found)
        {
            int c = _pathQueue.Count;
            _pathQueue = new Queue<NavPath>(_pathQueue.Distinct());
            Console.WriteLine($"FindPath called on a path that's already found, something's wrong. Trimmed {c - _pathQueue.Count} duplicate path requests");
            return;
        }
        
        // Start of pathing
        _swTotalTime.Start();
        _totalPaths++;
        
        //PathNode?[,] nodeGrid = new PathNode[World.BoardWidth,World.BoardHeight];
        // LinkedList<PathNode> nodesToConsider = new LinkedList<PathNode>();

        _nodeGrid = new PathNode[World.BoardWidth, World.BoardHeight];
        _nodesToConsider.Clear();
        _nodesToConsiderOffset = 0;
        
        PathNode n = new PathNode(navPath.Start);
        _nodesToConsider.Add(n);
        // nodesToConsider.Add(n);
        
        int count = 0;
        while (true)
        {
            // _swMisc.Start();
            count++;
            
            // Abort if we ran out of options.
            if (_nodesToConsider.Count == 0)
            {
                n = new PathNode(navPath.Start);
                Console.WriteLine("Couldn't find a path!");
                break;
            }
            
            // _swMisc.Stop();
            // _swFindNext.Start();
            
            n = _nodesToConsider[_nodesToConsiderOffset];
            _nodesToConsiderOffset++;
            
            // _swFindNext.Stop();
            // _swMisc.Start();
            
            // Break if we're done
            if (n.Pos == navPath.Destination)
            {
                // _swMisc.Stop();
                break;
            }
            
            // set cheapest node into grid
            _nodeGrid[n.Pos.X,n.Pos.Y] = n;
            
            // _swMisc.Stop();
            // _swAddNodes.Start();
            
            // add new nodes for consideration
            // left
            if (n.Pos.X-1 >= 0)
            {
                AddWeightedNode(n, n.Pos.X-1, n.Pos.Y, 1, navPath.Team, navPath);
            }
            
            // right
            if (n.Pos.X+1 < World.BoardWidth)
            {
                AddWeightedNode(n, n.Pos.X+1, n.Pos.Y, 1, navPath.Team, navPath);
            }
            
            // up
            if (n.Pos.Y-1 >= 0)
            {
                AddWeightedNode(n, n.Pos.X, n.Pos.Y-1, 1, navPath.Team, navPath);
            }
            
            // down
            if (n.Pos.Y+1 < World.BoardHeight)
            {
                AddWeightedNode(n, n.Pos.X, n.Pos.Y+1, 1, navPath.Team, navPath);
            }
            
            // top left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y - 1 > 0 
                && !(World.GetTile(n.Pos.X-1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y-1)?.NavSolid(navPath.Team) ?? false))
            {
                AddWeightedNode(n, n.Pos.X-1, n.Pos.Y-1, 1.4, navPath.Team, navPath);
            }
            
            // top right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y - 1 > 0 
                && !(World.GetTile(n.Pos.X+1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y-1)?.NavSolid(navPath.Team) ?? false))
            {
                AddWeightedNode(n, n.Pos.X+1, n.Pos.Y-1, 1.4, navPath.Team, navPath);
            }
            
            // bottom left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y + 1 < World.BoardHeight 
                && !(World.GetTile(n.Pos.X-1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y+1)?.NavSolid(navPath.Team) ?? false))
            {
                AddWeightedNode(n, n.Pos.X-1, n.Pos.Y+1, 1.4, navPath.Team, navPath);
            }
            
            // bottom right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y + 1 < World.BoardHeight
                && !(World.GetTile(n.Pos.X+1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y+1)?.NavSolid(navPath.Team) ?? false))
            {
                AddWeightedNode(n, n.Pos.X+1, n.Pos.Y+1, 1.4, navPath.Team, navPath);
            }
            
            // _swAddNodes.Stop();
        }
        
        //Console.WriteLine($"Found path in {count} loops, final node weight {n.Weight}");
        
        while (true)
        {
            navPath.Waypoints.Insert(0, n.Pos);
            if (n.PrevNode == null) break;
            n = n.PrevNode;
        }

        navPath.Found = true;
        // _totalNodes += _nodesToConsider.Count;
        
        _swTotalTime.Stop();
    }

    private static void AddWeightedNode(PathNode prevNode, int x, int y, double weight, Team team, NavPath path)
    {
        if (_nodeGrid[x, y]?.Set ?? false) return;
        
        // _swAddNodes.Stop();
        // _swNewNode.Start();
       
        // Create new node
        PathNode n = new PathNode(new Int2D(x, y));
        n.PrevNode = prevNode;
        n.Weight = prevNode.Weight;
        n.Weight += weight;
        // n.Weight += 0.1 * ((Math.Abs(x - path.Destination.X) + Math.Abs(y - path.Destination.Y)) - (Math.Abs(prevNode.Pos.X - path.Destination.X) + Math.Abs(prevNode.Pos.Y - path.Destination.Y)));
        if ((_nodeGrid[x, y]?.Weight ?? Double.MaxValue) < n.Weight)
        {
            // _swNewNode.Stop();
            // _swAddNodes.Start();
            return;
        }
        n.Weight += team.GetFearOf(x, y);
        Structure? structure = World.GetTile(x, y);
        if (!(structure == null || structure is Rubble))
        {
            if (structure is Minefield && structure.Team == team)
            {
                n.Weight += structure.Health;
            }
            else if (structure is HazardSign && structure.Team == team)
            {
                n.Weight += 1000000;
            }
            else
            {
                if (structure.NavSolid(team))
                {
                    n.Weight += structure.Health;
                    if (structure.Team == team) n.Weight += 1000000; //return null;
                }
            }
        }

        // _swNewNode.Stop();
        // _swRegisterNode.Start();
        
        // Register node
        if ((_nodeGrid[x,y]?.Weight ?? Double.MaxValue) > n.Weight)
        {
            _nodeGrid[x, y] = n;
            if (_nodesToConsider.Count == 0)
            {
                _nodesToConsider.Add(n);
                // _swRegisterNode.Stop();
                // _swAddNodes.Start();
                return;
            }
            for(int i = _nodesToConsiderOffset; i < _nodesToConsider.Count; i++)
            {
                if (n.Weight < _nodesToConsider[i].Weight)
                {
                    _nodesToConsider.Insert(i, n);
                    // _swRegisterNode.Stop();
                    // _swAddNodes.Start();
                    return;
                }
            }
            _nodesToConsider.Add(n);
        }
        // _swRegisterNode.Stop();
        // _swAddNodes.Start();
    }

    public static int GetQueueLength()
    {
        return _pathQueue.Count;
    }

    public static void ClearQueue()
    {
        _pathQueue.Clear();
    }

    public static void DrawDebug()
    {
        Raylib.BeginMode2D(World.Camera);
        for (int i = 0; i < World.BoardWidth; i++)
        {
            for (int j = 0; j < World.BoardHeight; j++)
            {
                if (_nodeGrid[i, j] != null && _nodeGrid[i, j].PrevNode != null)
                {
                    int t = (int)(_nodeGrid[i, j].Weight - _nodeGrid[i, j].PrevNode.Weight);
                    Color c = new Color(t, 255-t, 0, 200);
                    Raylib.DrawLineV(World.GetTileCenter(i, j), World.GetTileCenter(_nodeGrid[i, j].PrevNode.Pos), c);
                }
                else
                {
                    Raylib.DrawCircleV(World.GetTileCenter(i,j), 4, Raylib.RED);
                }
            }
        }
        Raylib.EndMode2D();
        
        // ReSharper disable once InconsistentNaming
        long totalSWTime = _swTotalTime.ElapsedMilliseconds;
        //long totalSWTime = _swMisc.ElapsedMilliseconds + _swFindNext.ElapsedMilliseconds + _swAddNodes.ElapsedMilliseconds + _swNewNode.ElapsedMilliseconds + _swRegisterNode.ElapsedMilliseconds;
        if (totalSWTime == 0) return;

        GUI.DrawTextLeft(Screen.HCenter + 350, Screen.VCenter - 250,
            $"Avg Pathing Time: {(1000 * _swTotalTime.Elapsed.TotalSeconds / _totalPaths).ToString("N3")}ms\n" +
            $"{_totalPaths} paths totalling {_swTotalTime.ElapsedMilliseconds}ms\n"); //+
        //     $"Average nodes per path: {_totalNodes / _totalPaths}\n" +
        //     $"Time in pathloop: {totalSWTime}ms\n" +
        //     $"Misc: {_swMisc.ElapsedMilliseconds}ms\n" +
        //     $"FindNext: {_swFindNext.ElapsedMilliseconds}ms\n" +
        //     $"AddNodes: {_swAddNodes.ElapsedMilliseconds}ms\n" +
        //     $"NewNode: {_swNewNode.ElapsedMilliseconds}ms\n" +
        //     $"RegisterNode: {_swRegisterNode.ElapsedMilliseconds}ms");
        //
        // int totalWidth = 1000;
        // int x = Screen.HCenter-500;
        // int width = (int)(totalWidth * _swMisc.ElapsedMilliseconds / totalSWTime);
        //
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.GRAY);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"Misc: {(int)(100 * _swMisc.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        // width = (int)(totalWidth * _swFindNext.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.GREEN);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"FindNext: {(int)(100 * _swFindNext.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        // width = (int)(totalWidth * _swAddNodes.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.SKYBLUE);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"AddNodes: {(int)(100 * _swAddNodes.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        // width = (int)(totalWidth * _swNewNode.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.ORANGE);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"NewNode: {(int)(100 * _swNewNode.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        // width = (int)(totalWidth * _swRegisterNode.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.PURPLE);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"RegisterNode: {(int)(100 * _swRegisterNode.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        //
        // width = totalWidth - x;
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Raylib.GRAY);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"Out of loop: {100 * width / totalWidth}%");
    }
}

public class NavPath
{
    public bool Found = false; 
    public Int2D Start;
    public Int2D Destination;
    public Team Team;
    public List<Int2D> Waypoints = new List<Int2D>();

    public NavPath(Team team)
    {
        Team = team;
    }
    
    public NavPath(Int2D start, Int2D destination, Team team)
    {
        Start = start;
        Destination = destination;
        Team = team;
    }

    public Int2D NextTile(Vector2 position)
    {
        if (Waypoints.Count == 0) { return Destination; }
        if (World.PosToTilePos(position) == Waypoints[0])
        {
            Waypoints.RemoveAt(0);
        }
        if (Waypoints.Count == 0) { return Destination; }

        return Waypoints[0];
    }

    public void Skip()
    {
        if (Waypoints.Count > 0) Waypoints.RemoveAt(0);
    }

    public Int2D? LookAhead(int index)
    {
        if (Waypoints.Count > index)
        {
            return Waypoints[index];
        }
        return null;
    }
    
    public bool TargetReached(Vector2 position)
    {
        // if (Waypoints.Count == 0) return true;
        return World.PosToTilePos(position) == Destination;
    }
    
    public NavPath Clone()
    {
        NavPath p = new NavPath(Start, Destination, Team);
        p.Found = Found;
        p.Waypoints = new List<Int2D>(Waypoints);
        return p;
    }
    
    public void Reset(Vector2 position)
    {
        Found = false;
        Waypoints.Clear();
        Start = World.PosToTilePos(position);
    }
}
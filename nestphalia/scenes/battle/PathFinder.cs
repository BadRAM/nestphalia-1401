using System.Diagnostics;
using Raylib_cs;

namespace nestphalia;

public static class PathFinder
{
    private static Queue<NavPath> _pathQueue = new Queue<NavPath>();

    private static PathNode?[,] _nodeGrid = new PathNode?[World.BoardWidth,World.BoardHeight];
    private static List<PathNode> _nodesToConsider = new List<PathNode>();
    // private static NodeConsiderationPool _nodesToConsider = new NodeConsiderationPool();
    private static int _nodesToConsiderOffset = 0;
    
    private static Stopwatch _swTotalTime = new Stopwatch();
    private static int _totalPaths = 0;
    private static long _totalNodes = 0;
    private static Stopwatch _swMisc = new Stopwatch();
    private static Stopwatch _swFindNext = new Stopwatch();
    private static Stopwatch _swAddNodes = new Stopwatch();
    // private static Stopwatch _swRegisterNode = new Stopwatch();
    // private static Stopwatch _swNewNode = new Stopwatch();
    
    
    private struct PathNode : IComparable<PathNode>
    {
        public Int2D Pos;
        public double Weight;
        public Int2D PrevNode;
        
        public PathNode(Int2D pos, Int2D prevNode)
        {
            Pos = pos;
            PrevNode = prevNode;
        }

        public PathNode GetPrevNode()
        {
            return _nodeGrid[PrevNode.X, PrevNode.Y] ?? this;
        }

        public int CompareTo(PathNode other)
        {
            return Weight.CompareTo(other.Weight);
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
            Console.WriteLine($"FindPath called on a path that's already found, from [{navPath.Start}] to [{navPath.Destination}] Trimmed {c - _pathQueue.Count} duplicate path requests");
            return;
        }
        
        // Start of pathing
        _swTotalTime.Start();
        _totalPaths++;
        
        Array.Clear(_nodeGrid);
        _nodesToConsider.Clear();
        _nodesToConsiderOffset = 0;
        
        PathNode n = new PathNode(navPath.Start, navPath.Start);
        _nodesToConsider.Add(n);
        
        int count = 0;
        while (true)
        {
             _swMisc.Start();
            count++;
            
            // Abort if we ran out of options.
            if (_nodesToConsider.Count == 0)
            // if (_nodesToConsider.IsEmpty())
            {
                n = new PathNode(navPath.Start, navPath.Start);
                Console.WriteLine("Couldn't find a path!");
                break;
            }
            
            _swMisc.Stop();
            _swFindNext.Start();
            
            // pull cheapest node off the top of the consider queue
            n = PopMinNode();
            
            _swFindNext.Stop();
            _swMisc.Start();
            
            // Break if we're done
            if (n.Pos == navPath.Destination)
            {
                _swMisc.Stop();
                break;
            }

            // guard against nodes we've already found the best path to
            if (_nodeGrid[n.Pos.X, n.Pos.Y] != null) continue;
            
            // set node into grid
            _nodeGrid[n.Pos.X,n.Pos.Y] = n;
            
            _swMisc.Stop();
            _swAddNodes.Start();
            
            // add new nodes for consideration
            // left
            if (n.Pos.X-1 >= 0)
            {
                AddWeightedNode(n.Pos, n.Pos.X-1, n.Pos.Y, 1, navPath.Team, navPath);
            }
            
            // right
            if (n.Pos.X+1 < World.BoardWidth)
            {
                AddWeightedNode(n.Pos, n.Pos.X+1, n.Pos.Y, 1, navPath.Team, navPath);
            }
            
            // up
            if (n.Pos.Y-1 >= 0)
            {
                AddWeightedNode(n.Pos, n.Pos.X, n.Pos.Y-1, 1, navPath.Team, navPath);
            }
            
            // down
            if (n.Pos.Y+1 < World.BoardHeight)
            {
                AddWeightedNode(n.Pos, n.Pos.X, n.Pos.Y+1, 1, navPath.Team, navPath);
            }
            
            // top left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y - 1 > 0 
                && !navPath.Team.GetNavSolid(n.Pos.X-1, n.Pos.Y)
                && !navPath.Team.GetNavSolid(n.Pos.X, n.Pos.Y-1))
            {
                AddWeightedNode(n.Pos, n.Pos.X-1, n.Pos.Y-1, 1.4, navPath.Team, navPath);
            }
            
            // top right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y - 1 > 0 
                && !navPath.Team.GetNavSolid(n.Pos.X+1, n.Pos.Y)
                && !navPath.Team.GetNavSolid(n.Pos.X, n.Pos.Y-1))
            {
                AddWeightedNode(n.Pos, n.Pos.X+1, n.Pos.Y-1, 1.4, navPath.Team, navPath);
            }
            
            // bottom left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y + 1 < World.BoardHeight 
                && !navPath.Team.GetNavSolid(n.Pos.X-1, n.Pos.Y)
                && !navPath.Team.GetNavSolid(n.Pos.X, n.Pos.Y+1))
            {
                AddWeightedNode(n.Pos, n.Pos.X-1, n.Pos.Y+1, 1.4, navPath.Team, navPath);
            }
            
            // bottom right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y + 1 < World.BoardHeight
                && !navPath.Team.GetNavSolid(n.Pos.X+1, n.Pos.Y)
                && !navPath.Team.GetNavSolid(n.Pos.X, n.Pos.Y+1))
            {
                AddWeightedNode(n.Pos, n.Pos.X+1, n.Pos.Y+1, 1.4, navPath.Team, navPath);
            }
            
            _swAddNodes.Stop();
        }
        
        //Console.WriteLine($"Found path in {count} loops, final node weight {n.Weight}");
        
        while (true)
        {
            navPath.Waypoints.Insert(0, n.Pos);
            if (n.PrevNode != n.Pos)
            {
                n = n.GetPrevNode();
            }
            else
            {
                break;
            }
        }

        navPath.Found = true;
        _totalNodes += count;
        _totalNodes += _nodesToConsider.Count;
        
        _swTotalTime.Stop();
    }

    private static void AddWeightedNode(Int2D prevNode, int x, int y, double moveDistance, Team team, NavPath path)
    {
        // if (_nodeGrid[x, y]?.Set ?? false) return;
        if (_nodeGrid[x, y] != null) return;
        
        // _swAddNodes.Stop();
        // _swNewNode.Start();
       
        // Create new node
        PathNode n = new PathNode(new Int2D(x, y), prevNode);
        n.Weight = n.GetPrevNode().Weight;
        n.Weight += moveDistance;
        // n.Weight += team.GetFearOf(x, y);
        n.Weight += team.GetTileWeight(x, y);

        // _swNewNode.Stop();
        // _swRegisterNode.Start();
        
        // Register node
        _nodesToConsider.Add(n);
        
        // _swRegisterNode.Stop();
        // _swAddNodes.Start();
    }
    
    
    // Functions which handle the node consideration pool
        
    private static PathNode PopMinNode()
    {
        double min = _nodesToConsider[0].Weight;
        int mindex = 0;
        for (int i = 0; i < _nodesToConsider.Count; i++)
        {
            if (_nodesToConsider[i].Weight < min)
            {
                min = _nodesToConsider[i].Weight;
                mindex = i;
            }
        }

        
        PathNode p = _nodesToConsider[mindex];
        _nodesToConsider[mindex] = _nodesToConsider[^1];
        _nodesToConsider.RemoveAt(_nodesToConsider.Count-1);
        return p;
        
        // _nodesToConsider.
        //
        // PathNode p = _nodesToConsider[^1];
        // _nodesToConsider.RemoveAt(_nodesToConsider.Count-1);
        // return p;
    }

    private static void CullNodes(Int2D pos)
    {
        for (int i = 0; i < _nodesToConsider.Count; i++)
        {
            if (_nodesToConsider[i].Pos == pos)
            {
                _nodesToConsider[i] = _nodesToConsider[^1];
                _nodesToConsider.RemoveAt(_nodesToConsider.Count-1);
            }
        }
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

        double maxWeight = 0;
        for (int i = 0; i < World.BoardWidth; i++)
        {
            for (int j = 0; j < World.BoardHeight; j++)
            {
                if (_nodeGrid[i, j] != null && _nodeGrid[i, j]?.GetPrevNode().Weight > maxWeight)
                {
                    maxWeight = _nodeGrid[i, j]?.GetPrevNode().Weight ?? 0;
                }
            }
        }

        for (int i = 0; i < World.BoardWidth; i++)
        {
            for (int j = 0; j < World.BoardHeight; j++)
            {
                if (_nodeGrid[i, j] != null && _nodeGrid[i, j]?.GetPrevNode() != null)
                {
                    int t = (int)((_nodeGrid[i, j]?.Weight ?? 0) / maxWeight * 255);
                    Color c = t > 255 ? Color.Pink : new Color(t, 255-t, t, 255);
                    Raylib.DrawLineV(World.GetTileCenter(i, j), World.GetTileCenter(_nodeGrid[i, j]?.GetPrevNode().Pos ?? new Int2D(0,0)), c);
                }
                else
                {
                    Raylib.DrawCircleV(World.GetTileCenter(i,j), 4, Color.Red);
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
            $"{_totalPaths} paths totalling {_swTotalTime.ElapsedMilliseconds}ms ({_pathQueue.Count} pending)\n" +
            $"Average nodes per path: {_totalNodes / _totalPaths}\n" +
            $"Time in pathloop: {totalSWTime}ms\n" +
            $"Misc: {_swMisc.ElapsedMilliseconds}ms\n" +
            $"FindNext: {_swFindNext.ElapsedMilliseconds}ms\n" +
            $"AddNodes: {_swAddNodes.ElapsedMilliseconds}ms\n");
            // $"NewNode: {_swNewNode.ElapsedMilliseconds}ms\n" +
            // $"RegisterNode: {_swRegisterNode.ElapsedMilliseconds}ms");
        
        int totalWidth = 1000;
        int x = Screen.HCenter-500;
        int width = (int)(totalWidth * _swMisc.ElapsedMilliseconds / totalSWTime);
        
        Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Gray);
        GUI.DrawTextLeft(x, Screen.VCenter-290, $"Misc: {(int)(100 * _swMisc.ElapsedMilliseconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swFindNext.ElapsedMilliseconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Green);
        GUI.DrawTextLeft(x, Screen.VCenter-290, $"FindNext: {(int)(100 * _swFindNext.ElapsedMilliseconds / totalSWTime)}%");
        x += width;
        width = (int)(totalWidth * _swAddNodes.ElapsedMilliseconds / totalSWTime);
        Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.SkyBlue);
        GUI.DrawTextLeft(x, Screen.VCenter-290, $"AddNodes: {(int)(100 * _swAddNodes.ElapsedMilliseconds / totalSWTime)}%");
        x += width;
        // width = (int)(totalWidth * _swNewNode.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Orange);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"NewNode: {(int)(100 * _swNewNode.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        // width = (int)(totalWidth * _swRegisterNode.ElapsedMilliseconds / totalSWTime);
        // Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Purple);
        // GUI.DrawTextLeft(x, Screen.VCenter-290, $"RegisterNode: {(int)(100 * _swRegisterNode.ElapsedMilliseconds / totalSWTime)}%");
        // x += width;
        
        width = totalWidth - x;
        Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Gray);
        GUI.DrawTextLeft(x, Screen.VCenter-290, $"Out of loop: {100 * width / totalWidth}%");
    }
}
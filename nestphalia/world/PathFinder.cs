using System.Diagnostics;
using Raylib_cs;

namespace nestphalia;

public static class PathFinder
{
    private static List<NavPath> _pathQueue = new List<NavPath>();

    private static PathNode?[,] _nodeGrid = new PathNode?[World.BoardWidth,World.BoardHeight];
    private static List<PathNode> _nodeQueue = new List<PathNode>();
    private static int _nodes;
    private static List<PathNode> _antinodeQueue = new List<PathNode>();
    private static int _antinodes;
    private static bool _anti;
    
    private static Stopwatch _swTotalTime = new Stopwatch();
    private static int _totalPaths = 0;
    
    #if DEBUG
    private static long _totalNodes = 0;
    private static Stopwatch _swMisc = new Stopwatch();
    private static Stopwatch _swFindNext = new Stopwatch();
    private static Stopwatch _swAddNodes = new Stopwatch();
    #endif
    
    private class PathNode
    {
        public Int2D Pos;
        public double Weight;
        public Int2D PrevNode;
        public bool Anti;
        
        public PathNode(Int2D pos, Int2D prevNode)
        {
            Pos = pos;
            PrevNode = prevNode;
        }

        public PathNode GetPrevNode()
        {
            Debug.Assert(_nodeGrid[PrevNode.X, PrevNode.Y] != null);
            return _nodeGrid[PrevNode.X, PrevNode.Y] ?? this;
        }
    }

    public static void RequestPath(NavPath navPath)
    {
        // One thousand guards
        Debug.Assert(!navPath.Found);
        //Debug.Assert(!_pathQueue.Contains(navPath));
        if (_pathQueue.Contains(navPath))
        {
            Console.WriteLine($"{navPath.Requester} double requested a path");
            return;
        }
        //Debug.Assert(navPath.Requester != "Beetle");
        if (navPath.Start == navPath.Destination)
        {
            Console.WriteLine($"{navPath.Requester} requested a zero length path.");
            navPath.Found = true;
            return;
        }
        
        // One function call
        _pathQueue.Add(navPath);
    }

    public static void ServeQueue(int max)
    {
        double startTime = Raylib.GetTime();
        for (int i = 0; i < max; i++)
        {
            if (_pathQueue.Count == 0)  return;
            NavPath p = _pathQueue[0];
            _pathQueue.RemoveAt(0);
            if (p.Found) // Catch queue duplicates
            {
                Console.WriteLine($"{p.Requester} requested pathing on a navPath that's already been found.");
                continue;
            }
            FindPath(p);
            if (i == max-1)
            {
                Console.WriteLine($"Max path calc in {(Raylib.GetTime() - startTime) * 1000}ms, Queue length: {GetQueueLength()}");
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

    // Called to force a path to be found NOW! 
    public static void DemandPath(NavPath navPath)
    {
        if (_pathQueue.Remove(navPath))
        {
            Console.WriteLine($"{navPath.Requester}'s path jumped the queue");
        }
        FindPath(navPath);
    }

    // FindPath is normally called by ServeQueue, but can be called directly if an immediate path is needed.
    private static void FindPath(NavPath navPath)
    {
        // Guard against invalid path request
        // Debug.Assert(!navPath.Found);
        if (navPath.Found)
        {
            Console.WriteLine($"{navPath.Requester} requested pathing on a navPath that's already been found.");
            return;
        }
        
        // Start of pathing
        _swTotalTime.Start();
        _totalPaths++;
        
        Array.Clear(_nodeGrid);
        _nodeQueue.Clear();
        _antinodeQueue.Clear();
        
        PathNode n = new PathNode(navPath.Start, navPath.Start);
        _nodeQueue.Add(n);
        
        n = new PathNode(navPath.Destination, navPath.Destination);
        _antinodeQueue.Add(n);
        
        #if DEBUG
        int count = 0;
        #endif
        // ===== Path Solver Loop =====================================================================================
        while (true)
        {
            #if DEBUG
            _swMisc.Start();
            count++;
            #endif
            
            // Guard against no-path scenarios
            Debug.Assert(_nodeQueue.Count != 0 && _antinodeQueue.Count != 0);

            // ----- Decide if this loop will be node or antinode -------------------------------------------------
            _anti = _nodes > _antinodes;
            
            #if DEBUG
            _swMisc.Stop();
            _swFindNext.Start();
            #endif

            // ----- Select Node ----------------------------------------------------------------------------------
            n = PopMinNode();
            
            #if DEBUG
            _swFindNext.Stop();
            _swMisc.Start();
            #endif
            
            if (_nodeGrid[n.Pos.X, n.Pos.Y] != null)
            {
                // When nodes and antinodes touch, we're done
                if ((_nodeGrid[n.Pos.X, n.Pos.Y]?.Anti ?? _anti) == !_anti)
                {
                    #if DEBUG
                    _swMisc.Stop();
                    #endif
                    break;
                }
                // guard against nodes we've already found the best path to
                continue;
            }
            
            // ----- Process selected node ------------------------------------------------------------------------
            _nodeGrid[n.Pos.X,n.Pos.Y] = n;
            
            if (_anti)
            {
                _antinodes++;
            }
            else
            {
                _nodes++;
            }
            
            #if DEBUG
            _swMisc.Stop();
            _swAddNodes.Start();
            #endif            

            // ----- Add new nodes for consideration --------------------------------------------------------------
            #region  Add Nodes
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
            #endregion
            
            #if DEBUG
            _swAddNodes.Stop();
            #endif
        }
        
        // ===== Navpath Generation ===================================================================================
        
        // Get the node and antinode that touched
        PathNode nn = !_anti ? n : _nodeGrid[n.Pos.X, n.Pos.Y] ?? n;
        PathNode an =  _anti ? n : _nodeGrid[n.Pos.X, n.Pos.Y] ?? n;
        
        // Build the first half by walking the nodes
        while (true)
        {
            navPath.Waypoints.Insert(0, nn.Pos);
            if (nn.PrevNode == nn.Pos) break;
            nn = nn.GetPrevNode();
        }
        
        // Build the second half by walking the antinodes
        while (true)
        {
            navPath.Waypoints.Add(an.Pos);
            if (an.PrevNode == an.Pos) break;
            an = an.GetPrevNode();
        }
        
        // Set the flag to let the minion know we're done
        navPath.Found = true;
        
        #if DEBUG
        _totalNodes += count;
        _totalNodes += _nodeQueue.Count;
        #endif
        
        _swTotalTime.Stop();
    }
    
    private static PathNode PopMinNode()
    {
        List<PathNode> queue = _anti ? _antinodeQueue : _nodeQueue;
        
        double min = queue[0].Weight;
        int mindex = 0;
        for (int i = 1; i < queue.Count; i++)
        {
            if (queue[i].Weight < min)
            {
                min = queue[i].Weight;
                mindex = i;
            }
        }
        
        PathNode p = queue[mindex];
        queue[mindex] = queue[^1];
        queue.RemoveAt(queue.Count-1);
        return p;
    }

    private static void AddWeightedNode(Int2D prevNode, int x, int y, double moveDistance, Team team, NavPath path)
    {
        if (_nodeGrid[x, y] != null) return;
       
        // Create new node
        PathNode n = new PathNode(new Int2D(x, y), prevNode);
        n.Weight = n.GetPrevNode().Weight;
        n.Weight += moveDistance;
        // n.Weight += team.GetFearOf(x, y);
        n.Weight += team.GetTileWeight(x, y);

        n.Anti = _anti;
        // Register node
        if (!_anti)
        {
            _nodeQueue.Add(n);
        }
        else
        {
            _antinodeQueue.Add(n);
        }
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
        if (totalSWTime == 0) return;

        string debugText = "";

        debugText += $"Avg Pathing Time: {(1000 * _swTotalTime.Elapsed.TotalSeconds / _totalPaths).ToString("N3")}ms\n";
        debugText += $"{_totalPaths} paths totalling {_swTotalTime.ElapsedMilliseconds}ms ({_pathQueue.Count} pending)\n";
        
        #if DEBUG
        debugText += $"Average nodes per path: {_totalNodes / _totalPaths}\n";
        debugText += $"Time in pathloop: {totalSWTime}ms\n";
        debugText += $"Misc: {_swMisc.ElapsedMilliseconds}ms\n";
        debugText += $"FindNext: {_swFindNext.ElapsedMilliseconds}ms\n";
        debugText += $"AddNodes: {_swAddNodes.ElapsedMilliseconds}ms\n";
        
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
        
        width = totalWidth - x;
        Raylib.DrawRectangle(x, Screen.VCenter-300, width, 40, Color.Gray);
        GUI.DrawTextLeft(x, Screen.VCenter-290, $"Out of loop: {100 * width / totalWidth}%");
        #endif

        GUI.DrawTextLeft(350, -250, debugText);
    }
}
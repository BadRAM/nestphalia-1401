using System.Diagnostics;
using Raylib_cs;

namespace nestphalia;

// Warning: Pathfinder uses static variables too much, and cannot run on multiple threads as a result. Making Pathfinder not static would fix.
public class PathFinder
{
    private List<NavPath> _pathQueue = new List<NavPath>();

    private PathNode?[,] _nodeGrid = new PathNode?[World.BoardWidth,World.BoardHeight];
    private List<PathNode> _nodeQueue = new List<PathNode>();
    private int _nodes;
    private List<PathNode> _antinodeQueue = new List<PathNode>();
    private int _antinodes;
    private bool _anti;
    
    private Stopwatch _swTotalTime = new Stopwatch();
    private int _totalPaths = 0;
    
    #if DEBUG
    private long _totalNodes = 0;
    private Stopwatch _swMisc = new Stopwatch();
    private Stopwatch _swFindNext = new Stopwatch();
    private Stopwatch _swAddNodes = new Stopwatch();
    #endif
    
    private class PathNode
    {
        public readonly Int2D Pos;
        public readonly Int2D PrevNode;
        public double Weight;
        public bool Anti;
        
        public PathNode(Int2D pos, Int2D prevNode, bool anti = false)
        {
            Pos = pos;
            PrevNode = prevNode;
            Anti = anti;
        }
    }

    // FindPath is normally called by ServeQueue, but can be called directly if an immediate path is needed.
    public void FindPath(NavPath navPath, bool batchMode = false)
    {
        // Guard against invalid path request
        // Debug.Assert(!navPath.Found);
        if (navPath.Found)
        {
            Console.WriteLine($"{navPath.Requester} requested pathing on a navPath that's already been found.");
            return;
        }
        
        _swTotalTime.Start();
        _totalPaths++;

        // ----- Clean up after previous pathing ------------------------------------------------------------------
        // We leave the data behind after pathing for debug visualization, and for batch pathing
        Array.Clear(_nodeGrid);
        _nodeQueue.Clear();
        _nodes = 0;
        _antinodeQueue.Clear();
        _antinodes = 0;
        PathNode n = new PathNode(Int2D.Zero, Int2D.Zero);
        
        // ----- Add start and endpoint nodes ---------------------------------------------------------------------
        _nodeQueue.Add(new PathNode(navPath.Start, navPath.Start));
        if (navPath.Points.Count == 0)
        {
            _antinodeQueue.Add(new PathNode(navPath.Destination, navPath.Destination, true));
        }
        else
        {
            foreach (Int2D navPathPoint in navPath.Points)
            {
                _antinodeQueue.Add(new PathNode(navPathPoint, navPathPoint, true));
            }
            navPath.Points.Clear();
        }

        // ===== Path Solver Loop =====================================================================================
        while (true)
        {
            #if DEBUG
            _swMisc.Start();
            #endif
            
            // Exit if either queue runs out of nodes.
            Debug.Assert(_nodeQueue.Count != 0 && _antinodeQueue.Count != 0);
            // if (_nodeQueue.Count == 0 || _antinodeQueue.Count == 0)
            // {
            //     #if DEBUG
            //     _swMisc.Stop();
            //     #endif
            //     break;
            // }

            // ----- Decide if this loop will be node or antinode -------------------------------------------------
            _anti = _nodes > _antinodes;
            if (_nodeQueue[^1].Weight == 0)     _anti = false;
            if (_antinodeQueue[^1].Weight == 0) _anti = true;
            if (batchMode)                      _anti = true;
            
            #if DEBUG
            _swMisc.Stop();
            _swFindNext.Start();
            #endif

            // ----- Select Node and set it into the nodegrid -----------------------------------------------------
            n = PopMinNode();
            
            #if DEBUG
            _swFindNext.Stop();
            _swMisc.Start();
            _totalNodes++;
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
            
            if (_anti)
            {
                _antinodes++;
            }
            else
            {
                _nodes++;
            }

            // Batchmode stops once the entire board is filled with antinodes
            if (batchMode && _antinodes >= World.BoardHeight * World.BoardWidth)
            {
                #if DEBUG
                _swMisc.Stop();
                #endif
                return; // <----- !!! EARLY RETURN HERE, TO AVOID TOUCHING THE NAVPATH !!!
            }
            
            // ----- Process selected node ------------------------------------------------------------------------
            _nodeGrid[n.Pos.X,n.Pos.Y] = n;
            
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
            if (Raylib.IsKeyDown(KeyboardKey.Q))
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(16, 8, 4, 255));
                World.DrawFloor();
                World.Draw();
                DrawDebug();
                Raylib.EndDrawing();
            }
            #endif
        }
        
        // ===== Navpath Generation ===================================================================================
        
        // Get the node and antinode that touched
        PathNode nn = !_anti ? n : _nodeGrid[n.Pos.X, n.Pos.Y] ?? n;
        PathNode an =  _anti ? n : _nodeGrid[n.Pos.X, n.Pos.Y] ?? n;
        
        // Build the first half by walking the nodes
        while (true)
        {
            navPath.Points.Insert(0, nn.Pos);
            if (nn.PrevNode == nn.Pos) break;
            nn = GetPrevNode(nn);
        }
        
        // Build the second half by walking the antinodes
        while (true)
        {
            navPath.Points.Add(an.Pos);
            if (an.PrevNode == an.Pos) break;
            an = GetPrevNode(an);
        }
        navPath.Destination = an.Pos;
        
        // Set the flag to let the minion know we're done
        navPath.Found = true;
        
        _swTotalTime.Stop();
    }

    // Calculates a flow map based on the first navpath's targets, then samples it to solve all the remaining paths.
    // Warning! All navpaths must have identical destinations!
    public void FindPathsBatched(List<NavPath> navPaths)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        FindPath(navPaths[0], true);

        foreach (NavPath navPath in navPaths)
        {
            PathNode an = _nodeGrid[navPath.Start.X, navPath.Start.Y];
            navPath.Points.Clear();
            
            // Build the path by walking the antinodes
            while (true)
            {
                navPath.Points.Add(an.Pos);
                if (an.PrevNode == an.Pos) break;
                an = GetPrevNode(an);
            }
            navPath.Destination = an.Pos;
        
            // Set the flag to let the minion know we're done
            navPath.Found = true;
        }
        sw.Stop();
        Console.WriteLine($"Batch pathed {navPaths.Count} bugs in {sw.Elapsed.TotalMilliseconds:N3}ms, {(sw.Elapsed.TotalMilliseconds / navPaths.Count):N4}ms/path");
    }
    
    private PathNode PopMinNode()
    {
        List<PathNode> queue = _anti ? _antinodeQueue : _nodeQueue;
        PathNode p = queue[^1];
        queue.RemoveAt(queue.Count-1);
        return p;
    }

    private void AddWeightedNode(Int2D prevNode, int x, int y, double moveDistance, Team team, NavPath path)
    {
        if (_nodeGrid[x, y] != null) return;
       
        // Create new node
        PathNode n = new PathNode(new Int2D(x, y), prevNode);
        n.Weight = GetPrevNode(n).Weight;
        n.Weight += moveDistance;
        n.Weight += team.GetTileWeight(x, y);

        n.Anti = _anti;
        // Register node
        List<PathNode> q = _anti ? _antinodeQueue : _nodeQueue;
        for(int i = q.Count-1; i >= 0; i--)
        {
            if (n.Weight < q[i].Weight)
            {
                q.Insert(i+1, n);
                return;
            }
        }
        q.Insert(0, n);
    }
    
    private PathNode GetPrevNode(PathNode node)
    {
        if (node == null) return null;
        return _nodeGrid[node.PrevNode.X, node.PrevNode.Y];
    }
    
    public void DrawDebug()
    {
        Raylib.BeginMode2D(World.Camera);

        for (int x = 0; x < World.BoardWidth; x++)
        for (int y = 0; y < World.BoardHeight; y++)
        {
            if (_nodeGrid[x, y] != null && GetPrevNode(_nodeGrid[x, y]) != null)
            {
                // int t = (int)((_nodeGrid[i, j]?.Weight ?? 0) / maxWeight * 255);
                Color c = (_nodeGrid[x, y]?.Anti ?? false) ? Color.Blue : Color.Red;
                Raylib.DrawLineEx(World.GetTileCenter(x, y), World.GetTileCenter(GetPrevNode(_nodeGrid[x, y]).Pos), 2, c);
                if (GetPrevNode(_nodeGrid[x, y]) == _nodeGrid[x, y]) Raylib.DrawCircleV(World.GetTileCenter(x,y), 4, c);
            }
            else
            {
                int r = 1;
                foreach (PathNode pathNode in _nodeQueue)
                {
                    if (pathNode.Pos.X == x && pathNode.Pos.Y == y) r++;
                }
                foreach (PathNode pathNode in _antinodeQueue)
                {
                    if (pathNode.Pos.X == x && pathNode.Pos.Y == y) r++;
                }
                Raylib.DrawCircleV(World.GetTileCenter(x,y), r, Color.Green);
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
        int barX = Screen.HCenter-500;
        int width = (int)(totalWidth * _swMisc.ElapsedMilliseconds / totalSWTime);
        
        Raylib.DrawRectangle(barX, Screen.VCenter-300, width, 40, Color.Gray);
        GUI.DrawTextLeft(barX, Screen.VCenter-290, $"Misc: {(int)(100 * _swMisc.ElapsedMilliseconds / totalSWTime)}%");
        barX += width;
        width = (int)(totalWidth * _swFindNext.ElapsedMilliseconds / totalSWTime);
        Raylib.DrawRectangle(barX, Screen.VCenter-300, width, 40, Color.Green);
        GUI.DrawTextLeft(barX, Screen.VCenter-290, $"FindNext: {(int)(100 * _swFindNext.ElapsedMilliseconds / totalSWTime)}%");
        barX += width;
        width = (int)(totalWidth * _swAddNodes.ElapsedMilliseconds / totalSWTime);
        Raylib.DrawRectangle(barX, Screen.VCenter-300, width, 40, Color.SkyBlue);
        GUI.DrawTextLeft(barX, Screen.VCenter-290, $"AddNodes: {(int)(100 * _swAddNodes.ElapsedMilliseconds / totalSWTime)}%");
        barX += width;
        
        width = totalWidth - barX;
        Raylib.DrawRectangle(barX, Screen.VCenter-300, width, 40, Color.Gray);
        GUI.DrawTextLeft(barX, Screen.VCenter-290, $"Out of loop: {100 * width / totalWidth}%");
        #endif

        GUI.DrawTextLeft(350, -250, debugText);
    }
}
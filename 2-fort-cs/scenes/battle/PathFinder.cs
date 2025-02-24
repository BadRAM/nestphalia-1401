using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public static class PathFinder
{
    private static Queue<NavPath> _pathQueue = new Queue<NavPath>();
    
    private class PathNode : IComparable<PathNode>
    {
        public Int2D Pos;
        public double Weight;
        //public float TotalWeight;
        public PathNode? PrevNode;
        
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
                Console.WriteLine($"Max path calc in {(Raylib.GetTime() - startTime) * 1000}ms");
            }
        }
    }
    
    private static void FindPath(NavPath navPath)
    {
        if (navPath.Found)
        {
            int c = _pathQueue.Count;
            _pathQueue = new Queue<NavPath>(_pathQueue.Distinct());
            Console.WriteLine($"FindPath called on a path that's already found, something's wrong. Trimmed {c - _pathQueue.Count} duplicate path requests");
            return;
        }
        
        //Console.WriteLine($"{Minion.Template.Name} pathing from {World.PosToTilePos(Minion.Position)} to {DesiredTarget}");
        
        PathNode?[,] nodeGrid = new PathNode[World.BoardWidth,World.BoardHeight];
        
        List<PathNode> nodesToConsider = new List<PathNode>();
        
        PathNode n = new PathNode(navPath.Start);
        nodesToConsider.Add(n);
        
        int count = 0;
        while (true)
        {
            count++;
            
            // Abort if we ran out of options.
            if (nodesToConsider.Count == 0)
            {
                n = new PathNode(navPath.Start);
                Console.WriteLine("Couldn't find a path!");
                break;
            }
            
            double minWeight = double.MaxValue;
            int mindex = 0;
            for (int i = 0; i < nodesToConsider.Count; i++)
            {
                if (nodesToConsider[i].Weight < minWeight)
                {
                    minWeight = nodesToConsider[i].Weight;
                    mindex = i;
                }
            }
            n = nodesToConsider[mindex];
            nodesToConsider.RemoveAt(mindex);
            
            // Break if we're done
            if (n.Pos == navPath.Destination) break;
            
            // set cheapest node into grid
            nodeGrid[n.Pos.X,n.Pos.Y] = n;
            
            // cull other nodes that point to same tile
            nodesToConsider.RemoveAll(a => a.Pos == n.Pos);
            
            // add new nodes for consideration
            // left
            if (n.Pos.X-1 >= 0 && nodeGrid[n.Pos.X-1,n.Pos.Y] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X-1, n.Pos.Y, 1, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // right
            if (n.Pos.X+1 < World.BoardWidth && nodeGrid[n.Pos.X+1,n.Pos.Y] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X+1, n.Pos.Y, 1, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // up
            if (n.Pos.Y-1 >= 0 && nodeGrid[n.Pos.X,n.Pos.Y-1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X, n.Pos.Y-1, 1, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // down
            if (n.Pos.Y+1 < World.BoardHeight && nodeGrid[n.Pos.X,n.Pos.Y+1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X, n.Pos.Y+1, 1, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // top left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y - 1 > 0 
                && !(World.GetTile(n.Pos.X-1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y-1)?.NavSolid(navPath.Team) ?? false)
                && nodeGrid[n.Pos.X - 1, n.Pos.Y - 1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X-1, n.Pos.Y-1, 1.5f, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // top right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y - 1 > 0 
                && !(World.GetTile(n.Pos.X+1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y-1)?.NavSolid(navPath.Team) ?? false)
                && nodeGrid[n.Pos.X + 1, n.Pos.Y - 1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X+1, n.Pos.Y-1, 1.5f, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // bottom left
            if (   n.Pos.X - 1 > 0 
                && n.Pos.Y + 1 < World.BoardHeight 
                && !(World.GetTile(n.Pos.X-1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y+1)?.NavSolid(navPath.Team) ?? false)
                && nodeGrid[n.Pos.X - 1, n.Pos.Y + 1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X-1, n.Pos.Y+1, 1.5f, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
            
            // bottom right
            if (   n.Pos.X + 1 < World.BoardWidth 
                && n.Pos.Y + 1 < World.BoardHeight
                && !(World.GetTile(n.Pos.X+1, n.Pos.Y)?.NavSolid(navPath.Team) ?? false)
                && !(World.GetTile(n.Pos.X, n.Pos.Y+1)?.NavSolid(navPath.Team) ?? false)
                && nodeGrid[n.Pos.X + 1, n.Pos.Y + 1] == null)
            {
                PathNode? nn = WeightedNode(n, n.Pos.X+1, n.Pos.Y+1, 1.5f, navPath.Team);
                if (nn != null) nodesToConsider.Add(nn);
            }
        }
        
        //Console.WriteLine($"Found path in {count} loops, final node weight {n.Weight}");

        while (true)
        {
            navPath.Waypoints.Insert(0, n.Pos);
            if (n.PrevNode == null) break;
            n = n.PrevNode;
        }

        navPath.Found = true;
    }

    private static PathNode? WeightedNode(PathNode prevNode, int x, int y, double weight, Team team)
    {
        PathNode n = new PathNode(new Int2D(x, y));
        n.PrevNode = prevNode;
        n.Weight = prevNode.Weight;
        n.Weight += weight;
        n.Weight += team.GetFearOf(x, y) * 0.2;
        Structure? structure = World.GetTile(x, y);
        if (structure == null || structure is Rubble) return n;
        if (structure is Minefield && structure.Team == team)
        {
            n.Weight += structure.Health;
            return n;
        }
        else if (structure is HazardSign && structure.Team == team)
        {
            n.Weight += 1000000;
            return n;
        }
        if (!structure.NavSolid(team)) return n;
        n.Weight += structure.Health;
        if (structure.Team == team) n.Weight += 1000000; //return null;
        
        return n;
    }

    public static int GetQueueLength()
    {
        return _pathQueue.Count;
    }

    public static void ClearQueue()
    {
        _pathQueue.Clear();
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
    
    public void Reset()
    {
        Destination = Start;
        Found = false;
        Waypoints.Clear();
    }
}
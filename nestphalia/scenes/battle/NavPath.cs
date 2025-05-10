using System.Numerics;

namespace nestphalia;

public class NavPath
{
    public bool Found; 
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
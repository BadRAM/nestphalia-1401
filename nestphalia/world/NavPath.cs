using System.Numerics;

namespace nestphalia;

public class NavPath
{
    public bool Found = false; 
    public Int2D Start;
    public Int2D Destination;
    public Team Team;
    public string Requester;
    public List<Int2D> Waypoints = new List<Int2D>();

    public NavPath(string requester, Team team)
    {
        Team = team;
        Requester = requester;
    }
    
    public NavPath(string requester, Int2D start, Int2D destination, Team team)
    {
        Start = start;
        Destination = destination;
        Team = team;
        Requester = requester;
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
    
    public bool TargetReached(Vector3 position)
    {
        return World.PosToTilePos(position) == Destination;
    }
    
    public NavPath Clone(string requester)
    {
        NavPath p = new NavPath(requester, Start, Destination, Team);
        p.Found = Found;
        p.Waypoints = new List<Int2D>(Waypoints);
        return p;
    }
    
    public void Reset(Vector3 position)
    {
        Found = false;
        Waypoints.Clear();
        Start = World.PosToTilePos(position);
    }
}
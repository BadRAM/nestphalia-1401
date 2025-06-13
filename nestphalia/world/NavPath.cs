using System.Numerics;

namespace nestphalia;

public class NavPath
{
    public bool Found = false; 
    public Int2D Start;
    public Int2D Destination;
    public Team Team;
    public string Requester;
    // Multi Use!
    //  - When calculating a path, this contains all the valid destinations.
    //  - When following a path, this contains the sequence of points to travel through.
    public List<Int2D> Points = new List<Int2D>();

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
        if (Points.Count == 0) { return Destination; }
        if (World.PosToTilePos(position) == Points[0])
        {
            Points.RemoveAt(0);
        }
        if (Points.Count == 0) { return Destination; }

        return Points[0];
    }

    public void Skip()
    {
        if (Points.Count > 0) Points.RemoveAt(0);
    }

    public Int2D? LookAhead(int index)
    {
        if (Points.Count > index)
        {
            return Points[index];
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
        p.Points = new List<Int2D>(Points);
        return p;
    }
    
    public void Reset(Vector3 position)
    {
        Found = false;
        Points.Clear();
        Start = World.PosToTilePos(position);
    }
}
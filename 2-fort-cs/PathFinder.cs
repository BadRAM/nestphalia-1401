namespace _2_fort_cs;

public class PathFinder
{
    public Int2D DesiredTarget;
    public Minion Minion;
    private List<Int2D> Path = new List<Int2D>();
    
    
    public void FindPath()
    {
        Path.Clear();
        
    }

    public bool TargetReached()
    {
        return World.PosToTilePos(Minion.Position) == DesiredTarget;
    }

    public Int2D NextTile()
    {
        if (World.PosToTilePos(Minion.Position) == Path[0])
        {
            Path.RemoveAt(0);
        }

        return Path[0];
    }
}
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class Team
{
    public string Name;
    private double[,] _fearMap = new double[World.BoardWidth, World.BoardHeight];
    private double[,] _hateMap = new double[World.BoardWidth, World.BoardHeight];
    public bool IsRightSide;
    public Color UnitTint;

    public Team(string name, bool isRightSide, Color unitTint)
    {
        Name = name;
        IsRightSide = isRightSide;
        UnitTint = unitTint;
    }

    public void Initialize()
    {
        for (int x = 0; x < World.BoardWidth; x++)
        {
            for (int y = 0; y < World.BoardHeight; y++)
            {
                Structure? s = World.GetTile(x, y);
                if (s != null && s.Team != this)
                {
                    _hateMap[x, y] = s.Template.BaseHate;
                }
                else
                {
                    _hateMap[x, y] = 0;
                }
                _fearMap[x, y] = 0;
            }
        }
    }
    
    public double GetHateFor(int x, int y)
    {
        return _hateMap[x, y];
    }
    
    public double GetHateFor(Int2D pos)
    {
        return _hateMap[pos.X, pos.Y];
    }

    public void AddHateFor(double hate, int x, int y)
    {
        _hateMap[x, y] += hate;
    }
    
    public double GetFearOf(int x, int y)
    {
        return _fearMap[x, y];
    }

    public void AddFearOf(double fear, int x, int y)
    {
        _fearMap[x, y] += fear;
    }
    
    public void AddFearOf(double fear, Int2D pos)
    {
        _fearMap[pos.X, pos.Y] += fear;
    }
}
using System.Numerics;
using Raylib_cs;

namespace _2_fort_cs;

public class TileTemplate
{
    public string Name;
    public Texture2D Texture;

    public TileTemplate(string name, Texture2D texture)
    {
        Name = name;
        Texture = texture;
    }

    // gotta do it this way so child types can be instantiated the same way as parent
    public virtual Tile Instantiate(int x, int y)
    {
        return new Tile(this, x, y);
    }
}

public class Tile
{
    public TileTemplate Template;
    private int testvar;
    private protected int X;
    private protected int Y;
    private protected Vector2 position;
    
    public Tile(TileTemplate template, int x, int y)
    {
        Template = template;
        X = x;
        Y = y;
        position = new Vector2(x*24+12, y*24+20);
    }
    
    public virtual void Draw(int x, int y)
    {
        Raylib.DrawTexture(Template.Texture, x, y, Color.White);
    }
    
    public virtual bool IsSolid()
    {
        return false;
    }

    public virtual TeamName GetTeam()
    {
        return TeamName.Neutral;
    }
    
    public virtual void Update() { }

    public virtual void WaveEffect() { }

    public virtual void Hurt(float damage) { }
    
    public virtual void Destroy() { }

    public Vector2 GetCenter()
    {
        return position;
    }
}
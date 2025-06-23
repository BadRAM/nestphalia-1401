using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class FloorTileTemplate
{
    public string ID;
    public Texture2D Texture;
    
    public FloorTileTemplate(string id, Texture2D texture)
    {
        ID = id;
        Texture = texture;
    }
    
    public virtual FloorTile Instantiate(int x, int y)
    {
        return new FloorTile(this, x, y);
    }
}

public class FloorTile
{
    public FloorTileTemplate Template;
    
    public FloorTile(FloorTileTemplate template, int x, int y)
    {
        Template = template;
    }
    
    public virtual void Draw(int x, int y)
    {
        Raylib.DrawTexture(Template.Texture, x, y, Color.White);
    }
}
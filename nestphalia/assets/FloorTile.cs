using System.Numerics;
using ZeroElectric.Vinculum;

namespace nestphalia;

public class FloorTileTemplate
{
    public string Name;
    public Texture Texture;
    
    public FloorTileTemplate(string name, Texture texture)
    {
        Name = name;
        Texture = texture;
    }
    
    // gotta do it this way so child types can be instantiated the same way as parent
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
        Raylib.DrawTexture(Template.Texture, x, y, Raylib.WHITE);
    }
}
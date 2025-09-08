using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class FloorTileTemplate : JsonAsset
{
    public Texture2D Texture;
    public Vector2 SubSprite;

    public FloorTileTemplate(JObject jObject) : base(jObject)
    {
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        SubSprite = jObject.Value<JObject>("SubSprite")?.ToObject<Vector2>() ?? Vector2.Zero;
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
        Raylib.DrawTextureRec(Template.Texture, new Rectangle(Template.SubSprite * 24, 24, 24), new Vector2(x,y), Color.White);
    }
}
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
        double maxFear = Math.Max(World.LeftTeam.GetFearOf(x, y), World.RightTeam.GetFearOf(x, y));
        maxFear = Math.Clamp(maxFear / 100, 0, 1);
        Color tint = Raylib.ColorLerp(Color.White, Color.DarkGray, (float)maxFear);
        Raylib.DrawTextureRec(Template.Texture, new Rectangle(Template.SubSprite * 24, 24, 24), new Vector2(x*24,y*24+8), tint);
    }
}
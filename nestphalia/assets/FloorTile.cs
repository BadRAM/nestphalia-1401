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
        // Color tint = Raylib.ColorLerp(Color.White, Color.DarkGray, (float)maxFear);
        float tint = (float)(1 - maxFear / 2);
        // float fadeZone = 1f / 3f;
        // tint = Raylib.ColorAlpha(tint, 
        //     Math.Min(Math.Min(
        //         Math.Clamp(x * fadeZone, 0, 1), 
        //         Math.Clamp(y * fadeZone, 0, 1)), Math.Min(
        //         Math.Clamp((World.BoardWidth - x-1) * fadeZone, 0, 1), 
        //         Math.Clamp((World.BoardHeight - y-1) * fadeZone, 0, 1))));
        // Raylib.DrawTextureRec(Template.Texture, new Rectangle(Template.SubSprite * 24, 24, 24), new Vector2(x*24,y*24+8), tint);

        Rectangle bounds = World.GetTileBounds(x, y);
        Rlgl.SetTexture(Template.Texture.Id);
        Rlgl.Begin(DrawMode.Quads);

        if ((x > World.BoardWidth/2) != (y > World.BoardHeight/2)) // rotate the quad so that diagonal transparencies are consistent
        {
            ColorAt(x, y+1, tint);
            Rlgl.TexCoord2f(0f, 1f);
            Rlgl.Vertex2f(bounds.Left(), bounds.Bottom());
            ColorAt(x+1, y+1, tint);
            Rlgl.TexCoord2f(1f, 1f);
            Rlgl.Vertex2f(bounds.Right(), bounds.Bottom());
            ColorAt(x+1, y, tint);
            Rlgl.TexCoord2f(1f, 0f);
            Rlgl.Vertex2f(bounds.Right(), bounds.Top());
            ColorAt(x, y, tint);
            Rlgl.TexCoord2f(0f, 0f);
            Rlgl.Vertex2f(bounds.Left(), bounds.Top());
        }
        else
        {
            ColorAt(x, y, tint);
            Rlgl.TexCoord2f(0f, 0f);
            Rlgl.Vertex2f(bounds.Left(), bounds.Top());
            ColorAt(x, y+1, tint);
            Rlgl.TexCoord2f(0f, 1f);
            Rlgl.Vertex2f(bounds.Left(), bounds.Bottom());
            ColorAt(x+1, y+1, tint);
            Rlgl.TexCoord2f(1f, 1f);
            Rlgl.Vertex2f(bounds.Right(), bounds.Bottom());
            ColorAt(x+1, y, tint);
            Rlgl.TexCoord2f(1f, 0f);
            Rlgl.Vertex2f(bounds.Right(), bounds.Top());
        }
        
        Rlgl.End();
        Rlgl.SetTexture(0);
    }

    public void ColorAt(int x, int y, float tint)
    {
        if (x == 0 || y == 0 || x == World.BoardWidth || y == World.BoardHeight)
        {
            Rlgl.Color4f(tint,tint,tint,0f);
        }
        else
        {
            Rlgl.Color4f(tint,tint,tint,1f);
        }
    }
}
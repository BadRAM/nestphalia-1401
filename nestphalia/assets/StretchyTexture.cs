using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class StretchyTexture : JsonAsset
{
    public Texture2D Texture;
    public int Top;
    public int Bottom;
    public int Left;
    public int Right;
    
    public StretchyTexture(JObject jObject) : base(jObject)
    {
        Top = jObject.Value<int?>("Top") ?? 0;
        Bottom = jObject.Value<int?>("Bottom") ?? 0;
        Left = jObject.Value<int?>("Left") ?? 0;
        Right = jObject.Value<int?>("Right") ?? 0;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }

    public void Draw()
    {
        
    }
}
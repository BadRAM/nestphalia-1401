using System.Drawing;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace nestphalia;

// This stores an animation or set of animations as a jsonAsset
// Each Imageset can only draw from one sprite, but multiple imagesets can pull from the same sprite/atlas
// Imagesets have a fixed size
public class SpriteSet : JsonAsset
{
    public Vector2 Size;
    public Dictionary<Set, List<Vector2>> Sprites;
    public string Texture;
    [JsonIgnore] private Texture2D _texture;
    
    public enum Set
    {
        Base, // Used for single animation ImageSets
        // Minion related sets:
        Walking,
        Flying,
        Attacking,
        Jumping
    }
    
    public SpriteSet(JObject jObject) : base(jObject)
    {
        Size = jObject.Value<JObject>("Size")?.ToObject<Vector2>() ?? throw new Exception("Spriteset must define a size!");
        Texture = jObject.Value<string>("Texture") ?? "";
        _texture = Resources.GetTextureByName(Texture);
        Sprites = jObject.Value<JObject>("Sprites")?.ToObject<Dictionary<Set, List<Vector2>>>() ?? new Dictionary<Set, List<Vector2>>();
    }

    public void Draw(Set set, int index, Vector2 position, Color? tint = null)
    {
        if (!Sprites.ContainsKey(set) || Sprites[set].Count < index)
        {
            Raylib.DrawTexturePro
            (
                Resources.MissingTexture, 
                new Rectangle(0, 0, Resources.MissingTexture.Height, Resources.MissingTexture.Width), 
                new Rectangle(position, Size),
                Vector2.Zero, 0f,
                Color.White
            );
            return;
        }
        
        Rectangle r = new Rectangle(Sprites[set][index], Size);
        Color t = tint ?? Color.White;
        Raylib.DrawTextureRec(_texture, r, position, t);
    }
}
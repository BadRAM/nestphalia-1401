using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class FloorScatterTexList : JsonAsset
{
    public List<string> Textures;
    
    public FloorScatterTexList(JObject jObject) : base(jObject)
    {
        Textures = jObject.Value<JArray>("Textures")?.ToObject<List<string>>() ?? new List<string>();
    }
}

public class FloorScatter
{
    public string TextureID;
    [JsonIgnore] private Texture2D _texture;
    public Vector2 Position;
    public float Rotation = 0.15f;

    public FloorScatter(string textureId, Vector2 position, float rotation)
    {
        TextureID = textureId;
        _texture = Resources.GetTextureByName(textureId);
        Position = position;
        Rotation = rotation;
    }

    public Rectangle Rect()
    {
        return new Rectangle(Position - _texture.Size()/2, _texture.Size());
    }

    public void Draw()
    {
        Raylib.DrawTexturePro(_texture, _texture.Rect(), new Rectangle(Position, _texture.Size()), _texture.Size()/2, Rotation, Color.White);
    }
}
using Newtonsoft.Json.Linq;

namespace nestphalia;

public abstract class JsonAsset
{
    // ReSharper disable once InconsistentNaming
    public readonly string ID;
    public string Type = ""; // This is only needed for jsonAssets to serialize back to json correctly.
    
    public JsonAsset(JObject jObject)
    {
        ID = jObject.Value<string?>("ID") ?? throw new Exception("Tried to load a jsonAsset with no ID!");
    }
}
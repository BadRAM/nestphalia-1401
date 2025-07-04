using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public abstract class JsonAsset
{
    public string ID;
    
    public JsonAsset(JObject jObject)
    {
        ID = jObject.Value<string?>("ID") ?? throw new Exception("Tried to load a jsonAsset with no ID!");
    }
}
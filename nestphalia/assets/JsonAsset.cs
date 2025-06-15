using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public abstract class JsonAsset
{
    public string ID;
    
    public JsonAsset(JObject jObject)
    {
        if (!jObject.ContainsKey("id"))
        {
            Console.WriteLine("Tried to load a jsonAsset with no id!");
            foreach (KeyValuePair<string,JToken?> keyValuePair in jObject)
            {
                Console.WriteLine($"    {keyValuePair.Key}: {keyValuePair.Value}");
            }
        }
        // Debug.Assert(jObject.ContainsKey("id"));
        ID = jObject.Value<string?>("id") ?? "error_no_id";
    }
}
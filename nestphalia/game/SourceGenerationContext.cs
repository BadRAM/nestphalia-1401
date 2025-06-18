using System.Text.Json.Serialization;

namespace nestphalia;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Fort))]
[JsonSerializable(typeof(CampaignSaveData))]
[JsonSerializable(typeof(SavedSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
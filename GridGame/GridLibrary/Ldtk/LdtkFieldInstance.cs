using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GridLibrary.Ldtk;

public struct LdtkFieldInstance
{
    [JsonPropertyName("__identifier")]
    public string Identifier { get; set; }
    [JsonPropertyName("__type")]
    public string Type { get; set; }
    [JsonPropertyName("__value")]
    public JsonValue Value { get; set; }
}
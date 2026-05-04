using System.Text.Json.Serialization;

namespace GridLibrary.Ldtk;

public struct LdtkCustomData
{
    public int TileId { get; set; }

    /// <summary>
    /// LDTK doesn't have the concept of animated tiles, so I'm using this field
    /// to store source rectangles for animation frames.
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; }
}

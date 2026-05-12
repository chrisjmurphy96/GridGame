using System;
using System.Text.Json.Serialization;

namespace GridLibrary.Ldtk;

/// <summary>
/// It turns out there's more than one type of layer instance.
/// For now I'm just adding all info to this shared class, but
/// in the future I should probably have separate classes based
/// on the "__type" field.
/// </summary>
public struct LdtkLayerInstance
{
    public Guid IID { get; set; }
    [JsonPropertyName("__identifier")]
    public string Identifier { get; set; }
    [JsonPropertyName("__tilesetDefUid")]
    public int? TilesetDefUid { get; set; }
    [JsonPropertyName("__tilesetRelPath")]
    public string? TilesetRelPath { get; set; }
    [JsonPropertyName("__cWid")]
    public int Columns { get; set; }
    [JsonPropertyName("__cHei")]
    public int Rows { get; set; }
    [JsonPropertyName("__gridSize")]
    public int GridSize { get; set; }

    // Tile layer only
    public LdtkGridTile[] GridTiles { get; set; }

    // Entity layer only
    public LdtkEntityInstance[] EntityInstances { get; set; }
}

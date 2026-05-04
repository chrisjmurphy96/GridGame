using System;
using System.Text.Json.Serialization;

namespace GridLibrary.Ldtk;

public struct LdtkLayerInstance
{
    public Guid IID { get; set; }
    [JsonPropertyName("__identifier")]
    public string Name { get; set; }
    [JsonPropertyName("__tilesetDefUid")]
    public int TilesetDefUid { get; set; }
    public LdtkGridTile[] GridTiles { get; set; }
    [JsonPropertyName("__tilesetRelPath")]
    public string TilesetRelPath { get; set; }
    [JsonPropertyName("__cWid")]
    public int Columns { get; set; }
    [JsonPropertyName("__cHei")]
    public int Rows { get; set; }
    [JsonPropertyName("__gridSize")]
    public int GridSize { get; set; }
}

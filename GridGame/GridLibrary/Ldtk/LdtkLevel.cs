using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace GridLibrary.Ldtk;

public struct LdtkLevel
{
    public string Identifier { get; set; }
    public Guid IID { get; set; }
    public LdtkLayerInstance[] LayerInstances { get; set; }
    [JsonPropertyName("pxWid")]
    public int LayerWidth { get; set; }
    [JsonPropertyName("pxHei")]
    public int LayerHeight { get; set; }

    public readonly LdtkLayerInstance GetTileLayer() => LayerInstances.Single(layer => layer.Identifier == "Tiles");
    public readonly LdtkLayerInstance GetEntityLayer() => LayerInstances.Single(layer => layer.Identifier == "Entities");
}

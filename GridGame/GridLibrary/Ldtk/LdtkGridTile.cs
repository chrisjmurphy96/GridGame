using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace GridLibrary.Ldtk;

public struct LdtkGridTile
{
    public int[] PX { get; set; }

    public readonly int ScreenOriginX => PX[0];
    public readonly int ScreenOriginY => PX[1];
    public readonly Point Position => new(ScreenOriginX, ScreenOriginY);

    public int[] Src { get; set; }

    public readonly int TextureOriginX => Src[0];
    public readonly int TextureOriginY => Src[1];

    [JsonPropertyName("t")]
    public int TileId { get; set; }
}
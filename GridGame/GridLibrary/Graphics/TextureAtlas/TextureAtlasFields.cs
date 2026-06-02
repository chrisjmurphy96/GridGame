using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridLibrary.Graphics.TextureAtlas;

internal class TextureAtlasFields
{
    public required string Directory { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, Rectangle> RegionNameToSourceRectangle { get; set; }
}

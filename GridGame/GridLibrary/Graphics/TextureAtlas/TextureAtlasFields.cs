using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridLibrary.Graphics.TextureAtlas;

internal class TextureAtlasFields
{
    public required string Directory { get; set; }
    public required string FileName { get; set; }
    public Dictionary<string, Rectangle> RegionNameToSourceRectangle { get; set; } = [];
    public Dictionary<string, AnimationData> AnimationNameToAnimationData { get; set; } = [];
}

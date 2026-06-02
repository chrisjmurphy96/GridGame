using Microsoft.Xna.Framework;

namespace GridLibrary.Graphics.TextureAtlas;

internal struct AnimationData
{
    public required int DelayInMilliseconds { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required Point[] Sources { get; set; }
}

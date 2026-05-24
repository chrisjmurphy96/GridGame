using Microsoft.Xna.Framework;

namespace GridLibrary.Graphics;

public class Sprite : BaseSprite
{
    /// <summary>
    /// Gets or Sets the source texture region represented by this sprite.
    /// </summary>
    public required TextureRegion TextureRegion { get; init; }

    /// <summary>
    /// Gets the width, in pixels, of this sprite. 
    /// </summary>
    /// <remarks>
    /// Width is calculated by multiplying the width of the source texture region by the x-axis scale factor.
    /// </remarks>
    public override float Width => TextureRegion.Width * Scale.X;

    /// <summary>
    /// Gets the height, in pixels, of this sprite.
    /// </summary>
    /// <remarks>
    /// Height is calculated by multiplying the height of the source texture region by the y-axis scale factor.
    /// </remarks>
    public override float Height => TextureRegion.Height * Scale.Y;

    /// <summary>
    /// Sets the origin of this sprite to the center.
    /// TODO: This might be better off being the default origin? For my sake.
    /// </summary>
    public void CenterOrigin()
    {
        Origin = new Vector2(TextureRegion.Width, TextureRegion.Height) * 0.5f;
    }
}

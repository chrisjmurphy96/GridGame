using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Graphics;

/// <summary>
/// Represents a rectangular region within a texture.
/// </summary>
public class TextureRegion 
{
    /// <summary>
    /// Gets or Sets the source texture this texture region is part of.
    /// The texture is the full image usually containing multiple regions.
    /// </summary>
    public required Texture2D Texture { get; init; }

    /// <summary>
    /// Gets or Sets the source rectangle boundary of this texture region within the source texture.
    /// This is an individual section, e.g. if you have a texture for a font, this would be a single letter.
    /// </summary>
    public required Rectangle SourceRectangle { get; init; }

    /// <summary>
    /// Gets the width, in pixels, of this texture region.
    /// </summary>
    public int Width => SourceRectangle.Width;

    /// <summary>
    /// Gets the height, in pixels, of this texture region.
    /// </summary>
    public int Height => SourceRectangle.Height;

    /// <summary>
    /// Gets the top normalized texture coordinate of this region.
    /// </summary>
    public float TopTextureCoordinate => SourceRectangle.Top / (float)Texture.Height;

    /// <summary>
    /// Gets the bottom normalized texture coordinate of this region.
    /// </summary>
    public float BottomTextureCoordinate => SourceRectangle.Bottom / (float)Texture.Height;

    /// <summary>
    ///  Gets the left normalized texture coordinate of this region.
    /// </summary>
    public float LeftTextureCoordinate => SourceRectangle.Left / (float)Texture.Width;

    /// <summary>
    /// Gets the right normalized texture coordinate of this region.
    /// </summary>
    public float RightTextureCoordinate => SourceRectangle.Right / (float)Texture.Width;
}

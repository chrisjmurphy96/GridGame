using System;
using Microsoft.Xna.Framework;

namespace GridLibrary.Graphics;

public class AnimatedSprite : BaseSprite
{
    /// <summary>
    /// Gets or Sets the animation for this animated sprite.
    /// </summary>
    public required Animation Animation { get; init; }

    public override float Width => CurrentFrame.Width * Scale.X;

    public override float Height => CurrentFrame.Height * Scale.Y;

    private int _currentFrameIndex = 0;
    private TimeSpan _elapsed = TimeSpan.Zero;

    /// <summary>
    /// Active frame of the animated sprite. This should be drawn to the screen.
    /// </summary>
    public TextureRegion CurrentFrame => Animation.CurrentFrame;


    /// <summary>
    /// Sets the origin of this sprite to the center.
    /// TODO: This might be better off being the default origin? For my sake.
    /// </summary>
    public void CenterOrigin()
    {
        Origin = new Vector2(CurrentFrame.Width, CurrentFrame.Height) * 0.5f;
    }

    /// <summary>
    /// Updates this animated sprite.
    /// </summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public void Update(GameTime gameTime)
    {
        Animation.Update(gameTime);
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Graphics;

public class Animation 
{
    /// <summary>
    /// The texture regions that make up the frames of this animation.  The order of the regions within the collection
    /// are the order that the frames should be displayed in.
    /// TODO: sorted list might be neat here? Maybe doesn't make much sense, if we have more than a few frames.
    /// </summary>
    public required IReadOnlyList<TextureRegion> Frames { get; init; } = [];

    /// <summary>
    /// The amount of time to delay between each frame before moving to the next frame for this animation.
    /// </summary>
    public TimeSpan Delay { get; set; } = DefaultDelay;

    public bool Loop { get; set; } = true;
    public bool ReachedLoopEnd { get; private set; } = false;

    /// <summary>
    /// 100 Milliseconds
    /// </summary>
    public static TimeSpan DefaultDelay => TimeSpan.FromMilliseconds(100);

    public int CurrentFrameIndex { get; private set; } = 0;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public TextureRegion CurrentFrame => Frames[CurrentFrameIndex];

    public static Animation FromFrameData(Texture2D atlas, FrameData frameData, int frameWidth, int frameHeight)
    {
        List<TextureRegion> frames = [];
        foreach (FrameSource frameSource in frameData.FrameSources)
        {
            frames.Add(new()
            {
                Texture = atlas,
                SourceRectangle = new Rectangle
                {
                    X = frameSource.X,
                    Y = frameSource.Y,
                    Width = frameWidth,
                    Height = frameHeight
                }
            });
        }
        Animation animation = new()
        {
            Frames = frames,
        };
        if (frameData.DelayInMilliseconds is not null)
            animation.Delay = TimeSpan.FromMilliseconds(frameData.DelayInMilliseconds.Value);
        return animation;
    }

    /// <summary>
    /// Updates this animated sprite.
    /// </summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public void Update(GameTime gameTime)
    {
        if (!Loop && ReachedLoopEnd)
            return;

        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= Delay)
        {
            _elapsed -= Delay;
            CurrentFrameIndex++;

            if (CurrentFrameIndex >= Frames.Count)
            {
                if (Loop)
                    CurrentFrameIndex = 0;
                else
                    CurrentFrameIndex = Frames.Count - 1;
                ReachedLoopEnd = true;
            }
            else
                ReachedLoopEnd = false;
        }
    }

    public void Reset()
    {
        CurrentFrameIndex = 0;
        ReachedLoopEnd = false;
    }
}

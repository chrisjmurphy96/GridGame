using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI;

public class AnimatedElement : UIElement
{
    private Animation? _animation;
    private bool _started = false;
    private Action? _onAnimationEnd;
    private Action? _onFrameReached;
    private bool _onFrameTriggered = false;
    private int _onFrameIndex;

    public AnimatedElement SetAnimation(Animation animation)
    {
        _animation = animation;
        SetTextureNoDefaults(_animation.CurrentFrame);
        return this;
    }

    public AnimatedElement ResetAnimation()
    {
        if (_animation is not null)
        {
            _animation.Reset();
            SetTextureNoDefaults(_animation.CurrentFrame);
        }
        
        return this;
    }

    public AnimatedElement Start()
    {
        _started = true;
        return this;
    }

    public AnimatedElement Stop()
    {
        _started = false;
        return this;
    }

    /// <summary>
    /// Sets a one time trigger for when the animation reaches the last frame.
    /// </summary>
    public AnimatedElement SetOnAnimationEnd(Action onAnimationEnd)
    {
        _onAnimationEnd = onAnimationEnd;
        return this;
    }

    /// <summary>
    /// Sets a one time trigger for when the frame at the index is reached
    /// </summary>
    public AnimatedElement SetFrameTrigger(int frameIndex, Action onFrameReached)
    {
        _onFrameReached = onFrameReached;
        _onFrameTriggered = false;
        _onFrameIndex = frameIndex;
        return this;
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsVisible || !_started)
            return;

        if (_animation is null)
            return;

        if (_onFrameReached is not null &&
            !_onFrameTriggered &&
            _animation.CurrentFrameIndex == _onFrameIndex)
        {
            _onFrameTriggered = true;
            Action? onFrameReached = _onFrameReached;
            _onFrameReached = null;
            if (onFrameReached is not null)
                onFrameReached();
        }
        if (_animation.ReachedLoopEnd)
        {
            Stop();
            Action? onAnimationEnd = _onAnimationEnd;
            _onAnimationEnd = null;
            if (onAnimationEnd is not null)
                onAnimationEnd();
            _animation.Reset();
        }
        else
        {
            _animation.Update(gameTime);
            SetTextureNoDefaults(_animation.CurrentFrame);
        }

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_animation is null)
            throw new ArgumentException($"Please call {SetAnimation} first");

        base.Draw(spriteBatch, parentBounds);
    }
}

using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI;

public class PhaseBanner : UIElement, IRouteableElement
{
    private string _text;
    private SpriteFont _font;
    private TimeSpan? _transition;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public void Initialize()
    {
        _elapsed = TimeSpan.Zero;
    }

    public void AfterInitialize() { }

    public PhaseBanner SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public PhaseBanner SetText(string text)
    {
        _text = text;
        return this;
    }

    public PhaseBanner SetTransitionTimeSpan(TimeSpan transition)
    {
        _transition = transition;
        return this;
    }

    /// <summary>
    /// Override <see cref="HandleInput(GameTime, InputInfo)"/> instead of Update since we only want this to run while
    /// this is focused.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="_"></param>
    /// <exception cref="ArgumentException"></exception>
    public override void HandleInput(GameTime gameTime, InputInfo _)
    {
        if (_transition is null)
            throw new ArgumentException($"Please call {nameof(SetTransitionTimeSpan)} before attempting to update");

        _elapsed += gameTime.ElapsedGameTime;
        if (_elapsed >= _transition)
        {
            Router.RouteWithHistory(DefaultRoutes.Grid);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_font is null)
            throw new ArgumentException($"Please call {nameof(SetFont)} before attempting to draw");
        if (_text is null)
            throw new ArgumentException($"Please call {nameof(SetText)} before attempting to draw");

        base.Draw(spriteBatch, parentBounds);

        Vector2 textSize = _font.MeasureString(_text);
        Vector2 centerOfText = textSize / 2;
        Vector2 centerOfBanner = GetCenter(parentBounds);
        spriteBatch.DrawString(
            _font,
            _text,
            centerOfBanner,
            Color.White,
            rotation: 0,
            origin: centerOfText,
            scale: 1,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);
    }
}

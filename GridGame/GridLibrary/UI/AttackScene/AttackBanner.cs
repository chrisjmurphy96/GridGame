using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI.AttackScene;

public class AttackBanner : UIElement
{
    private IMove? _move;
    private SpriteFont? _font;

    public AttackBanner SetMove(IMove move)
    {
        _move = move;
        return this;
    }

    public AttackBanner SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_font is null)
            throw new ArgumentException($"Please call {nameof(SetFont)} before attempting to draw");
        if (_move is null)
            throw new ArgumentException($"Please call {nameof(SetMove)} before attempting to draw");

        base.Draw(spriteBatch, parentBounds);

        Vector2 textSize = _font.MeasureString(_move.Name);
        Vector2 centerOfText = textSize / 2;
        Vector2 centerOfBanner = GetCenter(parentBounds);
        spriteBatch.DrawString(
            _font,
            _move.Name,
            centerOfBanner,
            Color.Black,
            rotation: 0,
            origin: centerOfText,
            scale: 1,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);
    }
}
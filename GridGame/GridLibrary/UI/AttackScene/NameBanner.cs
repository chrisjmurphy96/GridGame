using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI.AttackScene;

public class NameBanner : UIElement
{
    private string _bannerText = string.Empty;
    private SpriteFont? _font;

    public NameBanner SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public NameBanner SetText(string bannerText)
    {
        _bannerText = bannerText;
        return this;
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_font is null)
            throw new ArgumentException($"Please call {nameof(SetFont)} before attempting to draw");

        base.Draw(spriteBatch, parentBounds);

        Vector2 textSize = _font.MeasureString(_bannerText);
        Vector2 centerOfText = textSize / 2;
        Vector2 centerOfBanner = GetCenter(parentBounds);
        spriteBatch.DrawString(
            _font,
            _bannerText,
            centerOfBanner,
            Color.Black,
            rotation: 0,
            origin: centerOfText,
            scale: 1,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);
    }
}

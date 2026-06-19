using GridLibrary.Graphics;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridLibrary.UI.MapMenu;

public class MapMenu : UIElement, IRouteableElement
{
    private readonly List<IMenuItem> _menuItems = [];
    private TextureRegion? _focusTexture;
    private SpriteFont? _font;

    public MapMenu SetFocusTexture(TextureRegion textureRegion)
    {
        _focusTexture = textureRegion;
        return this;
    }

    public MapMenu SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public void Initialize()
    {
        // TODO: how do I put this next to the cursor?
        // Lots of checks to make if I want to do that.
        // - How does the cursor map to static screen space?
        // - Are we next to an edge and have to change from default positioning?
        _menuItems.Clear();
        _menuItems.Add(new EndTurnMenuItem());
    }

    public void AfterInitialize() { }

    public override void HandleInput(GameTime gameTime, InputInfo inputInfo)
    {
        if (inputInfo.SelectPressed())
        {
            _menuItems.Single().Click();
        }
        else if (inputInfo.CancelPressed())
        {
            Router.Back();
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_focusTexture is null)
            throw new ArgumentException($"Please call {SetFocusTexture} first");
        if (_font is null)
            throw new ArgumentException($"Please call {SetFont} first");

        base.Draw(spriteBatch, parentBounds);

        // Only one item for now, just draw it in the center
        IMenuItem menuItem = _menuItems.Single();
        Vector2 textSize = _font.MeasureString(menuItem.Name);
        Vector2 centerOfText = textSize / 2;
        Vector2 centerOfBanner = GetCenter(parentBounds);
        spriteBatch.DrawString(
            _font,
            menuItem.Name,
            centerOfBanner,
            Color.Black,
            rotation: 0,
            origin: centerOfText,
            scale: 1,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);

        // draw the focus
        Vector2 scale = new()
        {
            X = GetXScale(parentBounds),
            Y = GetYScale(parentBounds)
        };
        Vector2 centerOfFocus = _focusTexture.GetCenter();
        float x = centerOfBanner.X;
        float y = centerOfBanner.Y + centerOfText.Y + 2 * scale.Y;
        Vector2 focusPosition = new(x, y);
        spriteBatch.Draw(
            textureRegion: _focusTexture,
            position: focusPosition,
            Color.White,
            rotation: 0,
            origin: centerOfFocus,
            scale: scale,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);
    }
}

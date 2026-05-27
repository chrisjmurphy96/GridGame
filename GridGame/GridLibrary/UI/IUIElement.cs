using GridLibrary.Graphics;
using GridLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GridLibrary.UI
{
    public interface IUIElement
    {
        IReadOnlyList<IUIElement> Children { get; }
        UIValue Height { get; }
        UIHorizontalPadding HorizontalPadding { get; }
        bool IsFocused { get; }
        bool IsVisible { get; }
        float LayerDepth { get; }
        IUIElement? Parent { get; }
        TextureRegion? Texture { get; }
        UIVerticalPadding VerticalPadding { get; }
        UIValue Width { get; }

        IUIElement AddChild(IUIElement child);
        void Draw(SpriteBatch spriteBatch, Rectangle parentBounds);
        void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo);
        IUIElement PadHorizontal(float value, UIUnit unit, UIHorizontalPaddingOrientation orientation);
        IUIElement PadVertical(float value, UIUnit unit, UIVerticalPaddingOrientation orientation);
        IUIElement RemoveChild(IUIElement child);
        IUIElement SetHeight(int height, UIUnit unit);
        IUIElement SetIsVisible(bool isVisible);
        IUIElement SetLayerDepth(float layerDepth);
        IUIElement SetParent(IUIElement newParent);
        IUIElement SetSpriteEffects(SpriteEffects spriteEffects);
        IUIElement SetTexture(TextureRegion textureRegion);
        IUIElement SetWidth(int width, UIUnit unit);
    }
}
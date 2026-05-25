using System;
using System.Collections.Generic;
using GridLibrary.Graphics;
using GridLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.UI;

/// <summary>
/// To hopefully make setting properties easy, the calls
/// all return a reference to the caller to allow chaining.
/// I do not support an idea of absolute positioning. Sort of.
/// If the element is a child of another element, everything is
/// calculated based on the parents bounds. If you want "global",
/// just don't attach it to a parent (or leave it attached to the root).
/// </summary>
public class UIElement : IUIElement
{
    public IUIElement? Parent { get; private set; }
    public IReadOnlyList<IUIElement> Children => _children;
    private readonly List<IUIElement> _children = [];

    public UIHorizontalPadding HorizontalPadding { get; private set; } = new();
    public UIVerticalPadding VerticalPadding { get; private set; } = new();
    public UIValue Width { get; private set; } = new();
    public UIValue Height { get; private set; } = new();
    public TextureRegion? Texture { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public bool IsFocused => UIRoot.GetFocusedElement() == this;
    public float LayerDepth { get; private set; } = LayerDepths.StaticUI;

    /// <summary>
    /// For convenience this is also setting default Width and Height,
    /// but that might turn out to be unintuitive, AKA a bad idea.
    /// </summary>
    public IUIElement SetTexture(TextureRegion textureRegion)
    {
        Texture = textureRegion;
        Width.Unit = UIUnit.Pixels;
        Width.Value = textureRegion.Width;
        Height.Unit = UIUnit.Pixels;
        Height.Value = textureRegion.Height;
        return this;
    }

    public IUIElement SetParent(IUIElement newParent)
    {
        Parent?.RemoveChild(this);
        newParent.AddChild(this);
        Parent = newParent;
        return this;
    }

    public IUIElement RemoveChild(IUIElement child)
    {
        _children.Remove(child);
        return this;
    }

    public IUIElement AddChild(IUIElement child)
    {
        _children.Add(child);
        return this;
    }

    public IUIElement SetIsVisible(bool isVisible)
    {
        IsVisible = isVisible;
        return this;
    }

    public IUIElement PadHorizontal(float value, UIUnit unit, UIHorizontalPaddingOrientation orientation)
    {
        HorizontalPadding.Value = value;
        HorizontalPadding.Unit = unit;
        HorizontalPadding.Orientation = orientation;
        return this;
    }

    public IUIElement PadVertical(float value, UIUnit unit, UIVerticalPaddingOrientation orientation)
    {
        VerticalPadding.Value = value;
        VerticalPadding.Unit = unit;
        VerticalPadding.Orientation = orientation;
        return this;
    }

    public IUIElement SetWidth(int width, UIUnit unit)
    {
        Width.Value = width;
        Width.Unit = unit;
        return this;
    }

    public IUIElement SetHeight(int height, UIUnit unit)
    {
        Height.Value = height;
        Height.Unit = unit;
        return this;
    }

    public IUIElement SetLayerDepth(float layerDepth)
    {
        LayerDepth = layerDepth;
        return this;
    }

    // public virtual void Update(GameTime gameTime)
    // {
    //     foreach (UIElement child in _children)
    //         child.Update(gameTime);
    // }
    public virtual void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
    {

    }

    public virtual void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (!IsVisible)
            return;

        Vector2 position = new()
        {
            X = GetPositionX(parentBounds),
            Y = GetPositionY(parentBounds)
        };

        if (Texture is not null)
        {
            Vector2 scale = Vector2.One;
            scale.X = GetXScale(parentBounds);
            scale.Y = GetYScale(parentBounds);

            spriteBatch.Draw(
                textureRegion: Texture,
                position: position,
                Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                SpriteEffects.None,
                layerDepth: LayerDepth);
        }

        float widthInPixels = GetWidth(parentBounds);
        float heightInPixels = GetHeight(parentBounds);
        Rectangle currentBounds = new((int)position.X, (int)position.Y, (int)widthInPixels, (int)heightInPixels);

        foreach (IUIElement child in _children)
        {
            if (child.IsVisible)
                child.Draw(spriteBatch, currentBounds);
        }
    }

    protected float GetWidth(Rectangle parentBounds)
    {
        return Width.Unit switch
        {
            UIUnit.Pixels => Width.Value,
            UIUnit.Percentage => parentBounds.Width * (Width.Value / 100),
            _ => throw new NotImplementedException()
        };
    }

    protected float GetHeight(Rectangle parentBounds)
    {
        return Height.Unit switch
        {
            UIUnit.Pixels => Height.Value,
            UIUnit.Percentage => parentBounds.Height * (Height.Value / 100),
            _ => throw new NotImplementedException()
        };
    }

    protected float GetYScale(Rectangle parentBounds)
    {
        if (Texture is null)
            throw new ArgumentException("Cannot compute y scale without a texture");

        return Height.Unit switch
        {
            UIUnit.Pixels => Height.Value / Texture.Height,
            UIUnit.Percentage => (parentBounds.Height * (Height.Value / 100)) / Texture.Height,
            _ => throw new NotImplementedException()
        };
    }

    protected float GetXScale(Rectangle parentBounds)
    {
        if (Texture is null)
            throw new ArgumentException("Cannot compute x scale without a texture");

        return Width.Unit switch
        {
            UIUnit.Pixels => Width.Value / Texture.Width,
            UIUnit.Percentage => (parentBounds.Width * (Width.Value / 100)) / Texture.Width,
            _ => throw new NotImplementedException()
        };
    }

    protected float GetPositionX(Rectangle parentBounds)
    {
        float widthAmount = Width.Unit switch
        {
            UIUnit.Pixels => Width.Value,
            UIUnit.Percentage => parentBounds.Width * (Width.Value / 100),
            _ => throw new NotImplementedException()
        };
        float xAmount = HorizontalPadding.Unit switch
        {
            UIUnit.Pixels => HorizontalPadding.Value,
            UIUnit.Percentage => (parentBounds.Width * (HorizontalPadding.Value / 100)),
            _ => throw new NotImplementedException()
        };
        return HorizontalPadding.Orientation switch
        {
            UIHorizontalPaddingOrientation.FromLeft => parentBounds.X + xAmount,
            UIHorizontalPaddingOrientation.FromRight => (parentBounds.X + parentBounds.Width) - (xAmount + widthAmount),
            _ => throw new NotImplementedException()
        };
    }

    protected float GetPositionY(Rectangle parentBounds)
    {
        float heightAmount = Height.Unit switch
        {
            UIUnit.Pixels => Height.Value,
            UIUnit.Percentage => parentBounds.Height * (Height.Value / 100),
            _ => throw new NotImplementedException()
        };
        float yAmount = VerticalPadding.Unit switch
        {
            UIUnit.Pixels => VerticalPadding.Value,
            UIUnit.Percentage => (parentBounds.Height * (VerticalPadding.Value / 100)),
            _ => throw new NotImplementedException()
        };
        return VerticalPadding.Orientation switch
        {
            UIVerticalPaddingOrientation.FromTop => parentBounds.Y + yAmount,
            UIVerticalPaddingOrientation.FromBottom => (parentBounds.Y + parentBounds.Height) - (yAmount + heightAmount),
            _ => throw new NotImplementedException()
        };
    }
}

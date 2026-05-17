using System.Linq.Expressions;
using GridLibrary.Graphics;
using GridLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.UI;

/// <summary>
/// Focusing is at least manageable, if still a bit...quirky.
/// I think the current weirdness is mostly due to the fact that there's
/// no idea of reverting focus. The only way I could think to do that is
/// a Queue of focus elements, with the first entry always being the current focus.
/// When something loses focus, you could pop the queue and the previous element would
/// gain focus. But there's a lot of caveats that would go with that, for example, what if
/// you focused the same element multiple times?
/// </summary>
public class UIRoot
{
    private readonly GraphicsDevice _graphicsDevice;
    private Viewport _viewport;
    private readonly SpriteBatch _spriteBatch;
    private readonly KeyboardInfo _keyboardInfo;
    private readonly UIElement _root = new();
    private static UIElement? _focusedElement = null;
    /// <summary>
    /// Bit of a janky workaround, but this is to prevent
    /// HandleInput methods from immediately processing
    /// input. For example, if you hit a button causing
    /// an element to focus, and then that focused element
    /// listens for that same keypress, without this it would
    /// immediately trigger, which isn't what we want.
    /// </summary>
    private static bool _justSwitchedFocus = false;

    public UIRoot(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, KeyboardInfo keyboardInfo)
    {
        _graphicsDevice = graphicsDevice;
        Viewport viewport = graphicsDevice.Viewport;
        _viewport = viewport;
        _root
            .SetWidth<UIElement>(viewport.Width, UIUnit.Pixels)
            .SetHeight<UIElement>(viewport.Height, UIUnit.Pixels);
        _spriteBatch = spriteBatch;
        _keyboardInfo = keyboardInfo;
    }

    /// <summary>
    /// Currently:
    /// - Handles screen resizing
    /// - Calls HandleInput on the focused element, and ONLY the focused element.
    ///   This ensures we don't have awkward chains where multiple elements
    ///   can respond to the same input.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        Viewport viewport = _graphicsDevice.Viewport;
        if (_viewport.Bounds != viewport.Bounds)
        {
            _viewport = viewport;
            _root
                .SetWidth<UIElement>(viewport.Width, UIUnit.Pixels)
                .SetHeight<UIElement>(viewport.Height, UIUnit.Pixels);
        }
        if (_justSwitchedFocus)
        {
            _justSwitchedFocus = false;
        }
        else
            _focusedElement?.HandleInput(_keyboardInfo);

        // foreach (UIElement child in _root.Children)
        //     child.Update(gameTime);
    }

    /// <summary>
    /// By default width and height will be set the TextureRegion's width and height.
    /// </summary>
    public UIElement Create<T>(TextureRegion textureRegion) where T : UIElement, new()
    {
        T element = new T()
            .SetTexture<T>(textureRegion)
            .SetParent<T>(_root);
        return element;
    }

    public T AddToRoot<T>(T element) where T : UIElement
    {
        element.SetParent<T>(_root);
        return element;
    }

    public static void Focus(UIElement element)
    {
        if (_focusedElement is not null)
            _focusedElement.IsFocused = false;

        element.IsFocused = true;
        _focusedElement = element;
        _justSwitchedFocus = true;
    }

    public static void Unfocus()
    {
        if (_focusedElement is not null)
            _focusedElement.IsFocused = false;
        _focusedElement = null;
    }

    public void Draw()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        foreach(UIElement child in _root.Children)
        {
            if (child.IsVisible)
                child.Draw(_spriteBatch, _viewport.Bounds);
        }

        _spriteBatch.End();
    }
}

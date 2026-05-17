namespace GridLibrary.UI;

public interface IUIElement
{
    public bool IsVisible { get; }
    public bool HasFocus { get; }
}
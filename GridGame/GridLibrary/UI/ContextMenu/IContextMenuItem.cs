namespace GridLibrary.UI.ContextMenu;

public interface IContextMenuItem
{
    public string Name { get; }
    public void Click();
}

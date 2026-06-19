using GridLibrary.UI;

namespace GridLibrary.Routing;

public interface IRouteableElement : IUIElement
{
    /// <summary>
    /// Anything that should be run every time the component is navigated to goes here.
    /// </summary>
    public void Initialize();
    /// <summary>
    /// For handling anything like Route.ClearHistory() that should be called after Initialize().
    /// </summary>
    public void AfterInitialize();
}

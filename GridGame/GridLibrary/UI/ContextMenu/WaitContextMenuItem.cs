using GridLibrary.Routing;

namespace GridLibrary.UI.ContextMenu;

public class WaitContextMenuItem : IContextMenuItem
{
    public string Name => "Wait";
    public void Click()
    {
        Router.RouteTo(DefaultRoutes.Grid);
    }
}

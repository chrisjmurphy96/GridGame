using GridLibrary.Grid;
using GridLibrary.Routing;

namespace GridLibrary.UI.ContextMenu;

public class WaitContextMenuItem : IMenuItem
{
    public string Name => "Wait";
    public void Click()
    {
        if (GridState.Instance.ActiveEntity is not null)
            GridState.Instance.ActiveEntity.Value.entity.HasMoved = true;
        GridState.UnsetActiveEntity();
        Router.RouteWithoutHistory(DefaultRoutes.Grid);
    }
}

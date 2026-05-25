using GridLibrary.Grid;
using GridLibrary.Routing;

namespace GridLibrary.UI.ContextMenu;

public class WaitContextMenuItem : IContextMenuItem
{
    public string Name => "Wait";
    public void Click()
    {
        if (GridState.Instance.ActiveEntity is not null)
            GridState.Instance.ActiveEntity.Value.entity.HasMoved = true;
        Router.RouteTo(DefaultRoutes.Grid);
    }
}

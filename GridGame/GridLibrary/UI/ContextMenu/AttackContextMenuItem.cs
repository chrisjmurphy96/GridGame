using GridLibrary.Routing;

namespace GridLibrary.UI.ContextMenu;

public class AttackContextMenuItem : IMenuItem
{
    public string Name => "Attack";
    public void Click()
    {
        Router.RouteWithHistory(DefaultRoutes.MovePreview);
    }
}

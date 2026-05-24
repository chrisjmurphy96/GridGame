using GridLibrary.Routing;

namespace GridLibrary.UI.ContextMenu;

public class AttackContextMenuItem : IContextMenuItem
{
    public string Name => "Attack";
    public void Click()
    {
        Router.RouteTo(DefaultRoutes.MovePreview);
    }
}

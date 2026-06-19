using GridLibrary.Grid;
using GridLibrary.Routing;

namespace GridLibrary.UI.MapMenu;

public class EndTurnMenuItem : IMenuItem
{
    public string Name => "End Turn";

    public void Click()
    {
        GridState.Instance.Phase = Phase.Enemy;
        Router.RouteWithHistory(DefaultRoutes.EnemyPhaseBanner);
    }
}

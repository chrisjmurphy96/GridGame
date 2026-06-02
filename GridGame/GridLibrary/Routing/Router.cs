using System.Collections.Generic;
using GridLibrary.UI;
using GridLibrary.Grid;
using GridLibrary.UI.ContextMenu;

namespace GridLibrary.Routing;

/// <summary>
/// Since I haven't come up with a better solution for generic routing from a library...
/// PLEASE call AddDefaultRoutes.
/// </summary>
public static class Router
{
    private static readonly Dictionary<string, IRouteableElement> _routeNameToElement = [];
    private static IRouteableElement? _currentRoutElement;
    public static string CurrentRoute { get; private set; } = string.Empty;
    public static void RegisterRoute(string routeName, IRouteableElement element) => _routeNameToElement.Add(routeName, element);
    public static void RouteTo(string routeName)
    {
        if (_routeNameToElement.Count is 0)
            throw new KeyNotFoundException("Did you forget to initialize the routes?");

        if (_routeNameToElement.TryGetValue(routeName, out IRouteableElement? element) && element is not null)
        {
            _currentRoutElement?.SetIsVisible(false);

            element.Initialize();
            element.SetIsVisible(true);
            UIRoot.Focus(element);
            _currentRoutElement = element;
            CurrentRoute = routeName;
        }
        else
            throw new KeyNotFoundException($"No route registered for {routeName}");
    }

    /// <summary>
    /// Adds standard routes needed for the tactics game engine to work.
    /// </summary>
    public static void AddDefaultRoutes(TileGrid grid, MovementArrow movementArrow, ContextMenu contextMenu, MovePreview movePreview)
    {
        RegisterRoute(DefaultRoutes.Grid, grid);
        RegisterRoute(DefaultRoutes.MovementArrow, movementArrow);
        RegisterRoute(DefaultRoutes.ContextMenu, contextMenu);
        RegisterRoute(DefaultRoutes.MovePreview, movePreview);
    }
}

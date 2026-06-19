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
    private static IRouteableElement? _currentRouteElement;
    public static string CurrentRoute { get; private set; } = string.Empty;
    private static readonly Stack<string> _routeHistory = [];
    public static void RegisterRoute(string routeName, IRouteableElement element) => _routeNameToElement.Add(routeName, element);
    public static void RouteWithHistory(string routeName)
    {
        if (_routeNameToElement.Count is 0)
            throw new KeyNotFoundException("Did you forget to initialize the routes?");

        if (_routeNameToElement.TryGetValue(routeName, out IRouteableElement? element) && element is not null)
        {
            _currentRouteElement?.SetIsVisible(false);

            element.Initialize();
            element.SetIsVisible(true);
            UIRoot.Focus(element);
            _currentRouteElement = element;
            CurrentRoute = routeName;
            _routeHistory.Push(routeName);
            element.AfterInitialize();
        }
        else
            throw new KeyNotFoundException($"No route registered for {routeName}");
    }

    public static void Back()
    {
        if (_routeHistory.Count > 1)
        {
            _routeHistory.Pop();
            if (_routeHistory.TryPop(out string? previousRoute) && !string.IsNullOrWhiteSpace(previousRoute))
                RouteWithoutHistory(previousRoute);
            // Make sure we add back the current route, which might be updated from the RouteWithoutHistory above
            _routeHistory.Push(CurrentRoute);
        }
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

    public static void RouteWithoutHistory(string routeName)
    {
        if (_routeNameToElement.Count is 0)
            throw new KeyNotFoundException("Did you forget to initialize the routes?");

        if (_routeNameToElement.TryGetValue(routeName, out IRouteableElement? element) && element is not null)
        {
            _currentRouteElement?.SetIsVisible(false);

            element.Initialize();
            element.SetIsVisible(true);
            UIRoot.Focus(element);
            _currentRouteElement = element;
            CurrentRoute = routeName;
            element.AfterInitialize();
        }
        else
            throw new KeyNotFoundException($"No route registered for {routeName}");
    }

    public static void ClearHistory()
    {
        _routeHistory.Clear();
        // Add back the current route
        _routeHistory.Push(CurrentRoute);
    }
}

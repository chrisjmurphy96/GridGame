using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;

namespace GridLibrary.Graphics;

/// <summary>
/// TODO: need a way to bound movement. e.g. a unit in fire emblem can move X squares.
/// <summary>
public class MovementArrow<T> where T : struct, Enum
{
    public Point StartPosition { get; private set; } = Point.Zero;
    public Point EndPosition { get; private set; } = Point.Zero;
    public List<Point> Path { get; private set; } = [];
    private GridTile<T>[] _gridTiles = [];
    private int _rows = 0;
    private int _maxMovement = 0;
    private int _columns = 0;
    private Point? _previousEnd = null;

    public required TextureRegion HeadTexture { get; init; }
    public required TextureRegion StraightTexture { get; init; }
    public required TextureRegion BendTexture { get; init; }
    public required TextureRegion StartTexture { get; init; }

    public int Step { get; set; } = 64;
    public bool IsVisible { get; private set; } = false;

    public void Start(int columns, int rows, int maxMovement, Point start, GridTile<T>[] gridTiles)
    {
        // If the cursor is currently in an unwalkable area, don't start the path.
        int startIndex = GetIndex(_columns, column: start.X, row: start.Y);
        BaseGridTile startTile = gridTiles[startIndex];
        if (!startTile.GetTileInfo().CanWalk)
            return;

        StartPosition = start;
        EndPosition = start;
        IsVisible = true;
        Path = [start];
        _gridTiles = gridTiles;
        _columns = columns;
        _rows = rows;
        _maxMovement = maxMovement;
    }

    public void Cancel()
    {
        StartPosition = Point.Zero;
        EndPosition = Point.Zero;
        IsVisible = false;
        Path = [];
        _previousEnd = null;
        _maxMovement = 0;
    }

    public void Update(Point end)
    {
        if (IsVisible)
            Search(end);
        _previousEnd = end;
    }

    /// <summary>
    /// I tried to build this according to A*, but it's possible this is now
    /// something else entirely. Maybe it's more like Dijkstra's since I keep
    /// a history of nodes to start from. Maybe it's a hybrid since I only do that
    /// under a specific case.
    /// TODO: The path should keep current nodes, UNLESS:
    /// - The path is disconnected, such as by moving to a non-walkable tile
    /// - Fix diagonal movement when two directions are held. We don't get a connected path.
    /// </summary>
    public void Search(Point end)
    {
        if (StartPosition == end)
            Path = [StartPosition];

        // If the cursor is currently in an unwalkable area, skip updating the path.
        int endIndex = GetIndex(_columns, column: end.X, row: end.Y);
        BaseGridTile endTile = _gridTiles[endIndex];
        if (!endTile.GetTileInfo().CanWalk)
            return;
        
        // skip updating if we're at max movement
        if (GetDistance(StartPosition, end) > _maxMovement)
            return;

        // if we haven't moved, skip updating
        if (Path.Last() == end)
            return;

        // If the new end is closer to start, throw out the existing path
        // and generate a new one. This allows us to keep an accurate path
        // but only re-generate when absolutely needed.
        if (_previousEnd is not null &&
            GetDistance(StartPosition, _previousEnd ?? StartPosition) > GetDistance(StartPosition, end))
        {
            Path = GenerateShortestPath(end);
        }

        Path.Add(end);
    }

    private List<Point> GenerateShortestPath(Point end)
    {
        Point next = StartPosition;
        List<Point> path = [next];
        while (next != end)
        {
            // Up, down, left, right
            Point up = new()
            {
                X = next.X,
                Y = next.Y - 1
            };
            Point down = new()
            {
                X = next.X,
                Y = next.Y + 1
            };
            Point left = new()
            {
                X = next.X - 1,
                Y = next.Y
            };
            Point right = new()
            {
                X = next.X + 1,
                Y = next.Y
            };
            List<Point> nextPoints = [up, down, left, right];
            List<(Point next, int distance)> nextPointsWithDistance = [];
            foreach (Point potentialNext in nextPoints)
            {
                if (!InBounds(_columns, _rows, potentialNext))
                    continue;
                int gridIndex = GetIndex(_columns, column: potentialNext.X, row: potentialNext.Y);
                BaseGridTile gridTile = _gridTiles[gridIndex];
                if (gridTile.GetTileInfo().CanWalk)
                    nextPointsWithDistance.Add((potentialNext, GetDistance(potentialNext, end)));
            }

            if (nextPointsWithDistance.Count > 0)
            {
                next = nextPointsWithDistance.OrderBy(nextPoint => nextPoint.distance).First().next;
                path.Add(next);
            }
        }

        return path;
    }

    /// <summary>
    /// With grid based movement there is no "diagonal", so you can
    /// simply add the x and y difference.
    /// </summary>
    public static int GetDistance(Point start, Point end)
    {
        int x = Math.Abs(start.X - end.X);
        int y = Math.Abs(start.Y - end.Y);
        return x + y;
    }

    private static int GetIndex(int columns, int column, int row) => (row * columns) + column;

    private static bool InBounds(int columns, int rows, Point point) => 
        point.Y >= 0 && point.Y < rows && point.X >= 0 && point.X < columns;
}
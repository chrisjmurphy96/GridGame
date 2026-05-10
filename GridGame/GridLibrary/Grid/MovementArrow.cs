using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

/// <summary>
/// TODO: need a way to bound movement. e.g. a unit in fire emblem can move X squares.
/// <summary>
public class MovementArrow<T> where T : struct, Enum
{
    public Point StartPosition { get; private set; } = Point.Zero;
    public Point EndPosition { get; private set; } = Point.Zero;
    public List<Point> Path { get; private set; } = [];
    private GridTileList<T> _gridTiles = new (0);
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

    public void Start(int columns, int rows, int maxMovement, Point start, GridTileList<T> gridTiles)
    {
        // If the cursor is currently in an unwalkable area, don't start the path.
        GridTile<T> startTile = gridTiles[start];
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
    
    public void Search(Point end)
    {
        if (StartPosition == end)
            Path = [StartPosition];

        GridTile<T> endTile = _gridTiles[end];
        if (!endTile.GetTileInfo().CanWalk)
            return;
        
        // skip updating if we're past max range
        if (StartPosition.DistanceTo(end) > _maxMovement)
            return;

        // if we haven't moved, skip updating
        if (Path.Last() == end)
            return;

        Path = Dijkstra.Search<T>(StartPosition, end, _maxMovement, _gridTiles) ?? Path;
    }
}

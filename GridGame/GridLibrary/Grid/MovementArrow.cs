using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class MovementArrow<T> where T : struct, Enum
{
    public Point StartPosition { get; private set; } = Point.Zero;
    public Point EndPosition { get; private set; } = Point.Zero;
    public List<Point> Path { get; private set; } = [];
    private GridTileList<T> _gridTiles = new (0);
    private int _maxMovement = 0;
    private HashSet<Point> _walkableSpace = [];
    private Point? _previousEnd = null;

    public required TextureRegion HeadTexture { get; init; }
    public required TextureRegion StraightTexture { get; init; }
    public required TextureRegion BendTexture { get; init; }
    public required TextureRegion StartTexture { get; init; }

    public bool IsVisible { get; private set; } = false;

    public void Start(int maxMovement, Point start, GridTileList<T> gridTiles, HashSet<Point> walkableSpace)
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
        _maxMovement = maxMovement;
        _walkableSpace = walkableSpace;
    }

    public void Cancel()
    {
        StartPosition = Point.Zero;
        EndPosition = Point.Zero;
        IsVisible = false;
        Path = [];
        _previousEnd = null;
        _maxMovement = 0;
        _walkableSpace = [];
    }

    public void Update(Point end, HashSet<Point> walkableSpace)
    {
        if (IsVisible)
            Search(end, walkableSpace);
        _previousEnd = end;
    }
    
    private void Search(Point end, HashSet<Point> walkableSpace)
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

        Path = Dijkstra.Search<T>(StartPosition, end, _maxMovement, walkableSpace) ?? Path;
    }
}

using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

/// <summary>
/// Gets positions for "blue" grid of walkable tiles.
/// Gets positions for "red" tiles indicating attack range.
/// </summary>
public class MoveOverlay<T> where T : struct, Enum
{
    public required TextureRegion MovementTexture { get; init; }
    public required TextureRegion AttackTexture { get; init; }
    public HashSet<Point> MovementPoints { get; set; } = [];
    public HashSet<Point> AttackPoints { get; set; } = [];
    public bool IsVisible { get; private set; } = false;
    
    public void Show(int maxMovement, int attackRange, Point start, GridTileList<T> gridTiles, Dictionary<Point, IEntity> entities)
    {
        // If the cursor is currently in an unwalkable area, don't start the path.
        GridTile<T> startTile = gridTiles[start];
        if (!startTile.GetTileInfo().CanWalk)
            return;

        IsVisible = true;

        // Dijkstra search the space from start for reachable nodes.
        MovementPoints = Dijkstra.GetWalkable(start, maxMovement, gridTiles, entities);
        AttackPoints = Dijkstra.GetAttackable(attackRange, MovementPoints, gridTiles);
    }

    public void Hide()
    {
        MovementPoints.Clear();
        AttackPoints.Clear();
        IsVisible = false;
    }
}
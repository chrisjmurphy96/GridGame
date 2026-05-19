using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

/// <summary>
/// This could probably have stricter validation to make sure we have a valid rectangle.
/// Honestly for right now I only care about it for the utility of always being able to grab the index
/// without re-implementing the index every time.
/// </summary>
public class GridTileList : List<GridTile>
{
    /// <summary>
    /// Gets the grid tile at column and row.
    /// </summary>
    public GridTile this[int column, int row] => this[GetIndex(column, row)];

    public GridTile this[Point point] => this[point.X, point.Y];

    public bool InBounds(Point point) => point.Y >= 0 && point.Y < Rows && point.X >= 0 && point.X < _columns;

    private readonly int _columns;
    private int Rows => Count > 0 ? Count / _columns : 0;

    public GridTileList(int columns) : base()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        _columns = columns;
    }

    private int GetIndex(int column, int row) => (row * _columns) + column;
}
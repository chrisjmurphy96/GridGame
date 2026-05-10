using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

/// <summary>
/// This could probably have stricter validation to make sure we have a valid rectangle.
/// Honestly for right now I only care about it for the utility of always being able to grab the index
/// without re-implementing the index every time.
/// </summary>
public class GridTileList<T>(int columns) : List<GridTile<T>>() where T : struct, Enum
{
    /// <summary>
    /// Gets the grid tile at column and row.
    /// </summary>
    public GridTile<T> this[int column, int row] => this[GetIndex(column, row)];

    public GridTile<T> this[Point point] => this[point.X, point.Y];

    private int Rows => Count / _columns;

    public bool InBounds(Point point) => point.Y >= 0 && point.Y < Rows && point.X >= 0 && point.X < _columns;

    private readonly int _columns = columns;

    private int GetIndex(int column, int row) => (row * _columns) + column;
}
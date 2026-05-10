using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Utility functions for common grid operations.
/// The <see cref="GridLibrary.Grid.Grid{tileType}" /> operates on unit distance,
/// that's why things are +/- 1.
/// </summary>
public static class PointExtensions
{
    public static int DistanceTo(this Point point, Point other) => Math.Abs(other.X - point.X) + Math.Abs(other.Y - point.Y);

    public static Point[] GetNeighbors(this Point point) => [Up(point), Down(point), Left(point), Right(point)];
    public static Point Up(this Point point) => new() { X = point.X, Y = point.Y - 1 };
    public static Point Down(this Point point) => new() { X = point.X, Y = point.Y + 1 };
    public static Point Left(this Point point) => new() { X = point.X - 1, Y = point.Y };
    public static Point Right(this Point point) => new() { X = point.X + 1, Y = point.Y };

    public static bool IsAbove(this Point point, Point other) => point.Y < other.Y;
    public static bool IsBelow(this Point point, Point other) => point.Y > other.Y;
    public static bool IsToTheLeftOf(this Point point, Point other) => point.X < other.X;
    public static bool IsToTheRightOf(this Point point, Point other) => point.X > other.X;
}
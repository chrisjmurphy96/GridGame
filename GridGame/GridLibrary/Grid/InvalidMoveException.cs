using System;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class InvalidMoveException(Point start, Point end) : Exception($"Cannot reach {end} from {start}")
{
}

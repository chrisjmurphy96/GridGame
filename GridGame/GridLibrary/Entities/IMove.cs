using System;
using System.Collections.Generic;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;

namespace GridLibrary.Entities;

// Could be anything?
public interface IMove
{
    public string Name { get; }
    public int Range { get; }
    public int Damage { get; }
    public int HitChance { get; }
    public int CritChance { get; }
    public void PreviewMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target);
        
    /// <summary>
    /// Takes a list of entities the move is being performed on.
    /// </summary>
    public void PerformMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target);
}

// public interface IAttack : IMove
// {
//     public int LowestValue { get; }
//     public int HighestValue { get; }
//     public int CritChance { get; }
//     public int HitChance { get; }
// }

// // Support moves can be very complicated. They might allow you to
// // heal, teleport, make a wall, etc., and all of the above on
// // one or more allies or yourself.
// public interface ISupportMove : IMove
// {
//     public int LowestValue { get; }
//     public int HighestValue { get; }
// }
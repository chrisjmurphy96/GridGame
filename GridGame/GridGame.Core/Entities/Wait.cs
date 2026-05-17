using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using Microsoft.Xna.Framework;

namespace GridGame.Core.Entities;

public class WaitMove : IMove
{
    public string Name => "Wait";
    public int Damage => 0;
    public int Range => 0;
    public int HitChance => 0;
    public int CritChance => 0;

    public void PreviewMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target)
    {
        // nothing to do
    }

    public void PerformMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target)
    {
        // nothing to do
    }
}
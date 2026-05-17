using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using Microsoft.Xna.Framework;

namespace GridGame.Core.AttackMoves;

public class MeleeAttack : IMove
{
    public required string Name { get; init; }
    public required int Damage { get; init; }
    public required int HitChance { get; init; }
    public required int CritChance { get; init; }
    public int Range => 1;

    public void PreviewMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target)
    {
        // throw new NotImplementedException();
    }

    public void PerformMove(
        Dictionary<Point, IEntity> entities,
        Point movementEnd,
        Point target)
    {
        // we must be making an attack if this is true
        if (target != movementEnd)
        {
            entities.TryGetValue(target, out IEntity? entityToAttack);
            if (entityToAttack is not null)
            {
                int attackValue = Random.Shared.Next(6, 8);
                entityToAttack.Health.Subtract(attackValue);
            }
        }
    }
}
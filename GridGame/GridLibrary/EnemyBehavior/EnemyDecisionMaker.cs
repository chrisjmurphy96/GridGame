using GridLibrary.Entities;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GridLibrary.EnemyBehavior;

public static class EnemyDecisionMaker
{
    private const int KILL_SCORE = 1000;
    /// <summary>
    /// Number of steps we search down for determining movement (when attacks aren't possible)
    /// </summary>
    private const int MAX_WALK_DISTANCE = 100;

    /// <summary>
    /// This doesn't account for a number of things at the moment. Here's an (incomplete) list:
    /// - Units that can ignore blocking terrain (!CanWalk)
    /// - Unreachable islands (for example a square of walkable terrain surrounded by unwalkable terrain)
    /// - Units that can heal (themselves or others)
    /// - Doesn't prioritize standing on tiles giving advantages such as Avoid, Defence, or Healing
    /// </summary>
    public static Decision? GetNextDecision()
    {
        Dictionary<Point, IEntity> entities = GridState.Instance.Entities;
        IEnumerable<KeyValuePair<Point, IEntity>> candidates = entities.Where(pointToEntity => !pointToEntity.Value.IsFriendly && !pointToEntity.Value.HasMoved);
        if (!candidates.Any())
            return null;
        (Point position, IEntity entity) = candidates.First();
        GridTileList gridTiles = GridState.Instance.Tiles;
        HashSet<Point> walkable = Dijkstra.GetWalkable(position, entity.MovementRange, gridTiles, entities, forEnemy: true);
        // Try to attack first
        int highScore = -1;
        Decision decision = new()
        {
            Entity = entity,
            PreviousPosition = position
        };
        List<KeyValuePair<Point, IEntity>> friendlyEntities = [.. entities.Where(pointToEntity => pointToEntity.Value.IsFriendly)];
        for (int moveIndex = 0; moveIndex < entity.Moves.Count; moveIndex++)
        {
            IMove move = entity.Moves[moveIndex];
            HashSet<Point> attackable = Dijkstra.GetAttackable(move.Range, walkable, gridTiles);
            //attackable.UnionWith(walkable);
            IEnumerable<Point> attackPoints = attackable.Where(attackablePoint => entities.ContainsKey(attackablePoint) && entities[attackablePoint].IsFriendly);
            foreach (Point attackPoint in attackPoints)
            {
                int localScore = 0;
                IEntity toAttack = entities[attackPoint];

                // I'm thinking we don't factor in crit right now?
                int potentialDamage = MathHelper.Max(0, move.Damage - toAttack.Defense);
                // How much damage can we deal?
                if (potentialDamage > 0)
                    localScore += potentialDamage;
                // Can we kill the entity?
                bool kills = toAttack.Health.CurrentHealth - potentialDamage <= 0;
                if (kills)
                    localScore += KILL_SCORE;
                
                // Attempt to attack from as far away as possible. This should generally
                // make the unit attack from a safer position.
                int distanceScore = -1;
                foreach (Point walkingPoint in walkable)
                {
                    int distance = walkingPoint.DistanceTo(attackPoint);
                    bool inRange = distance <= move.Range;
                    bool betterDistanceScore = distance > distanceScore;
                    bool isOccupied = GridState.Instance.Entities.ContainsKey(walkingPoint);
                    if (inRange && betterDistanceScore && !isOccupied)
                    {
                        distanceScore = distance;
                        decision.Position = walkingPoint;
                    }
                }
                // if we fail to find a valid position, skip to the next option.
                if (decision.Position is null)
                    continue;
                    
                int potentialDamageTaken = 0;
                int toAttackRange = toAttack.SelectedMove.Range;
                if (decision.Position.Value.DistanceTo(attackPoint) <= toAttackRange)
                {
                    potentialDamageTaken = toAttack.SelectedMove.Damage - entity.Defense;
                }

                // don't let damage taken stop us from attacking, make sure our score stays at least 0
                localScore = MathHelper.Max(0, localScore - potentialDamageTaken);

                if (localScore > highScore)
                {
                    highScore = localScore;
                    decision.AttackPosition = attackPoint;
                    decision.MoveIndex = moveIndex;
                }
            }
        }
        if (decision.AttackPosition is not null)
            return decision;

        // If we can't attack, can we move?
        // We just attempt to find the closest entity from our current position, following
        // pathing rules.

        // "Flood" algorithm for the map, filtering out spaces that are invalid moves.
        HashSet<Point> completeWalkableSpace = [];
        for (int column = 0; column < gridTiles.Columns; column++)
        {
            for (int row = 0; row < gridTiles.Rows; row++)
            {
                Point point = new(column, row);
                GridTile gridTile = gridTiles[column, row];
                bool isOccupiedByFriendly = entities.TryGetValue(point, out IEntity? entityToCheck) && entityToCheck?.IsFriendly is true;
                if (gridTile.TileInfo.CanWalk && !isOccupiedByFriendly)
                    completeWalkableSpace.Add(point);
            }
        }
        int minDistance = MAX_WALK_DISTANCE + 1;
        Point goal = Point.Zero;
        foreach ((Point friendlyEntityPosition, _) in friendlyEntities)
        {
            // Add the space just for the entity we're trying to reach. Make sure to remove this at the end.
            completeWalkableSpace.UnionWith([friendlyEntityPosition]);

            List<Point>? path = Dijkstra.FindShortestPath(position, friendlyEntityPosition, MAX_WALK_DISTANCE, completeWalkableSpace);
            if (path?.Count > 0)
            {
                // should this be +1?
                if (path.Count > entity.MovementRange)
                    path = [.. path.Take(entity.MovementRange)];

                for (int pathIndex = path.Count - 1; pathIndex > 0; pathIndex--)
                {
                    Point potentialPosition = path[pathIndex];
                    int distance = pathIndex + 1;
                    bool isOccupied = entities.ContainsKey(potentialPosition);
                    if (distance < minDistance && !isOccupied)
                    {
                        minDistance = distance;
                        decision.Position = potentialPosition;
                        // short circuit once we find the first open position in the path
                        break;
                    }
                }
            }

            completeWalkableSpace.Remove(friendlyEntityPosition);
        }
        
        return decision;
    }
}

using GridLibrary.Entities;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GridLibrary.EnemyBehavior;

public static class EnemyDecisionMaker
{
    private const int IN_RANGE_MODIFIER = 10000;
    private const int KILL_MODIFIER = 100000;
    /// <summary>
    /// Number of steps we search down for determining movement (when attacks aren't possible).
    /// This can be tweaked, but every increase is very expensive.
    /// </summary>
    private const int MAX_WALK_DISTANCE = 40;

    /// <summary>
    /// This doesn't account for a number of things at the moment. Here's an (incomplete) list:
    /// - Units that can ignore blocking terrain (!CanWalk)
    /// - Unreachable islands (for example a square of walkable terrain surrounded by unwalkable terrain)
    /// - Units that can heal (themselves or others)
    /// - Doesn't prioritize standing on tiles giving advantages such as Avoid, Defence, or Healing
    /// - Always tries to move, even when standing still is acceptable (most noticable with multiple friendly entities)
    /// </summary>
    public static Decision? GetNextDecision()
    {
        Dictionary<Point, IEntity> entities = GridState.Instance.Entities;
        IEnumerable<KeyValuePair<Point, IEntity>> candidates = entities.Where(pointToEntity => !pointToEntity.Value.IsFriendly && !pointToEntity.Value.HasMoved);
        if (!candidates.Any())
            return null;
        (Point position, IEntity currentEntity) = candidates.First();
        GridTileList gridTiles = GridState.Instance.Tiles;
        HashSet<Point> currentWalkableSpace = Dijkstra.GetWalkable(position, currentEntity.MovementRange, gridTiles, entities, forEnemy: true);
        // Try to attack first
        int highScore = int.MinValue;
        Decision decision = new()
        {
            Entity = currentEntity,
            PreviousPosition = position
        };
        bool cannotMove = currentWalkableSpace.Count is 1;
        if (cannotMove)
            return decision;
        Stopwatch attackStopwatch = new();
        attackStopwatch.Start();
        for (int moveIndex = 0; moveIndex < currentEntity.Moves.Count; moveIndex++)
        {
            IMove move = currentEntity.Moves[moveIndex];
            HashSet<Point> attackable = Dijkstra.GetAttackable(move.Range, currentWalkableSpace, gridTiles);
            IEnumerable<Point> attackPoints = attackable.Where(attackablePoint => entities.ContainsKey(attackablePoint) && entities[attackablePoint].IsFriendly);
            foreach (Point attackPoint in attackPoints)
            {
                int localScore = 0;
                Point? localBestPosition = null;
                IEntity toAttack = entities[attackPoint];

                // I'm thinking we don't factor in crit right now?
                int potentialDamage = MathHelper.Max(0, move.Damage - toAttack.Defense);
                // How much damage can we deal?
                if (potentialDamage > 0)
                    localScore += potentialDamage;
                // Can we kill the entity?
                bool kills = toAttack.Health.CurrentHealth - potentialDamage <= 0;
                if (kills)
                    localScore += KILL_MODIFIER;
                
                // Attempt to attack from as far away as possible. This should generally
                // make the unit attack from a safer position.
                int distanceScore = -1;
                foreach (Point walkingPoint in currentWalkableSpace)
                {
                    bool isCurrentSpaceInner = walkingPoint == position;
                    int sameSpaceModifier = isCurrentSpaceInner ? 1 : 0;
                    int distance = walkingPoint.DistanceTo(attackPoint);
                    bool inRange = distance <= move.Range;
                    bool betterDistanceScore = (distance * 10 + sameSpaceModifier) > distanceScore;
                    bool isOccupied = GridState.Instance.Entities.ContainsKey(walkingPoint);
                    if (inRange && betterDistanceScore && (!isOccupied || isCurrentSpaceInner))
                    {
                        distanceScore = distance * 10 + sameSpaceModifier;
                        localBestPosition = walkingPoint;
                    }
                }
                // if we fail to find a valid position, skip to the next option.
                if (localBestPosition is null)
                    continue;
                    
                int potentialDamageTaken = 0;
                int toAttackRange = toAttack.SelectedMove.Range;
                if (localBestPosition.Value.DistanceTo(attackPoint) <= toAttackRange)
                {
                    potentialDamageTaken = toAttack.SelectedMove.Damage - currentEntity.Defense;
                }

                // don't let damage taken stop us from attacking, make sure our score stays at least 0
                localScore = localScore - potentialDamageTaken;
                localScore *= 10;
                bool isCurrentSpace = localBestPosition == position;
                if (isCurrentSpace)
                    localScore += 1;
                if (localScore > highScore)
                {
                    highScore = localScore;
                    decision.AttackPosition = attackPoint;
                    decision.MoveIndex = moveIndex;
                    decision.Position = localBestPosition;
                }
            }
        }
        if (decision.AttackPosition is not null)
            return decision;

        attackStopwatch.Stop();


        // Create flood of walkable space to find other attack options if we leave our current walking range
        List<Point> fullWalkableSpace = [];
        Dictionary<Point, IEntity> friendlyEntities = [];
        for (int column = 0; column < gridTiles.Columns; column++)
        {
            for (int row = 0; row < gridTiles.Rows; row++)
            {
                Point point = new(column, row);
                if (entities.TryGetValue(point, out IEntity? entity) && entity?.IsFriendly is true)
                    friendlyEntities.Add(point, entity);
                else if (gridTiles[point].TileInfo.CanWalk)
                    fullWalkableSpace.Add(point);
            }
        }

        // Perhaps we can save some cycles by pre-computing the optimal target to go for in terms of damage,
        // and then inspect if we can reach them in that order. Since checking expected damage is pretty cheap and fast,
        // this should let us short circuit pretty often I think.
        // Do we consider how much damage we take back still?
        highScore = int.MinValue;
        HashSet<Point> fullWalkableSpaceHashSet = [.. fullWalkableSpace];
        foreach ((Point toAttackPosition, IEntity toAttack) in friendlyEntities)
        {
            for (int moveIndex = 0; moveIndex < currentEntity.Moves.Count; moveIndex++)
            {
                IMove move = currentEntity.Moves[moveIndex];
                int localScore = 0;

                // I'm thinking we don't factor in crit right now?
                int potentialDamage = MathHelper.Max(0, move.Damage - toAttack.Defense);
                // How much damage can we deal?
                if (potentialDamage > 0)
                    localScore += potentialDamage;
                // Can we kill the entity?
                bool kills = toAttack.Health.CurrentHealth - potentialDamage <= 0;
                if (kills)
                    localScore += KILL_MODIFIER;

                // Attempt to attack from as far away as possible. This should generally
                // make the unit attack from a safer position.
                Stopwatch attackPointStopwatch = new();
                attackPointStopwatch.Start();
                int distanceScore = int.MinValue;
                Dictionary<Point, int> positionToScore = [];
                foreach (Point walkingPoint in fullWalkableSpace)
                {
                    bool isCurrentSpaceInner = walkingPoint == position;
                    int sameSpaceModifier = isCurrentSpaceInner ? 1 : 0;
                    int distance = walkingPoint.DistanceTo(toAttackPosition);
                    bool inRange = distance <= move.Range;
                    bool isOccupied = GridState.Instance.Entities.ContainsKey(walkingPoint);
                    if ((!isOccupied || isCurrentSpaceInner))
                    {
                        if (inRange)
                        {
                            int localDistanceScore = (distance * 10) + sameSpaceModifier;
                            if (localDistanceScore >= distanceScore)
                            {
                                distanceScore = localDistanceScore;
                                positionToScore.Add(walkingPoint, distanceScore);
                            }
                        }
                        else if (distance <= 10)
                        {
                            int localDistanceScore = (distance * -10) + sameSpaceModifier;
                            if (localDistanceScore >= distanceScore)
                            {
                                distanceScore = localDistanceScore;
                                positionToScore.Add(walkingPoint, distanceScore);
                            }
                        }
                    }
                }
                // if we fail to find a valid position, skip to the next option.
                if (positionToScore.Count is 0)
                    continue;

                IEnumerable<Point> moveCandidates = positionToScore.Where(p => p.Value == distanceScore).Select(p => p.Key);
                Point? closestPoint = null;
                Point localBestPosition = moveCandidates.First();
                int bestDistanceToTarget = int.MaxValue;
                foreach (Point moveCandidate in moveCandidates)
                {
                    foreach (Point walkablePoint in currentWalkableSpace)
                    {
                        bool isCurrentSpaceInner = walkablePoint == position;
                        if (entities.ContainsKey(walkablePoint) && !isCurrentSpaceInner)
                            continue;
                        // Doing this in a loop is the single most expensive operation. The easiest performance toggle is MAX_WALK_DISTANCE.
                        int currentDistance = Dijkstra.FindShortestPath(walkablePoint, moveCandidate, MAX_WALK_DISTANCE, fullWalkableSpaceHashSet)?.Count ?? int.MaxValue;
                        if (currentDistance < bestDistanceToTarget)
                        {
                            bestDistanceToTarget = currentDistance;
                            closestPoint = walkablePoint;
                            localBestPosition = moveCandidate;
                        }
                    }
                }
                if (closestPoint is null)
                    continue;
                // Weight the distance a bit. This might result in some suboptimal moves, but
                // I think it's worth the tradeoff of keeping the game moving in these more extreme distance scenarios.
                localScore -= bestDistanceToTarget;

                int potentialDamageTaken = 0;
                int toAttackRange = toAttack.SelectedMove.Range;
                if (localBestPosition.DistanceTo(toAttackPosition) <= move.Range)
                {
                    int potentialDamageDealt = move.Damage - toAttack.Defense;
                    localScore += potentialDamageDealt;
                    localScore += IN_RANGE_MODIFIER;
                    if (potentialDamageDealt >= toAttack.Health.CurrentHealth)
                        localScore += KILL_MODIFIER;
                }
                // Always pretend we'll get hit back
                potentialDamageTaken = toAttack.SelectedMove.Damage - currentEntity.Defense;

                localScore = localScore - potentialDamageTaken;
                localScore *= 10;
                bool isCurrentSpace = localBestPosition == position;
                if (isCurrentSpace)
                    localScore += 1;
                if (localScore > highScore)
                {
                    highScore = localScore;
                    decision.Position = closestPoint;
                }
            }
        }
        
        return decision;
    }
}

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
    private const int KILL_MODIFIER = 500;
    private const int DEATH_MODIFIER = 250;
    /// <summary>
    /// Number of steps we search down for determining movement (when attacks aren't possible)
    /// </summary>
    private const int MAX_WALK_DISTANCE = 100;
    private const int NO_CONTACT_MODIFIER = 20;

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
                foreach (Point walkingPoint in walkable)
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
                        Debug.WriteLine($"Current best: {walkingPoint}, Score: {distanceScore}");
                    }
                }
                // if we fail to find a valid position, skip to the next option.
                if (localBestPosition is null)
                    continue;
                    
                int potentialDamageTaken = 0;
                int toAttackRange = toAttack.SelectedMove.Range;
                if (localBestPosition.Value.DistanceTo(attackPoint) <= toAttackRange)
                {
                    potentialDamageTaken = toAttack.SelectedMove.Damage - entity.Defense;
                }

                // don't let damage taken stop us from attacking, make sure our score stays at least 0
                localScore = MathHelper.Max(0, localScore - potentialDamageTaken);
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
                    Debug.WriteLine($"Current best: {decision.Position}, High score: {highScore}");
                }
            }
        }
        if (decision.AttackPosition is not null)
            return decision;

        // Check for attacks we can make if we leave our current walkable area
        //HashSet<Point> maxWalkable = Dijkstra.GetWalkable(position, MAX_WALK_DISTANCE, gridTiles, entities, forEnemy: true);
        //for (int moveIndex = 0; moveIndex < entity.Moves.Count; moveIndex++)
        //{
        //    IMove move = entity.Moves[moveIndex];
        //    HashSet<Point> attackable = Dijkstra.GetAttackable(move.Range, maxWalkable, gridTiles);
        //    IEnumerable<Point> attackPoints = attackable.Where(attackablePoint => entities.ContainsKey(attackablePoint) && entities[attackablePoint].IsFriendly);
        //    foreach (Point attackPoint in attackPoints)
        //    {
        //        int localScore = 0;
        //        IEntity toAttack = entities[attackPoint];

        //        // I'm thinking we don't factor in crit right now?
        //        int potentialDamage = MathHelper.Max(0, move.Damage - toAttack.Defense);
        //        // How much damage can we deal?
        //        if (potentialDamage > 0)
        //            localScore += potentialDamage;
        //        // Can we kill the entity?
        //        bool kills = toAttack.Health.CurrentHealth - potentialDamage <= 0;
        //        if (kills)
        //            localScore += KILL_MODIFIER;

        //        // Attempt to attack from as far away as possible. This should generally
        //        // make the unit attack from a safer position.
        //        int distanceScore = -1;
        //        foreach (Point walkingPoint in walkable)
        //        {
        //            int distance = walkingPoint.DistanceTo(attackPoint);
        //            bool inRange = distance <= move.Range;
        //            bool betterDistanceScore = distance > distanceScore;
        //            bool isOccupied = GridState.Instance.Entities.ContainsKey(walkingPoint);
        //            if (inRange && betterDistanceScore && !isOccupied)
        //            {
        //                distanceScore = distance;
        //                Dijkstra.FindShortestPath(position, walkingPoint, MAX_WALK_DISTANCE)
        //                Point actualPoint = 
        //                decision.Position = walkingPoint;
        //            }
        //        }
        //        // if we fail to find a valid position, skip to the next option.
        //        if (decision.Position is null)
        //            continue;

        //        int potentialDamageTaken = 0;
        //        int toAttackRange = toAttack.SelectedMove.Range;
        //        if (decision.Position.Value.DistanceTo(attackPoint) <= toAttackRange)
        //        {
        //            potentialDamageTaken = toAttack.SelectedMove.Damage - entity.Defense;
        //        }

        //        // don't let damage taken stop us from attacking, make sure our score stays at least 0
        //        localScore = MathHelper.Max(0, localScore - potentialDamageTaken);

        //        if (localScore > highScore)
        //        {
        //            highScore = localScore;
        //            decision.AttackPosition = attackPoint;
        //            decision.MoveIndex = moveIndex;
        //        }
        //    }
        //}
        //if (decision.AttackPosition is not null)
        //    return decision;

        // If we can't attack, can we move?
        // We just attempt to find the closest entity from our current position, following
        // pathing rules.
        // Prioritize entities that we can actually reach, over ones with no open sides

        // "Flood" algorithm for the map, filtering out spaces that are invalid moves.
        //HashSet<Point> completeWalkableSpace = [];
        //for (int column = 0; column < gridTiles.Columns; column++)
        //{
        //    for (int row = 0; row < gridTiles.Rows; row++)
        //    {
        //        Point point = new(column, row);
        //        GridTile gridTile = gridTiles[column, row];
        //        bool isOccupiedByFriendly = entities.TryGetValue(point, out IEntity? entityToCheck) && entityToCheck?.IsFriendly is true;
        //        if (gridTile.TileInfo.CanWalk && !isOccupiedByFriendly)
        //            completeWalkableSpace.Add(point);
        //    }
        //}
        ////int minDistance = MAX_WALK_DISTANCE + 1;
        ////int 
        //Point goal = Point.Zero;
        //// Doesn't account for if the move does damage/should target enemies
        //int highestPossibleRange = entity.Moves.Select(move => move.Range).OrderDescending().First();
        //foreach ((Point friendlyEntityPosition, _) in friendlyEntities)
        //{
        //    IEnumerable<(Point point, GridTile tile)> tiles = gridTiles.GetNeighborsWithinRange(friendlyEntityPosition, highestPossibleRange);
        //    foreach((Point point, GridTile tile) in tiles)
        //    {
                
        //    }


        //    // new plan, do a dijkstra search for surrounding tiles, pick one with least distance to entity? Sub-priority of closest to current position.
        //    // How to sort:
        //    // - Length of valid path after walking back to valid ground. Most important
        //    //   - Raw distance to target?
        //    //     - Distance from current position?
        //    // Maybe can simplify to: (distance to target after getting as close as possible)
        //    HashSet<Point> closestValidNeighbors = [];
        //    IEnumerable<Point> neighbors = friendlyEntityPosition.GetNeighbors().Where(gridTiles.InBounds);

        //    foreach (Point neighbor in neighbors)
        //    {
        //        HashSet<Point>? path = Dijkstra.FindShortestPath(position, neighbor, MAX_WALK_DISTANCE, completeWalkableSpace);
        //        if (path is null)
        //            continue;
        //        for (int pathIndex = path.Count - 1; pathIndex > 0; pathIndex--)
        //        {
        //        }
        //    }


        //    // find valid surrounding tiles we could route to
        //    // can expand in one direction at a time?
        //    // set a limit at 3 tiles away or something?
        //    //    HashSet<Point> closestValidNeighbors = [];
        //    //IEnumerable<Point> neighbors = friendlyEntityPosition.GetNeighbors().Where(gridTiles.InBounds);
        //    //foreach (Point neighbor in neighbors)
        //    //{
        //    //    if (entities.ContainsKey(neighbor) || !gridTiles[neighbor].TileInfo.CanWalk)
        //    //    {
        //    //        // expand search

        //    //    }
        //    //    else
        //    //        closestValidNeighbors.Add(neighbor);
        //    //}


        //    //bool noOpenSpots = false;
        //    // Add the space just for the entity we're trying to reach. Make sure to remove this at the end.
        //    completeWalkableSpace.UnionWith([friendlyEntityPosition]);

        //    // TODO: If spaces around entity are occupied, target them instead?
        //    List<Point>? path = Dijkstra.FindShortestPath(position, friendlyEntityPosition, MAX_WALK_DISTANCE, completeWalkableSpace);
        //    if (path?.Count > 0)
        //    {
        //        // should this be +1?
        //        if (path.Count > entity.MovementRange)
        //            path = [.. path.Take(entity.MovementRange)];

        //        Point potentialPosition = path.Last();//[pathIndex];
        //        int distance = path.Count;
        //        bool isOccupied = entities.ContainsKey(potentialPosition);
        //        if (distance < minDistance && !isOccupied)
        //        {
        //            minDistance = distance;
        //            decision.Position = potentialPosition;
        //            // short circuit once we find the first open position in the path
        //            //break;
        //        }
        //        else
        //        {
        //            IEnumerable<Point[]> surroundingPoints = friendlyEntityPosition.GetNeighbors().Select(neighbor => neighbor.GetNeighbors());
        //            HashSet<Point> surroundingPointsSet = [];
        //            foreach (Point[] neighboringPoints in surroundingPoints)
        //            {
        //                surroundingPointsSet.UnionWith(neighboringPoints);
        //            }
        //            surroundingPointsSet.Remove(friendlyEntityPosition);
        //            foreach (Point endPoint in surroundingPointsSet)
        //            {
        //                if (entities.ContainsKey(endPoint) || !gridTiles[endPoint].TileInfo.CanWalk)
        //                    continue;

        //                // TODO: If spaces around entity are occupied, target them instead?
        //                List<Point>? closestPath = Dijkstra.FindShortestPath(position, endPoint, MAX_WALK_DISTANCE, completeWalkableSpace);
        //                if (closestPath?.Count > 0)
        //                {
        //                    // should this be +1?
        //                    if (closestPath.Count > entity.MovementRange)
        //                        closestPath = [.. closestPath.Take(entity.MovementRange)];

        //                    Point potentialPosition2 = closestPath.Last();//[pathIndex];
        //                    int distance2 = closestPath.Count;
        //                    bool isOccupied2 = entities.ContainsKey(potentialPosition2);
        //                    if (distance < minDistance && !isOccupied)
        //                    {
        //                        minDistance = distance2 + NO_CONTACT_MODIFIER;
        //                        decision.Position = potentialPosition2;
        //                        // short circuit once we find the first open position in the path
        //                        //break;
        //                    }
        //                    //for (int pathIndex = path.Count - 1; pathIndex > 0; pathIndex--)
        //                    //{
        //                    //}
        //                }
        //            }
        //        }
        //        //for (int pathIndex = path.Count - 1; pathIndex > 0; pathIndex--)
        //        //{
        //        //}
        //    }
        //    //else if (noOpenSpots) // expand the search till we find a viable candidate?
        //    //{
        //    //    //int maxBreadth = 3;
        //    //    //int currentBreadth = 0;
        //    //    IEnumerable<Point[]> surroundingPoints = friendlyEntityPosition.GetNeighbors().Select(neighbor => neighbor.GetNeighbors());
        //    //    HashSet<Point> surroundingPointsSet = [];
        //    //    foreach (Point[] neighboringPoints in surroundingPoints)
        //    //    {
        //    //        surroundingPointsSet.UnionWith(neighboringPoints);
        //    //    }
        //    //    foreach(Point endPoint in surroundingPointsSet)
        //    //    {
        //    //        if (entities.ContainsKey(endPoint) || !gridTiles[endPoint].TileInfo.CanWalk)
        //    //            continue;

        //    //        // TODO: If spaces around entity are occupied, target them instead?
        //    //        List<Point>? closestPath = Dijkstra.FindShortestPath(position, endPoint, MAX_WALK_DISTANCE, completeWalkableSpace);
        //    //        if (closestPath?.Count > 0)
        //    //        {
        //    //            // should this be +1?
        //    //            if (closestPath.Count > entity.MovementRange)
        //    //                closestPath = [.. closestPath.Take(entity.MovementRange)];

        //    //            Point potentialPosition = closestPath.Last();//[pathIndex];
        //    //            int distance = closestPath.Count;
        //    //            bool isOccupied = entities.ContainsKey(potentialPosition);
        //    //            if (distance < minDistance && !isOccupied)
        //    //            {
        //    //                minDistance = distance + NO_CONTACT_MODIFIER;
        //    //                decision.Position = potentialPosition;
        //    //                // short circuit once we find the first open position in the path
        //    //                //break;
        //    //            }
        //    //            //for (int pathIndex = path.Count - 1; pathIndex > 0; pathIndex--)
        //    //            //{
        //    //            //}
        //    //        }
        //    //    }
        //    //}

        //    completeWalkableSpace.Remove(friendlyEntityPosition);
        //}
        
        return decision;
    }

    /// <summary>
    /// 
    /// </summary>
    //private static Point GetTargetPosition(Point startPosition, Point endPosition, List<IEntity> entities, GridTileList gridTiles)
    //{

    //}
}

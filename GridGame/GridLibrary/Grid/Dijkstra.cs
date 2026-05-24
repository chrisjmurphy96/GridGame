using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Entities;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

/// <summary>
/// This really is just classic Dijkstra, as seen here: https://www.geeksforgeeks.org/dsa/dijkstras-shortest-path-algorithm-greedy-algo-.7/
/// The main difference is I added logic around the grid itself, checking boundaries,
/// if a tile is walkable, etc. I also added the concept of closest neighbor so that
/// I could build a path from the result, instead of just having a distance number.
/// </summary>
public static class Dijkstra
{
    private const int MAX_ITERATIONS = 200;

    public static List<Point>? FindShortestPath(
        Point start,
        Point end,
        int maxMovement,
        HashSet<Point> walkableSpace)
    {
        int distance = start.DistanceTo(end);
        if (distance > maxMovement)
            throw new InvalidMoveException(start, end);
        Dictionary<Point, SearchNode> exploredSpace = new()
        {
            { start, new SearchNode { LowestCostNeighbor = start, StepsToReach = 0 } }
        };
        // lowest priority is removed first. From here, we can use
        // distance remaining to end as the priority
        PriorityQueue<Point, int> _searchQueue = new();
        _searchQueue.Enqueue(start, distance);

        int iterations = 0;
        while (_searchQueue.Count > 0 && iterations < MAX_ITERATIONS)
        {
            Point nextPoint = _searchQueue.Dequeue();
            SearchNode currentNode = exploredSpace[nextPoint];
            if (currentNode.StepsToReach >= maxMovement)
                continue;
            foreach(Point neighbor in nextPoint.GetNeighbors())
            {
                if (!walkableSpace.Contains(neighbor))
                    continue;

                int neighborDistanceToEnd = neighbor.DistanceTo(end);

                if (exploredSpace.TryGetValue(neighbor, out SearchNode? node))
                {
                    int newStepsToReach = currentNode.StepsToReach + 1;
                    if (newStepsToReach < node.StepsToReach)
                    {
                        node.StepsToReach = newStepsToReach;
                        node.LowestCostNeighbor = nextPoint;
                        _searchQueue.Enqueue(neighbor, neighborDistanceToEnd);
                    }
                }
                else
                {
                    exploredSpace.Add(neighbor, new SearchNode
                    {
                        StepsToReach = currentNode.StepsToReach + 1,
                        LowestCostNeighbor = nextPoint
                    });
                    _searchQueue.Enqueue(neighbor, neighborDistanceToEnd);
                }
            }
        }

        // safely return if we don't find a solution.
        // sometimes there is no valid path.
        if (!exploredSpace.ContainsKey(end))
        {
            return null;
        }

        // build back the solution from the end, then reverse it to to get the proper order.
        Point point = end;
        List<Point> solution = [end];
        while (point != start)
        {
            SearchNode node = exploredSpace[point];
            solution.Add(node.LowestCostNeighbor);
            point = node.LowestCostNeighbor;
        }

        // the start position is not counted towards movement.
        // safely return if the shortest solution is still too long.
        if (solution.Count > maxMovement + 1)
        {
            return null;
        }

        solution.Reverse();
        return solution;
    }

    public static HashSet<Point> GetWalkable(
        Point start,
        int maxMovement,
        GridTileList gridTiles,
        Dictionary<Point, IEntity> entities,
        bool forEnemy = false)
    {
        Dictionary<Point, ReachableNode> exploredSpace = new()
        {
            { start, new ReachableNode { StepsToReach = 0 } }
        };
        PriorityQueue<Point, int> _searchQueue = new();
        _searchQueue.Enqueue(start, 0);

        int iterations = 0;
        while (_searchQueue.Count > 0 && iterations < MAX_ITERATIONS)
        {
            Point nextPoint = _searchQueue.Dequeue();
            ReachableNode currentNode = exploredSpace[nextPoint];
            if (currentNode.StepsToReach >= maxMovement)
                continue;
            
            foreach(Point neighbor in nextPoint.GetNeighbors())
            {
                if (!gridTiles.InBounds(neighbor))
                    continue;

                if (!gridTiles[neighbor].TileInfo.CanWalk)
                    continue;

                // This reads a bit confusingly, but basically if we're calculating
                // for friendly units, other friendly units are valid walking spaces,
                // but not enemies, and vice versa for enemy units.
                if (entities.TryGetValue(neighbor, out IEntity? entity) &&
                    entity is not null &&
                    entity.IsFriendly == forEnemy)
                    continue;

                int newStepsToReach = currentNode.StepsToReach + 1;
                if (exploredSpace.TryGetValue(neighbor, out ReachableNode? node))
                {
                    if (newStepsToReach < node.StepsToReach)
                    {
                        node.StepsToReach = newStepsToReach;
                        _searchQueue.Enqueue(neighbor, newStepsToReach);
                    }
                }
                else
                {
                    exploredSpace.Add(neighbor, new ReachableNode
                    {
                        StepsToReach = newStepsToReach
                    });
                    _searchQueue.Enqueue(neighbor, newStepsToReach);
                }
            }
        }

        return [.. exploredSpace.Select(node => node.Key)];
    }

    public static HashSet<Point> GetAttackable(int attackRange, HashSet<Point> walkablePoints, GridTileList gridTiles)
    {
        if (attackRange is 0)
            return [];

        HashSet<Point> attackPoints = [];
        foreach(Point point in walkablePoints)
        {
            Point[] neighbors = point.GetNeighbors();
            // find all points in attackRange but not in the walkable set
            foreach(Point neighbor in neighbors)
            {
                if (walkablePoints.Contains(neighbor))
                    continue;

                IEnumerable<Point> reachable = GetReachable(neighbor, attackRange, gridTiles)
                    .Where(reachablePoint => !walkablePoints.Contains(reachablePoint) && gridTiles.InBounds(reachablePoint));
                attackPoints.UnionWith(reachable);
            }
        }

        return attackPoints;
    }

    public static HashSet<Point> GetReachable(Point start, int range, GridTileList gridTiles)
    {
        Dictionary<Point, ReachableNode> exploredSpace = new()
        {
            { start, new ReachableNode { StepsToReach = 0 } }
        };
        PriorityQueue<Point, int> _searchQueue = new();
        _searchQueue.Enqueue(start, 0);

        int iterations = 0;
        while (_searchQueue.Count > 0 && iterations < MAX_ITERATIONS)
        {
            Point nextPoint = _searchQueue.Dequeue();
            ReachableNode currentNode = exploredSpace[nextPoint];
            if (currentNode.StepsToReach >= range - 1)
                continue;
            
            foreach(Point neighbor in nextPoint.GetNeighbors())
            {
                if (!gridTiles.InBounds(neighbor))
                    continue;

                int newStepsToReach = currentNode.StepsToReach + 1;
                if (exploredSpace.TryGetValue(neighbor, out ReachableNode? node))
                {
                    if (newStepsToReach < node.StepsToReach)
                    {
                        node.StepsToReach = newStepsToReach;
                        _searchQueue.Enqueue(neighbor, newStepsToReach);
                    }
                }
                else
                {
                    exploredSpace.Add(neighbor, new ReachableNode
                    {
                        StepsToReach = newStepsToReach
                    });
                    _searchQueue.Enqueue(neighbor, newStepsToReach);
                }
            }
        }

        return [.. exploredSpace.Select(node => node.Key)];
    }
}

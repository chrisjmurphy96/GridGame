using System;
using System.Collections.Generic;
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

    public static List<Point>? Search<T>(Point start, Point end, int maxMovement, GridTileList<T> gridTiles) where T : struct, Enum
    {
        int distance = GetDistance(start, end);
        if (distance > maxMovement)
            throw new InvalidMoveException(start, end);
        Dictionary<Point, DecisionGraphNode> exploredSpace = new()
        {
            { start, new DecisionGraphNode { LowestCostNeighbor = start, StepsToReach = 0 } }
        };
        // lowest priority is removed first. From here, we can use
        // distance remaining to end as the priority
        PriorityQueue<Point, int> _searchQueue = new();
        _searchQueue.Enqueue(start, distance);

        int iterations = 0;
        while (_searchQueue.Count > 0 && iterations < MAX_ITERATIONS)
        {
            Point nextPoint = _searchQueue.Dequeue();
            DecisionGraphNode currentNode = exploredSpace[nextPoint];
            if (currentNode.StepsToReach >= maxMovement)
                continue;
            foreach(Point neighbor in GetNeighbors(nextPoint))
            {
                if (!gridTiles.InBounds(neighbor))
                    continue;

                int neighborDistanceToEnd = GetDistance(end, neighbor);
                bool tooFarFromStart = GetDistance(start, neighbor) > maxMovement;
                bool tooFarFromEnd = neighborDistanceToEnd > maxMovement;
                if (tooFarFromStart || tooFarFromEnd)
                    continue;
                
                if (!gridTiles[neighbor].GetTileInfo().CanWalk)
                    continue;

                if (exploredSpace.TryGetValue(neighbor, out DecisionGraphNode? node))
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
                    exploredSpace.Add(neighbor, new DecisionGraphNode
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
            DecisionGraphNode node = exploredSpace[point];
            solution.Add(node.LowestCostNeighbor);
            point = node.LowestCostNeighbor;
        }

        solution.Reverse();
        return solution;
    }

    // These could all be extension methods for Point, if I want them in other places
    private static Point[] GetNeighbors(Point point) => [Up(point), Down(point), Left(point), Right(point)];
    private static Point Up(Point point) => new() { X = point.X, Y = point.Y - 1 };
    private static Point Down(Point point) => new() { X = point.X, Y = point.Y + 1 };
    private static Point Left(Point point) => new() { X = point.X - 1, Y = point.Y };
    private static Point Right(Point point) => new() { X = point.X + 1, Y = point.Y };
    // Grid distance is very simple since there is no diagonal.
    private static int GetDistance(Point a, Point b) => Math.Abs(b.X - a.X) + Math.Abs(b.Y - a.Y);
}

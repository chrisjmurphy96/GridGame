using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class DecisionGraphNode
{
    // keep this updated with current lowest number
    public int StepsToReach { get; set; } = int.MaxValue;
    public required Point LowestCostNeighbor { get; set; }
}

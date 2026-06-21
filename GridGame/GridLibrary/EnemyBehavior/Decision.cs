using GridLibrary.Entities;
using Microsoft.Xna.Framework;

namespace GridLibrary.EnemyBehavior;

public class Decision
{
    public required IEntity Entity { get; init; }
    public Point PreviousPosition { get; init; }
    public int? MoveIndex { get; set; } = null;
    public Point? Position { get; set; } = null;
    public Point? AttackPosition { get; set; } = null;
}
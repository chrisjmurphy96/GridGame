using GridLibrary.Entities;

namespace GridGame.Core.AttackMoves;

public class MeleeAttack : IMove
{
    public required string Name { get; init; }
    public required int Damage { get; init; }
    public required int HitChance { get; init; }
    public required int CritChance { get; init; }
    public int Range => 2;
    public int ContactFrame { get; init; }
}
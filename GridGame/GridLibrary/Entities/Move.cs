namespace GridLibrary.Entities;

public readonly struct Move : IMove
{
    public required string Name { get; init; }
    public required int Damage { get; init; }
    public required int HitChance { get; init; }
    public required int CritChance { get; init; }
    public required int Range { get; init; }
    public required int RegularContactFrame { get; init; }
    public required int CritContactFrame { get; init; }
    public required string RegularAnimationKey { get; init; }
    public required string CritAnimationKey { get; init; }
}
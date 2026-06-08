namespace GridLibrary.Entities;

// Could be anything?
public interface IMove
{
    public string Name { get; }
    public int Range { get; }
    public int Damage { get; }
    public int HitChance { get; }
    public int CritChance { get; }
    /// <summary>
    /// Frame count when damage should be applied.
    /// </summary>
    public int RegularContactFrame { get; }
    public int CritContactFrame { get; }
    public string RegularAnimationKey { get; }
    public string CritAnimationKey { get; }
}
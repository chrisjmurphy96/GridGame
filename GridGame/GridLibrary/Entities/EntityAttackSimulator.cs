using System;
using GridLibrary.Grid;

namespace GridLibrary.Entities;

public static class EntityAttackSimulator
{
    /// <summary>
    /// I could use some other parameter to seed this, but frankly if I get to a point where
    /// I have to worry about players RNG manipulating, I've made it.
    /// </summary>
    private readonly static Random _random = new();
    private const float _critMod = 1.5f;

    /// <summary>
    /// Simulates a single attack. To simulate a back and forth, simply call for the next entity with the parameters reversed.
    /// </summary>
    public static AttackResult Simulate(IEntity attacker, IEntity attacked, TileInfo attackedTileInfo)
    {
        AttackResult attackResult = new (false, false, 0);
        // 1. Check if we hit.
        int actualHitChance = attacker.SelectedMove.HitChance - attacked.DodgeChance - attackedTileInfo.DodgeModifier;
        int hitChanceValue = GetChance();
        if (actualHitChance >= hitChanceValue)
        {
            attackResult.Hit = true;
        }

        // 2. Check if we crit.
        int critChanceValue = GetChance();
        if (attacker.SelectedMove.CritChance >= critChanceValue)
        {
            attackResult.Crit = true;
        }

        // 3. Calculate resulting damage.
        if (attackResult.Hit)
        {
            int damageAfterArmor = attacker.SelectedMove.Damage - attacked.Defense;
            if (attackResult.Crit)
                attackResult.Damage = (int)(damageAfterArmor * _critMod);
            else
                attackResult.Damage = damageAfterArmor;
            if (attackResult.Damage < 0)
                attackResult.Damage = 0;
        }

        return attackResult;
    }

    /// <summary>
    /// Returns a range of 1-100. This is to make sure a
    /// crit chance of 0 never results in a crit, and a crit chance of 100 always does.
    /// </summary>
    /// <returns></returns>
    private static int GetChance()
    {
        return (_random.Next() % 100) + 1;
    }
}
using System.Collections.Generic;
using GridGame.Core.AttackMoves;
using GridLibrary.Entities;
using GridLibrary.Graphics;

namespace GridGame.Core.Entities;

public class Goblin : IEntity
{
    public static string LdtkIdentifier => "Goblin";

    public string DisplayName => "Gobbo";
    public TextureRegion Texture { get; }
    public EntityHealth Health { get; } = new EntityHealth(30);
    public int MovementRange => 5;
    public bool IsFriendly => false;
    public bool IsPlayerControllable => false;

    public IMove DefaultAttack { get; }

    public List<IMove> Moves { get; }

    public Goblin(TextureRegion textureRegion)
    {
        Texture = textureRegion;
        DefaultAttack = new MeleeAttack
        {
            Name = "Gobbo Attack",
            Damage = 5,
            HitChance = 65,
            CritChance = 3
        };
        Moves = [new WaitMove(), DefaultAttack];
    }
}
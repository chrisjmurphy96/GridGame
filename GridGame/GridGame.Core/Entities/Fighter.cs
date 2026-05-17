using System.Collections.Generic;
using GridGame.Core.AttackMoves;
using GridLibrary.Entities;
using GridLibrary.Graphics;

namespace GridGame.Core.Entities;

public class Fighter : IEntity
{
    public static string LdtkIdentifier => "Fighter";
    
    public string DisplayName => "Gigough Chad";
    public TextureRegion Texture { get; }
    public EntityHealth Health { get; } = new EntityHealth(80);
    public int MovementRange => 6;
    public bool IsFriendly => true;
    public bool IsPlayerControllable => true;
    public IMove DefaultAttack { get; }
    public List<IMove> Moves { get; }

    public Fighter(TextureRegion texture)
    {
        Texture = texture;
        DefaultAttack = new MeleeAttack
        {
            Name = "Melee Attack",
            Damage = 8,
            HitChance = 80,
            CritChance = 5
        };
        Moves = [new WaitMove(), DefaultAttack];
    }
}

using GridLibrary.Entities;
using GridLibrary.Graphics;

namespace GridGame.Core.Entities;

public class Goblin : IEntity
{
    public static string LdtkIdentifier => "Goblin";

    public StandardEntityProperties Properties { get; }

    public Goblin(TextureRegion textureRegion)
    {
        Properties = new()
        {
            Texture = textureRegion,
            Health = 30,
            AttackRange = 1,
            MovementRange = 5,
            IsFriendly = false,
            IsPlayerControllable = false
        };
    }
}

using GridLibrary.Entities;
using GridLibrary.Graphics;

namespace GridGame.Core.Entities;

public class Fighter : IEntity
{
    public static string LdtkIdentifier => "Fighter";

    public StandardEntityProperties Properties { get; }

    public Fighter(TextureRegion textureRegion)
    {
        Properties = new()
        {
            Texture = textureRegion,
            Health = 80,
            AttackRange = 1,
            MovementRange = 6,
            IsFriendly = true,
            IsPlayerControllable = true
        };
    }
}

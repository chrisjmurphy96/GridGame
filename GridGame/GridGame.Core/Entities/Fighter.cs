using GridLibrary.Entities;
using GridLibrary.Graphics;

namespace GridGame.Core.Entities;

// the fighter and goblin should probably be implemented in Core instead,
// since they are specific to this game
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

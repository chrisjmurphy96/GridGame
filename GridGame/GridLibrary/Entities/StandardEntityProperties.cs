using GridLibrary.Graphics;

namespace GridLibrary.Entities;

public class StandardEntityProperties
{
    public TextureRegion Texture { get; init; }
    public int Health = 0;
    public int MovementRange = 0;
    public int AttackRange = 0;
    public bool IsFriendly = false;
    public bool IsPlayerControllable = false;
}

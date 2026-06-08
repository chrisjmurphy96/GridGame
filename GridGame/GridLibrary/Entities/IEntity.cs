using System.Collections.Generic;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Entities;

public interface IEntity
{
    static string LdtkIdentifier { get; } = string.Empty;
    public string DisplayName { get; }
    public TextureRegion ActiveTexture { get; }
    public string DodgeAnimationKey { get; set; }
    public EntityHealth Health { get; }
    public int Defense { get; }
    public int DodgeChance { get; }
    public int MovementRange { get; }
    public bool IsFriendly { get; }
    public bool IsPlayerControllable { get; }
    public bool IsVisible { get; set; }
    public bool HasMoved { get; set; }
    public IMove SelectedMove { get; set; }
    public List<IMove> Moves { get; }

    public void SetAnimation(EntityMapAnimationType entityAnimationType);
    public void Update(GameTime game);
}

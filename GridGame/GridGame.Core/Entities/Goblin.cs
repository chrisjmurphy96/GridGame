using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridGame.Core.Entities;

public class Goblin : IEntity
{
    public static string LdtkIdentifier => "Goblin";

    public string DisplayName => "Gobbo";

    private readonly EntityMapAnimations _entityAnimations;

    public TextureRegion ActiveTexture => _entityAnimations.CurrentFrame;
    public string DodgeAnimationKey { get; set; }
    public EntityHealth Health { get; } = new EntityHealth(16);
    public int Defense { get; } = 3;
    public int DodgeChance { get; } = 0;
    public int MovementRange => 5;
    public bool IsFriendly => false;
    public bool IsPlayerControllable => false;
    public bool IsVisible { get; set; } = true;

    public IMove SelectedMove { get; set; }

    public List<IMove> Moves { get; } = [];
    public bool HasMoved { get; set; }

    public Goblin(EntityMapAnimations entityAnimations)
    {
        _entityAnimations = entityAnimations;
    }

    public void SetAnimation(EntityMapAnimationType entityAnimationType)
    {
        _entityAnimations.SetAnimation(entityAnimationType);
    }

    public void Update(GameTime gameTime)
    {
        _entityAnimations.Update(gameTime);
    }
}
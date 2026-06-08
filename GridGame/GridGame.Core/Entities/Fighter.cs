using System.Collections.Generic;
using GridGame.Core.AttackMoves;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridGame.Core.Entities;

public class Fighter : IEntity
{
    public static string LdtkIdentifier => "Fighter";
    
    public string DisplayName => "Gigough Chad";

    private readonly EntityMapAnimations _entityAnimations;

    public TextureRegion ActiveTexture => _entityAnimations.CurrentFrame;
    public string DodgeAnimationKey { get; set; }
    public EntityHealth Health { get; } = new EntityHealth(40);
    public int Defense => 5;
    public int DodgeChance => 0;
    public int MovementRange => 6;
    public bool IsFriendly => true;
    public bool IsPlayerControllable => true;
    public bool IsVisible { get; set; } = true;
    public IMove SelectedMove { get; set; }
    public List<IMove> Moves { get; } = [];
    public bool HasMoved { get; set; }

    public Fighter(EntityMapAnimations entityAnimations)
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

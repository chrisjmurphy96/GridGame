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

    private readonly EntityAnimations _entityAnimations;

    public TextureRegion ActiveTexture => _entityAnimations.CurrentFrame;
    public Animation AttackAnimation => _entityAnimations.Attack;
    public EntityHealth Health { get; } = new EntityHealth(40);
    public int Defense => 5;
    public int DodgeChance => 0;
    public int MovementRange => 6;
    public bool IsFriendly => true;
    public bool IsPlayerControllable => true;
    public bool IsVisible { get; set; } = true;
    public IMove SelectedMove { get; set; }
    public List<IMove> Moves { get; }
    public bool HasMoved { get; set; }

    public Fighter(EntityAnimations entityAnimations)
    {
        _entityAnimations = entityAnimations;
        SelectedMove = new MeleeAttack
        {
            Name = "Melee Attack",
            Damage = 15,
            HitChance = 80,
            CritChance = 5,
            ContactFrame = 5
        };
        Moves = [SelectedMove];
    }

    public void SetAnimation(EntityAnimationType entityAnimationType)
    {
        _entityAnimations.SetAnimation(entityAnimationType);
    }

    public void Update(GameTime gameTime)
    {
        _entityAnimations.Update(gameTime);
    }
}

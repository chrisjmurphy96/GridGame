using GridGame.Core.AttackMoves;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridGame.Core.Entities;

public class Goblin : IEntity
{
    public static string LdtkIdentifier => "Goblin";

    public string DisplayName => "Gobbo";

    private readonly EntityAnimations _entityAnimations;

    public TextureRegion ActiveTexture => _entityAnimations.CurrentFrame;
    public Animation AttackAnimation => _entityAnimations.Attack;
    public EntityHealth Health { get; } = new EntityHealth(16);
    public int Defense { get; } = 3;
    public int MovementRange => 5;
    public bool IsFriendly => false;
    public bool IsPlayerControllable => false;
    public bool IsVisible { get; set; } = true;

    public IMove SelectedMove { get; set; }

    public List<IMove> Moves { get; }
    public bool HasMoved { get; set; }

    public Goblin(EntityAnimations entityAnimations)
    {
        _entityAnimations = entityAnimations;
        SelectedMove = new MeleeAttack
        {
            Name = "Gobbo Attack",
            Damage = 10,
            HitChance = 65,
            CritChance = 3
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
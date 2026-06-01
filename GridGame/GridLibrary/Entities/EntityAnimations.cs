using System.Collections.Generic;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Entities;

public class EntityAnimations
{
    private readonly Dictionary<EntityAnimationType, Animation> _animations;
    private EntityAnimationType _activeAnimation = EntityAnimationType.Idle;
    public TextureRegion CurrentFrame => _animations[_activeAnimation].CurrentFrame;
    public Animation Attack { get; }

    public EntityAnimations(
        Animation idle,
        Animation walkLeft,
        Animation walkRight,
        Animation walkUp,
        Animation walkDown,
        Animation attack)
    {
        _animations = new Dictionary<EntityAnimationType, Animation>()
        {
            { EntityAnimationType.Idle, idle },
            { EntityAnimationType.WalkLeft, walkLeft },
            { EntityAnimationType.WalkRight, walkRight },
            { EntityAnimationType.WalkUp, walkUp },
            { EntityAnimationType.WalkDown, walkDown }
        };
        Attack = attack;
    }

    public void SetAnimation(EntityAnimationType entityAnimationType)
    {
        if (_activeAnimation != entityAnimationType)
        {
            _animations[_activeAnimation].Reset();
        }

        _activeAnimation = entityAnimationType;
    }

    public void Update(GameTime gameTime)
    {
        _animations[_activeAnimation].Update(gameTime);
    }
}

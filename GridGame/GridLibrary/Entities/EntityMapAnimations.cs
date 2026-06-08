using System.Collections.Generic;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Entities;

public class EntityMapAnimations
{
    private readonly Dictionary<EntityMapAnimationType, Animation> _animations;
    private EntityMapAnimationType _activeAnimation = EntityMapAnimationType.Idle;
    public TextureRegion CurrentFrame => _animations[_activeAnimation].CurrentFrame;

    public EntityMapAnimations(
        Animation idle,
        Animation walkLeft,
        Animation walkRight,
        Animation walkUp,
        Animation walkDown)
    {
        _animations = new Dictionary<EntityMapAnimationType, Animation>()
        {
            { EntityMapAnimationType.Idle, idle },
            { EntityMapAnimationType.WalkLeft, walkLeft },
            { EntityMapAnimationType.WalkRight, walkRight },
            { EntityMapAnimationType.WalkUp, walkUp },
            { EntityMapAnimationType.WalkDown, walkDown }
        };
    }

    public void SetAnimation(EntityMapAnimationType entityAnimationType)
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
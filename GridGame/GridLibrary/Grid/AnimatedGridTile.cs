using System;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public abstract class AnimatedGridTile<T>(Point position, Animation animation, T tileType) :
    GridTile<T>(position, animation.CurrentFrame, tileType) where T : Enum
{
    private readonly Animation _animation = animation;

    public override void Update(GameTime gameTime)
    {
        _animation.Update(gameTime);
        Texture = _animation.CurrentFrame;
    }
}
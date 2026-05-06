using System;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class AnimatedGridTile<T>(Point position, T tileType, Animation animation) :
    GridTile<T>(position, animation.CurrentFrame, tileType) where T : Enum
{
    private readonly Animation _animation = animation;

    public override void Update(GameTime gameTime)
    {
        _animation.Update(gameTime);
        Texture = _animation.CurrentFrame;
    }
}
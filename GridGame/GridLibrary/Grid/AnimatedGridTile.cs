using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class AnimatedGridTile(Point position, Animation animation, TileInfo tileInfo) :
    GridTile(position, animation.CurrentFrame, tileInfo)
{
    private readonly Animation _animation = animation;

    public override void Update(GameTime gameTime)
    {
        _animation.Update(gameTime);
        Texture = _animation.CurrentFrame;
    }
}
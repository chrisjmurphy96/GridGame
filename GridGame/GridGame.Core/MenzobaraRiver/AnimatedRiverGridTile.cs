using GridGame.Core.Tiles;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;

namespace GridGame.Core.MenzobaraRiver;

public class AnimatedRiverGridTile(Point position, Animation animation, TileType tileType) : 
    AnimatedGridTile<TileType>(position, animation, tileType)
{
    public override TileInfo GetTileInfo()
    {
        return TileType.GetTileInfo();
    }
}
using GridGame.Core.Tiles;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;

namespace GridGame.Core.MenzobaraRiver;

public class RiverGridTile(Point position, TileType tileType, Animation animation) : AnimatedGridTile<TileType>(position, tileType, animation)
{
    public override TileInfo GetTileInfo()
    {
        return TileType.GetTileInfo();
    }
}
using GridGame.Core.Tiles;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;

namespace GridGame.Core.MenzobaraRiver;

public class RiverGridTile(Point position, TextureRegion texture, TileType tileType) :
    GridTile<TileType>(position, texture, tileType)
{
    public override TileInfo GetTileInfo() => TileType.GetTileInfo();
}
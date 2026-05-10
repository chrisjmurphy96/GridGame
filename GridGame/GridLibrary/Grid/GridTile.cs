using System;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public abstract class GridTile<T>(Point position, TextureRegion texture, T tileType) :
    BaseGridTile(position, texture) where T : Enum
{
    public T TileType { get; } = tileType;
}

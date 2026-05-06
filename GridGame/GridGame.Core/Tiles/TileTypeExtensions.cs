using System;
using GridLibrary.Grid;

namespace GridGame.Core.Tiles;

public static class TileTypeExtensions
{
    public static TileInfo GetTileInfo(this TileType tileType)
    {
        return tileType switch
        {
            TileType.Forest => new TileInfo { DodgeModifier = 20 },
            TileType.River => new TileInfo { CanWalk = false },
            TileType.Bridge => new TileInfo(),
            TileType.Grass => new TileInfo(),
            _ => throw new NotImplementedException(),
        };
    }
}
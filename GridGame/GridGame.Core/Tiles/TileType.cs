namespace GridGame.Core.Tiles;

/// <summary>
/// LDTK automatically assings ids to tiles based on position in the atlas.
/// This is to let us map back from those ids to meaningful info.
/// </summary>
public enum TileType
{
    Forest = 0,
    River = 1,
    Bridge = 4,
    Grass = 5
}
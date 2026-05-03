// using System.Drawing;
// using System.Security.Cryptography.X509Certificates;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class GridTile
{
    public Point Position = Point.Zero;
    // Placeholder till I have a real entity concept to work with
    public string Entity = string.Empty;
    public required TextureRegion Texture { get; init; }
    public required int TileType { get; init; }
}
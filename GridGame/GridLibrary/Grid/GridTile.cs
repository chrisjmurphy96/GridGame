using System;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class GridTile<T>(Point position, T tileType, TextureRegion texture) where T : Enum
{
    public Point Position = position;
    // Placeholder till I have a real entity concept to work with
    public string Entity = string.Empty;
    public TextureRegion Texture { get; protected set; } = texture;
    public T TileType { get; } = tileType;
    
    public virtual void Update(GameTime gameTime) { }
}
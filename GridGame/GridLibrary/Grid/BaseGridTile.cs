using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public abstract class BaseGridTile(Point position, TextureRegion texture)
{
    public Point Position = position;
    // Placeholder till I have a real entity concept to work with
    public string Entity = string.Empty;
    public TextureRegion Texture { get; protected set; } = texture;
    public abstract TileInfo GetTileInfo();
    
    public virtual void Update(GameTime gameTime) { }
}
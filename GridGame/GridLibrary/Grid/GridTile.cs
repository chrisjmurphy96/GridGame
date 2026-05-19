using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;


public class GridTile(Point position, TextureRegion texture, TileInfo tileInfo)
{
    public Point Position = position;
    public TextureRegion Texture { get; protected set; } = texture;
    public TileInfo TileInfo { get; } = tileInfo;
    
    public virtual void Update(GameTime gameTime) { }
}
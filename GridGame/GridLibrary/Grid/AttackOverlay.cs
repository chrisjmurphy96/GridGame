using System.Collections.Generic;
using GridLibrary.Graphics;
using GridLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

/// <summary>
/// Gets positions for "blue" grid of walkable tiles.
/// Gets positions for "red" tiles indicating attack range.
/// </summary>
public class AttackOverlay : UIElement
{
    public required Sprite AttackOverlaySprite { get; init; }
    public HashSet<Point> AttackPoints { get; set; } = [];
    
    public void Show(int attackRange, Point entityPosition, GridTileList gridTiles)
    {
        // Dijkstra search the space from start for reachable nodes.
        AttackPoints = Dijkstra.GetAttackable(attackRange, [entityPosition], gridTiles);
        SetIsVisible(true);
    }

    public void Hide()
    {
        AttackPoints.Clear();
        SetIsVisible(false);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        // These are square tiles, Width can be used as our size. This only works so long as
        // the move overlay texture is the same size as the grid tiles
        float spriteSize = AttackOverlaySprite.TextureRegion.Width;
        foreach(Point point in AttackPoints)
        {
            Vector2 position = point.ToVector2() * AttackOverlaySprite.Scale * spriteSize;
            spriteBatch.Draw(
                    AttackOverlaySprite,
                    position,
                    opaqueness: 0.85f);
        }
    }
}
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public static class SpriteBatchExtensions
{
    private static Color Inactive = new(100, 100, 100);

    public static void Draw(
        this SpriteBatch spriteBatch,
        Dictionary<Point, IEntity> entities,
        int scalar,
        SpriteFont? debugFont = null)
    {
        Vector2 scale = Vector2.One * scalar;
        foreach((Point position, IEntity entity) in entities)
        {
            if (!entity.IsVisible)
                continue;

            Color color = entity.HasMoved ? Inactive : Color.White;
            int spriteSize = entity.ActiveTexture.Width;
            Vector2 positionVector = position.ToVector2() * scalar * spriteSize;
            spriteBatch.Draw(
                textureRegion: entity.ActiveTexture,
                positionVector,
                color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                SpriteEffects.None,
                layerDepth: LayerDepths.Entities);
            
            if (debugFont is not null)
            {
                Vector2 fontPosition = positionVector - new Vector2(0, debugFont.LineSpacing);
                spriteBatch.DrawString(debugFont, entity.Health.ToString(), fontPosition, Color.Black);
            }
        }
    }

    public static void DrawEnemyAttackPoints(
        this SpriteBatch spriteBatch,
        HashSet<Point> enemyAttackPoints,
        TextureRegion movementTexture,
        int scalar)
    {
        if (enemyAttackPoints.Count is 0)
            return;

        Vector2 scale = Vector2.One * scalar;
        int spriteSize = movementTexture.Width;
        foreach (Point point in enemyAttackPoints)
        {
            Vector2 position = point.ToVector2() * scalar * spriteSize;
            spriteBatch.Draw(
                    textureRegion: movementTexture,
                    position,
                    Color.White * 0.5f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: LayerDepths.MoveOverlay);
        }
    }

    public static void Draw(this SpriteBatch spriteBatch, MoveOverlay moveOverlay, int scalar)
    {
        if (!moveOverlay.IsVisible)
            return;

        Vector2 scale = Vector2.One * scalar;
        // These are square tiles, Width can be used as our size. This only works so long as
        // the move overlay texture is the same size as the grid tiles
        int spriteSize = moveOverlay.MovementTexture.Width;
        foreach(Point point in moveOverlay.MovementPoints)
        {
            Vector2 position = point.ToVector2() * scalar * spriteSize;
            spriteBatch.Draw(
                    textureRegion: moveOverlay.MovementTexture,
                    position,
                    Color.White * 0.85f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: LayerDepths.MoveOverlay);
        }

        foreach(Point point in moveOverlay.AttackPoints)
        {
            Vector2 position = point.ToVector2() * scalar * spriteSize;
            spriteBatch.Draw(
                    textureRegion: moveOverlay.AttackTexture,
                    position,
                    Color.White * 0.85f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: LayerDepths.MoveOverlay);
        }
    }

    public static void Draw(this SpriteBatch spriteBatch, Cursor cursor)
    {
        if (cursor.IsVisible)
            spriteBatch.Draw(cursor.CursorSprite, cursor.Position);
    }

    /// <summary>
    /// Submit an <see cref="AnimatedSprite"> for drawing in the current batch.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch instance used for batching draw calls.</param>
    /// <param name="animatedSprite"></param>
    /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
    public static void Draw(this SpriteBatch spriteBatch, AnimatedSprite animatedSprite, Vector2 position)
    {
        spriteBatch.Draw(
            animatedSprite.CurrentFrame,
            position,
            animatedSprite.Color,
            animatedSprite.Rotation,
            animatedSprite.Origin, 
            animatedSprite.Scale, 
            animatedSprite.Effects,
            animatedSprite.LayerDepth);
    }

    /// <summary>
    /// Submit a <see cref="Sprite"> for drawing in the current batch.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch instance used for batching draw calls.</param>
    /// <param name="sprite"></param>
    /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
    public static void Draw(this SpriteBatch spriteBatch, Sprite sprite, Vector2 position, float opaqueness = 1)
    {
        spriteBatch.Draw(
            sprite.TextureRegion,
            position,
            sprite.Color * opaqueness,
            sprite.Rotation,
            sprite.Origin, 
            sprite.Scale, 
            sprite.Effects,
            sprite.LayerDepth);
    }

    /// <summary>
    /// Submit this texture region for drawing in the current batch.
    /// </summary>
    /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
    /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
    /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
    /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
    /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
    /// <param name="scale">The amount of scaling to apply to the x- and y-axes when drawing this texture region on screen.</param>
    /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
    /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
    public static void Draw(
        this SpriteBatch spriteBatch,
        TextureRegion textureRegion,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        spriteBatch.Draw(
            textureRegion.Texture,
            position,
            textureRegion.SourceRectangle,
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth
        );
    }
}
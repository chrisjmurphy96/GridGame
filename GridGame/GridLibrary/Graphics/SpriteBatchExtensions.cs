using GridLibrary.Graphics;
using GridLibrary.Grid;

namespace Microsoft.Xna.Framework.Graphics;

public static class SpriteBatchExtensions
{
    public static void Draw(this SpriteBatch spriteBatch, Grid grid, bool drawGridOverlay = false)
    {
        // _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        Vector2 scale = Vector2.One * grid.Scalar;
        foreach(GridTile gridTile in grid.Tiles)
        {
            Vector2 position = gridTile.Position.ToVector2() * grid.Scalar;
            spriteBatch.Draw(
                texture: grid.MapAtlas,
                position,
                gridTile.Texture.SourceRectangle,
                Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                SpriteEffects.None,
                layerDepth: 1.0f);

            if (drawGridOverlay)
            {
                spriteBatch.Draw(
                    texture: grid.GridOverlay.Texture,
                    position,
                    grid.GridOverlay.SourceRectangle,
                    Color.White * 0.25f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: 1.0f);
            }
        }

        spriteBatch.Draw(grid.Cursor);
    }

    public static void Draw(this SpriteBatch spriteBatch, Cursor cursor)
    {
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
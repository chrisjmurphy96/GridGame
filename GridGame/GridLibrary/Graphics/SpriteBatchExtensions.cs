using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Graphics;
using GridLibrary.Grid;

namespace Microsoft.Xna.Framework.Graphics;

public static class SpriteBatchExtensions
{
    public static void Draw<T>(this SpriteBatch spriteBatch, Grid<T> grid, bool drawGridOverlay = false) where T : struct, Enum
    {
        Vector2 scale = Vector2.One * grid.Scalar;
        foreach(GridTile<T> gridTile in grid.Tiles)
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
                    textureRegion: grid.GridOverlay,
                    position,
                    Color.White * 0.25f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: 1.0f);
            }
        }
        spriteBatch.Draw(grid.MovementArrow, grid.Scalar, grid.TileSize);
        spriteBatch.Draw(grid.Cursor);
    }

    public static class Direction
    {
        public static float Up = Single.DegreesToRadians(0);
        public static float Down = Single.DegreesToRadians(180);
        public static float Left = Single.DegreesToRadians(270);
        public static float Right = Single.DegreesToRadians(90);
    }

    /// <summary>
    /// TODO: This is...a lot. Please, for my own sake, factor this out.
    /// </summary>
    public static void Draw<T>(this SpriteBatch spriteBatch, MovementArrow<T> movementArrow, int scalar, int tileSize) where T : struct, Enum
    {
        // Since we have to center the origin for nice rotation, we need an offset
        // to re-center the sprite on the grid.
        Vector2 positionOffset = Vector2.One * 0.5f * scalar * tileSize;
        Vector2 GetPosition(Point point) => (point.ToVector2() * scalar * tileSize) + positionOffset;
        float GetRotation(Point current, Point next)
        {
            if (next.IsAbove(current))
            {
                return Direction.Up;
            }
            else if (next.IsBelow(current))
            {
                return Direction.Down;
            }
            else if (next.IsToTheLeftOf(current))
            {
                return Direction.Left;
            }
            else if (next.IsToTheRightOf(current))
            {
                return Direction.Right;
            }

            throw new ArgumentException("Did you pass in the same point twice?");
        }
        void Draw(TextureRegion texture, Point point, float rotation)
        {
            Vector2 position = GetPosition(point);
            Vector2 textureCenter = texture.GetCenter();
            spriteBatch.Draw(
                textureRegion: texture,
                position: position,
                color: Color.White,
                rotation: rotation,
                origin: textureCenter,
                scale: Vector2.One * scalar,
                effects: SpriteEffects.None,
                layerDepth: 1.0f);
        }

        if (!movementArrow.IsVisible)
            return;
        if (movementArrow.Path.Count is 1)
        {
            Draw(movementArrow.HeadTexture, movementArrow.Path.First(), rotation: Direction.Up);
            return;
        }

        // TODO: rotation
        Point startPoint = movementArrow.Path[0];
        float startRotation = GetRotation(startPoint, movementArrow.Path[1]);
        Draw(movementArrow.StartTexture, startPoint, startRotation);

        List<Point> path = movementArrow.Path;
        float previousDirection = startRotation;
        for (int i = 1; i < movementArrow.Path.Count - 1; i++)
        {
            Point current = path[i];
            Point next = path[i + 1];
            float direction = GetRotation(current, next);

            TextureRegion texture = movementArrow.StraightTexture;
            if (previousDirection != direction)
            {
                texture = movementArrow.BendTexture;
                Point previous = path[i - 1];
                float rotation = Direction.Up;
                bool upRight = previous.IsBelow(current) && current.IsToTheLeftOf(next);
                bool upLeft = previous.IsBelow(current) && current.IsToTheRightOf(next);
                bool downRight = previous.IsAbove(current) && current.IsToTheLeftOf(next);
                bool downLeft = previous.IsAbove(current) && current.IsToTheRightOf(next);
                bool rightUp = previous.IsToTheLeftOf(current) && current.IsBelow(next);
                bool rightDown = previous.IsToTheLeftOf(current) && current.IsAbove(next);
                bool leftUp = previous.IsToTheRightOf(current) && current.IsBelow(next);
                bool leftDown = previous.IsToTheRightOf(current) && current.IsAbove(next);
                if (upRight || leftDown)
                    rotation = 0f;
                else if (upLeft || rightDown)
                    rotation = Single.DegreesToRadians(90);
                else if (downRight || leftUp)
                    rotation = Single.DegreesToRadians(270);
                else if (downLeft || rightUp)
                    rotation = Single.DegreesToRadians(180);
                Draw(texture, current, rotation);
            }
            else
            {
                Draw(texture, current, rotation: direction);
            }
            previousDirection = direction;
        }

        Point headPoint = movementArrow.Path.Last();
        float headRotation = GetRotation(movementArrow.Path[^2], headPoint);
        Draw(movementArrow.HeadTexture, movementArrow.Path.Last(), headRotation);  
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
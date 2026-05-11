using System;
using System.Collections.Generic;
using System.Linq;
using GridLibrary.Graphics;
using GridLibrary.Grid;

namespace Microsoft.Xna.Framework.Graphics;

public static class MovementArrowSpriteBatchExtensions
{
    private static class ArrowDirection
    {
        public static float Up = Single.DegreesToRadians(0);
        public static float Down = Single.DegreesToRadians(180);
        public static float Left = Single.DegreesToRadians(270);
        public static float Right = Single.DegreesToRadians(90);
    }

    private static class BendDirection
    {
        public static float UpRightOrLeftDown = Single.DegreesToRadians(0);
        public static float UpLeftOrRightDown = Single.DegreesToRadians(90);
        public static float DownRightOrLeftUp = Single.DegreesToRadians(270);
        public static float DownLeftOrRightUp = Single.DegreesToRadians(180);
    }

    public static void Draw<T>(this SpriteBatch spriteBatch, MovementArrow<T> movementArrow, int scalar, int tileSize) where T : struct, Enum
    {
        // Since we have to center the origin for nice rotation, we need an offset
        // to re-center the sprite on the grid.
        Vector2 positionOffset = Vector2.One * 0.5f * scalar * tileSize;
        if (!movementArrow.IsVisible)
            return;
        if (movementArrow.Path.Count is 1)
        {
            spriteBatch.Draw(movementArrow.HeadTexture, movementArrow.Path.First(), rotation: ArrowDirection.Up, scalar, tileSize, positionOffset);
            return;
        }

        Point startPoint = movementArrow.Path[0];
        float startRotation = GetRotation(startPoint, movementArrow.Path[1]);
        spriteBatch.Draw(movementArrow.StartTexture, startPoint, startRotation, scalar, tileSize, positionOffset);

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
                spriteBatch.DrawBend(movementArrow.BendTexture, path[i - 1], current, next, scalar, tileSize, positionOffset);
            }
            else
            {
                spriteBatch.Draw(texture, current, rotation: direction, scalar, tileSize, positionOffset);
            }
            previousDirection = direction;
        }

        Point headPoint = movementArrow.Path.Last();
        float headRotation = GetRotation(movementArrow.Path[^2], headPoint);
        spriteBatch.Draw(movementArrow.HeadTexture, headPoint, headRotation, scalar, tileSize, positionOffset);
    }

    private static void DrawBend(
        this SpriteBatch spriteBatch,
        TextureRegion texture,
        Point previous,
        Point current,
        Point next,
        int scalar,
        int tileSize,
        Vector2 positionOffset)
    {
        float rotation = BendDirection.UpRightOrLeftDown;
        bool upLeft = previous.IsBelow(current) && current.IsToTheRightOf(next);
        bool downRight = previous.IsAbove(current) && current.IsToTheLeftOf(next);
        bool downLeft = previous.IsAbove(current) && current.IsToTheRightOf(next);
        bool rightUp = previous.IsToTheLeftOf(current) && current.IsBelow(next);
        bool rightDown = previous.IsToTheLeftOf(current) && current.IsAbove(next);
        bool leftUp = previous.IsToTheRightOf(current) && current.IsBelow(next);

        if (upLeft || rightDown)
            rotation = BendDirection.UpLeftOrRightDown;
        else if (downRight || leftUp)
            rotation = BendDirection.DownRightOrLeftUp;
        else if (downLeft || rightUp)
            rotation = BendDirection.DownLeftOrRightUp;
        spriteBatch.Draw(texture, current, rotation, scalar, tileSize, positionOffset);
    }

    private static void Draw(
        this SpriteBatch spriteBatch,
        TextureRegion texture,
        Point point,
        float rotation,
        int scalar,
        int tileSize,
        Vector2 positionOffset)
    {
        Vector2 position = GetPosition(point, scalar, tileSize, positionOffset);
        Vector2 textureCenter = texture.GetCenter();
        spriteBatch.Draw(
            textureRegion: texture,
            position: position,
            color: Color.White * 0.85f,
            rotation: rotation,
            origin: textureCenter,
            scale: Vector2.One * scalar,
            effects: SpriteEffects.None,
            layerDepth: 1.0f);
    }

    private static Vector2 GetPosition(Point point, int scalar, int tileSize, Vector2 positionOffset) => (point.ToVector2() * scalar * tileSize) + positionOffset;

    private static float GetRotation(Point current, Point next)
    {
        if (next.IsAbove(current))
        {
            return ArrowDirection.Up;
        }
        else if (next.IsBelow(current))
        {
            return ArrowDirection.Down;
        }
        else if (next.IsToTheLeftOf(current))
        {
            return ArrowDirection.Left;
        }
        // only other option is right. Guaranteed in a grid.
        return ArrowDirection.Right;
    }
}
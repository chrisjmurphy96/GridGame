using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Input;
using GridLibrary.Routing;
using GridLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridLibrary.Grid;

public class MovementArrow : UIElement, IRouteableElement
{
    public Point StartPosition { get; private set; } = Point.Zero;
    public List<Point> Path { get; private set; } = [];
    private int _maxMovement = 0;
    private GridTileList _tiles = new(0);
    private readonly Cursor _cursor;
    private readonly MoveOverlay _moveOverlay;

    public required TextureRegion HeadTexture { get; init; }
    public required TextureRegion StraightTexture { get; init; }
    public required TextureRegion BendTexture { get; init; }
    public required TextureRegion StartTexture { get; init; }


    private readonly static TimeSpan MOVE_INPUT_DELAY = TimeSpan.FromMilliseconds(100);

    public MovementArrow(Cursor cursor, MoveOverlay moveOverlay)
    {
        _cursor = cursor;
        _moveOverlay = moveOverlay;
    }

    public void Initialize()
    {
        // We might be coming back from cancelling a move, so make sure the onscreen position
        // is correct.
        GridState.ResetActiveEntityPosition();
        (Point position, IEntity entity) = 
            GridState.Instance.ActiveEntity ?? throw new ArgumentException($"No {nameof(GridState.Instance.ActiveEntity)}");
        _cursor.Show();
        entity.IsVisible = true;

        StartPosition = position;
        Path = [StartPosition];
        _maxMovement = entity.MovementRange;
        _tiles = GridState.Instance.Tiles;
        _moveOverlay.Show(_maxMovement, entity.SelectedMove.Range, StartPosition, _tiles, GridState.Instance.Entities);
        Update(GridState.Instance.CursorPosition, _moveOverlay.MovementPoints);
    }

    public void AfterInitialize() { }

    public void Cancel()
    {
        StartPosition = Point.Zero;
        Path = [];
        _maxMovement = 0;
    }

    /// <summary>
    /// There's a lot of shared code here, possible some stuff could be moved into the cursor itself
    /// since both this and <see cref="TileGrid" /> reference it.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputInfo inputInfo)
    {
        if (inputInfo.DownPressed() ||
            inputInfo.DownHeld(MOVE_INPUT_DELAY))
        {
            inputInfo.ResetDownHold();
            MoveCursorDown();
        }
        if (inputInfo.UpPressed() ||
            inputInfo.UpHeld(MOVE_INPUT_DELAY))
        {
            inputInfo.ResetUpHold();
            MoveCursorUp();
        }
        if (inputInfo.RightPressed() ||
            inputInfo.RightHeld(MOVE_INPUT_DELAY))
        {
            inputInfo.ResetRightHold();
            MoveCursorRight();
        }
        if (inputInfo.LeftPressed() ||
            inputInfo.LeftHeld(MOVE_INPUT_DELAY))
        {
            inputInfo.ResetLeftHold();
            MoveCursorLeft();
        }
        if (inputInfo.SelectPressed())
        {
            CursorClick();
        }
        if (inputInfo.CancelPressed())
        {
            Router.Back();
        }
        // TODO: some keys should probably still work even when
        // something isn't focused. How do I make an exception for these?
        // Maybe they should just be their own class and not a UI element?
        // if (keyboardInfo.WasKeyJustPressed(Keys.O))
        // {
        //     ToggleEnemyOverlay();
        // }
    }

    public void Update(Point end, HashSet<Point> walkableSpace)
    {
        Search(end, walkableSpace);
    }
    
    private void Search(Point end, HashSet<Point> walkableSpace)
    {
        if (StartPosition == end)
            Path = [StartPosition];

        GridTile endTile = _tiles[end];
        if (!endTile.TileInfo.CanWalk)
            return;
        
        // skip updating if we're past max range
        if (StartPosition.DistanceTo(end) > _maxMovement)
            return;

        // if we haven't moved, skip updating
        if (Path.Last() == end)
            return;

        Path = Dijkstra.FindShortestPath(StartPosition, end, _maxMovement, walkableSpace) ?? Path;
    }

    public void MoveCursorUp()
    {
        if (GridState.Instance.CursorPosition.Y > 0)
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Up();
            _cursor.MoveUp();
            Update(GridState.Instance.CursorPosition, _moveOverlay.MovementPoints);
        }
    }

    public void MoveCursorDown()
    {
        if (GridState.Instance.CursorPosition.Y < (_tiles.Rows - 1))
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Down();
            _cursor.MoveDown();
            Update(GridState.Instance.CursorPosition, _moveOverlay.MovementPoints);
        }
    }

    public void MoveCursorRight()
    {
        if (GridState.Instance.CursorPosition.X < (_tiles.Columns - 1))
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Right();
            _cursor.MoveRight();
            Update(GridState.Instance.CursorPosition, _moveOverlay.MovementPoints);
        }
    }

    public void MoveCursorLeft()
    {
        if (GridState.Instance.CursorPosition.X > 0)
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Left();
            _cursor.MoveLeft();
            Update(GridState.Instance.CursorPosition, _moveOverlay.MovementPoints);
        }
    }

    public void CursorClick()
    {
        if (!_moveOverlay.MovementPoints.Contains(GridState.Instance.CursorPosition))
            return;
        MovementState.Path = Path;
        Router.RouteWithoutHistory(DefaultRoutes.MovementAnimation);
    }

    private static class ArrowDirection
    {
        public static float Up = float.DegreesToRadians(0);
        public static float Down = float.DegreesToRadians(180);
        public static float Left = float.DegreesToRadians(270);
        public static float Right = float.DegreesToRadians(90);
    }

    private static class BendDirection
    {
        public static float UpRightOrLeftDown = float.DegreesToRadians(0);
        public static float UpLeftOrRightDown = float.DegreesToRadians(90);
        public static float DownRightOrLeftUp = float.DegreesToRadians(270);
        public static float DownLeftOrRightUp = float.DegreesToRadians(180);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        // TODO: Don't you dare leave this future me. This is purely a placeholder
        // while in the dire straits of a major rewrite
        int scalar = 4;
        spriteBatch.Draw(_moveOverlay, 4);
        base.Draw(spriteBatch, parentBounds);

        // Since we have to center the origin for nice rotation, we need an offset
        // to re-center the sprite on the grid.
        int spriteSize = HeadTexture.Width;
        Vector2 positionOffset = Vector2.One * 0.5f * scalar * spriteSize;
        if (!IsVisible)
            return;
        if (Path.Count is 1)
        {
            Draw(spriteBatch, HeadTexture, Path.First(), rotation: ArrowDirection.Up, scalar, spriteSize, positionOffset);
            return;
        }

        Point startPoint = Path[0];
        float startRotation = GetRotation(startPoint, Path[1]);
        Draw(spriteBatch, StartTexture, startPoint, startRotation, scalar, spriteSize, positionOffset);

        List<Point> path = Path;
        float previousDirection = startRotation;
        for (int i = 1; i < Path.Count - 1; i++)
        {
            Point current = path[i];
            Point next = path[i + 1];
            float direction = GetRotation(current, next);

            TextureRegion texture = StraightTexture;
            if (previousDirection != direction)
            {
                DrawBend(spriteBatch, BendTexture, path[i - 1], current, next, scalar, spriteSize, positionOffset);
            }
            else
            {
                Draw(spriteBatch, texture, current, rotation: direction, scalar, spriteSize, positionOffset);
            }
            previousDirection = direction;
        }

        Point headPoint = Path.Last();
        float headRotation = GetRotation(Path[^2], headPoint);
        Draw(spriteBatch, HeadTexture, headPoint, headRotation, scalar, spriteSize, positionOffset);
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
        else if (next.IsLeftOf(current))
        {
            return ArrowDirection.Left;
        }
        // only other option is right. Guaranteed in a grid.
        return ArrowDirection.Right;
    }

    private void DrawBend(
        SpriteBatch spriteBatch,
        TextureRegion texture,
        Point previous,
        Point current,
        Point next,
        int scalar,
        int tileSize,
        Vector2 positionOffset)
    {
        float rotation = BendDirection.UpRightOrLeftDown;
        bool upLeft = previous.IsBelow(current) && current.IsRightOf(next);
        bool downRight = previous.IsAbove(current) && current.IsLeftOf(next);
        bool downLeft = previous.IsAbove(current) && current.IsRightOf(next);
        bool rightUp = previous.IsLeftOf(current) && current.IsBelow(next);
        bool rightDown = previous.IsLeftOf(current) && current.IsAbove(next);
        bool leftUp = previous.IsRightOf(current) && current.IsBelow(next);

        if (upLeft || rightDown)
            rotation = BendDirection.UpLeftOrRightDown;
        else if (downRight || leftUp)
            rotation = BendDirection.DownRightOrLeftUp;
        else if (downLeft || rightUp)
            rotation = BendDirection.DownLeftOrRightUp;
        Draw(spriteBatch, texture, current, rotation, scalar, tileSize, positionOffset);
    }

    private void Draw(
        SpriteBatch spriteBatch,
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
            layerDepth: LayerDepths.MovementArrow);
    }
}

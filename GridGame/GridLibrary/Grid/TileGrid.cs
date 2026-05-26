using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Input;
using GridLibrary.Routing;
using GridLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridLibrary.Grid;

/// <summary>
/// I am heavily considering making the grid a UI element.
/// Only problem is, a lot of the default UI element stuff
/// doesn't really make sense. The only real benefit I'd get
/// is some code boiler plate reduction and proper IsFocused logic.
/// </summary>
public class TileGrid : UIElement, IRouteableElement
{
    public TextureRegion GridOverlay { get; }
    public int Scalar { get; }
    public Cursor Cursor { get; }
    public MovementArrow MovementArrow { get; }
    public MoveOverlay MoveOverlay { get; }
    public TextureRegion EnemyMoveOverlayTexture { get; }

    private readonly SpriteFont? _debugFont;

    private readonly static TimeSpan MOVE_DELAY = TimeSpan.FromMilliseconds(100);
    public List<MoveOverlay> EnemyMoveOverlays { get; } = [];
    public HashSet<Point> EnemyAttackPoints { get; } = [];
    public EventHandler? Next { get; set; } = null;
    public EventHandler? Previous { get; set; } = null;
    
    public TileGrid(
        TextureRegion gridOverlayTexture,
        int scalar,
        Cursor cursor,
        MovementArrow movementArrow,
        MoveOverlay moveOverlay,
        TextureRegion enemyMoveOverlayTexture,
        SpriteFont? debugFont)
    {
        GridOverlay = gridOverlayTexture;
        Scalar = scalar;
        Cursor = cursor;
        MovementArrow = movementArrow;
        MoveOverlay = moveOverlay;
        EnemyMoveOverlayTexture = enemyMoveOverlayTexture;
        _debugFont = debugFont;
    }

    public void Initialize() { }

    public override void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
    {
        GridState.UnsetActiveEntity();
        
        if (keyboardInfo.WasKeyJustPressed(Keys.Down) ||
            keyboardInfo.IsKeyHeldDown(Keys.Down, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Down);
            MoveCursorDown();
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Up) ||
            keyboardInfo.IsKeyHeldDown(Keys.Up, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Up);
            MoveCursorUp();
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Right) ||
            keyboardInfo.IsKeyHeldDown(Keys.Right, MOVE_DELAY))
        {   
            keyboardInfo.ResetKeyHold(Keys.Right);
            MoveCursorRight();
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Left) ||
            keyboardInfo.IsKeyHeldDown(Keys.Left, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Left);
            MoveCursorLeft();
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            CursorClick(gameTime);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            // I guess we could play a silly noise, but there's nothing to do?
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.O))
        {
            ToggleEnemyOverlay();
        }
    }

    public void Update(GameTime gameTime)
    {
        Cursor.Update(gameTime);
        foreach (GridTile gridTile in GridState.Instance.Tiles)
            gridTile.Update(gameTime);
        UpdateEnemyOverlay();
    }

    public bool ShowEnemyOverlay { get; private set; } = false;

    private void ToggleEnemyOverlay()
    {
        ShowEnemyOverlay = !ShowEnemyOverlay;
    }

    /// <summary>
    /// This could probably be its own class, but for now just letting it live here.
    /// The available attack points can change as friendly units move, so this needs to
    /// be recalculated each update cycle.
    /// </summary>
    private void UpdateEnemyOverlay()
    {
        EnemyAttackPoints.Clear();
        if (!ShowEnemyOverlay)
        {
            return;
        }

        Dictionary<Point, IEntity> entities = GridState.Instance.Entities;
        foreach((Point entityPosition, IEntity entity) in entities)
        {
            if (entity.IsFriendly)
                continue;

            // TODO: Could add clicking a singular enemy entity shows just their range.
            GridTileList tiles = GridState.Instance.Tiles;
            HashSet<Point> walkable = Dijkstra.GetWalkable(entityPosition, entity.MovementRange, tiles, entities, forEnemy: true);
            HashSet<Point> attackable = Dijkstra.GetAttackable(entity.SelectedMove.Range, walkable, tiles);
            EnemyAttackPoints.UnionWith(walkable);
            EnemyAttackPoints.UnionWith(attackable);
        }
    }

    public void MoveCursorUp()
    {
        if (GridState.Instance.CursorPosition.Y > 0)
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Up();
            Cursor.MoveUp();
        }
    }

    public void MoveCursorDown()
    {
        if (GridState.Instance.CursorPosition.Y < (GridState.Instance.Tiles.Rows - 1))
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Down();
            Cursor.MoveDown();
        }
    }

    public void MoveCursorRight()
    {
        if (GridState.Instance.CursorPosition.X < (GridState.Instance.Tiles.Columns - 1))
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Right();
            Cursor.MoveRight();
        }
    }

    public void MoveCursorLeft()
    {
        if (GridState.Instance.CursorPosition.X > 0)
        {
            GridState.Instance.CursorPosition = GridState.Instance.CursorPosition.Left();
            Cursor.MoveLeft();
        }
    }

    /// <summary>
    /// - If context menu is visible, do nothing. They need to cancel it or choose a move.
    /// - If the player has chosen where to move a character, show the context menu.
    /// - If the player has selected a controllable character, start the movement action.
    /// </summary>
    public void CursorClick(GameTime gameTime)
    {
        GridState.Instance.Entities.TryGetValue(GridState.Instance.CursorPosition, out IEntity? entity);
        if (entity is null ||
            !entity.IsPlayerControllable)
        {
            return;
        }
        GridState.SetActiveEntity();
        Router.RouteTo(DefaultRoutes.MovementArrow);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        Vector2 scale = Vector2.One * Scalar;
        foreach(GridTile gridTile in GridState.Instance.Tiles)
        {
            Vector2 position = gridTile.Position.ToVector2() * Scalar;
            spriteBatch.Draw(
                textureRegion: gridTile.Texture,
                position: position,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: LayerDepths.Tiles);
        }
        spriteBatch.DrawEnemyAttackPoints(EnemyAttackPoints, EnemyMoveOverlayTexture, Scalar);
        spriteBatch.Draw(GridState.Instance.Entities, Scalar, _debugFont);
        spriteBatch.Draw(Cursor);

        base.Draw(spriteBatch, parentBounds);
    }
}
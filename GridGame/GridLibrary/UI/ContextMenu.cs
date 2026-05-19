using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridLibrary.UI;

/// <summary>
/// First priorities:
/// - Menu positioning
/// - Move text positioning
/// - Binding. How do we get back a move to perform for the entity?
/// - Melee attacks first
/// - Ranged attacks second
/// - Fancy magic last
/// 
/// Nice to haves:
/// - Scroll logic in the menu
/// - Secondary menu popouts
/// </summary>
public class ContextMenu : UIElement
{
    public required TextureRegion FocusTexture { get; init; }
    public required SpriteFont Font { get; init; }
    public required KeyboardInfo KeyboardInfo { get; init; }
    public required MovePreview MovePreview { get; init; }

    private IEntity? _performer;
    private Point? _cursorPosition;

    public List<IMove> Moves => _performer?.Moves ?? [];
    public int FocusIndex { get; private set; } = 0;
    private EventHandler? _selectMove;
    private TimeSpan _showTime = TimeSpan.Zero;
    private Action _hideMoveOverlay;
    private Action _showMoveOverlay;
    private static readonly TimeSpan MENU_INTERACTION_DELAY = TimeSpan.FromMilliseconds(100);
    public bool PreviewingMove { get; private set; } = false;
    private readonly Dictionary<string, HashSet<Point>> _attackNameToAttackPoints = [];

    public void Open(
        IEntity performer,
        Point cursorPosition,
        GameTime gameTime,
        Action moveSelectCallback,
        GridTileList gridTiles,
        Action showMoveOverlay,
        Action hideMoveOverlay)
    {
        _performer = performer;
        _cursorPosition = cursorPosition;
        SetIsVisible<ContextMenu>(true);
        _showTime = gameTime.TotalGameTime;
        _selectMove += (_, _) => moveSelectCallback();
        _showMoveOverlay = showMoveOverlay;
        _hideMoveOverlay = hideMoveOverlay;

        foreach (IMove move in _performer.Moves)
        {
            HashSet<Point> attackPoints = [];
            if (move.Range > 0)
                attackPoints = Dijkstra.GetAttackable(move.Range, [_cursorPosition.Value], gridTiles);
            _attackNameToAttackPoints.Add(move.Name, attackPoints);
        }
    }

    /// <summary>
    /// Automatically unsubscribes SelectMove events
    /// </summary>
    public void Close()
    {
        _performer = null;
        _cursorPosition = null;
        SetIsVisible<ContextMenu>(false);
        UIRoot.Unfocus();
        FocusIndex = 0;
        _showTime = TimeSpan.Zero;
        _selectMove = null;
        _attackNameToAttackPoints.Clear();
    }

    public override void HandleInput(KeyboardInfo keyboardInfo)
    {
        if (KeyboardInfo.WasKeyJustPressed(Keys.Up))
        {
            IndexUp();
        }
        else if (KeyboardInfo.WasKeyJustPressed(Keys.Down))
        {
            IndexDown();
        }
        else if (KeyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            if (_cursorPosition is null)
                throw new ArgumentException($"No {nameof(_cursorPosition)}");
            if (_performer is null)
                throw new InvalidOperationException();
            IMove move = Moves[FocusIndex];
            HashSet<Point> attackPoints = _attackNameToAttackPoints[move.Name];
            SetIsVisible<ContextMenu>(false);
            void success()
            {
                _selectMove?.Invoke(this, EventArgs.Empty);
                Close();
            }
            void cancellation()
            {
                SetIsVisible<ContextMenu>(true);
                UIRoot.Focus(this);
            }
            if (attackPoints.Count is 0)
            {
                success();
                return;
            }
            _hideMoveOverlay();
            SetIsVisible<ContextMenu>(false);
            MovePreview.Open(move, _performer, attackPoints, success, cancellation);
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            _showMoveOverlay();
            Close();
        }
    }

    private void IndexUp()
    {
        if (FocusIndex > 0)
            FocusIndex--;
    }

    private void IndexDown()
    {
        if (FocusIndex < Moves.Count - 1)
            FocusIndex++;
    }

    /// <summary>
    /// In the future, would be nice to have the idea of Text and List elements,
    /// that could just be drawn as child elements, but we make do.
    /// </summary>
    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        base.Draw(spriteBatch, parentBounds);

        float yScale = GetYScale(parentBounds);
        Vector2 movePosition = new()
        {
            X = GetPositionX(parentBounds) + 2 * GetXScale(parentBounds),
            Y = GetPositionY(parentBounds) + (10 * yScale)
        };
        for (int i = 0; i < Moves.Count; i++)
        {
            IMove move = Moves[i];
            spriteBatch.DrawString(Font, move.Name, movePosition, Color.Black);
            movePosition += new Vector2(x: 0, y: Font.LineSpacing + yScale);
            if (i == FocusIndex)
            {
                Vector2 focusPosition = new(x: movePosition.X, movePosition.Y - yScale);
                spriteBatch.Draw(
                    textureRegion: FocusTexture,
                    position: focusPosition,
                    Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: Vector2.One * yScale,
                    SpriteEffects.None,
                    layerDepth: 1);
            }
        }
    }
}
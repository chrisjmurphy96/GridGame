using GridLibrary.Entities;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridLibrary.UI;

public class MovePreview : UIElement, IRouteableElement
{
    public required SpriteFont Font { get; init; }
    public required AttackOverlay AttackOverlay { get; init; }
    public required Cursor Cursor { get; init; }

    public IMove? Move { get; private set; }
    public IEntity? Performer { get; set; }
    public IEntity? Receiver { get; set; }

    public void Initialize()
    {
        Point cursorPosition = GridState.Instance.CursorPosition;
        GridTileList gridTiles = GridState.Instance.Tiles;
        IEntity activeEntity = GridState.Instance.ActiveEntity?.entity ?? throw new ArgumentException($"No {nameof(activeEntity)}");
        Performer = activeEntity;
        Move = activeEntity.SelectedMove;
        int range = activeEntity.SelectedMove.Range;
        AttackOverlay.Show(range, cursorPosition, gridTiles);
        Dictionary<Point, IEntity> entities = GridState.Instance.Entities;
        bool foundEnemy = false;
        foreach (Point attackPoint in AttackOverlay.AttackPoints)
        {
            if (entities.TryGetValue(attackPoint, out IEntity? entity) &&
                entity is not null &&
                !entity.IsFriendly)
            {
                GridState.Instance.CursorPosition = attackPoint;
                foundEnemy = true;
                Cursor.SetPosition(attackPoint);
                // TODO: The cursor probably needs to be the source of truth for this.
                // It's getting ridiculous managing this separately.
                GridState.Instance.CursorPosition = attackPoint;
                break;
            }
        }
        if (!foundEnemy && AttackOverlay.AttackPoints.Count > 0)
        {
            Point attackPoint = AttackOverlay.AttackPoints.First();
            Cursor.SetPosition(attackPoint);
            // TODO: The cursor probably needs to be the source of truth for this.
            // It's getting ridiculous managing this separately.
            GridState.Instance.CursorPosition = attackPoint;
        }
    }

    /// <summary>
    /// TODO: In Echoes, you actually can't select a square to attack unless it contains an enemy.
    /// That's probably a simple check to add.
    /// </summary>
    public override void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
    {
        if (!IsVisible)
            return;

        Point cursorPosition = GridState.Instance.CursorPosition;
        if (keyboardInfo.WasKeyJustPressed(Keys.Up))
        {
            Point up = cursorPosition.Up();
            bool enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(up)?.IsFriendly) ?? false;
            if (AttackOverlay.AttackPoints.Contains(up) && enemyPresent)
            {
                Cursor.SetPosition(up);
                GridState.Instance.CursorPosition = up;
            }
            else
            {
                Point? validCandidate = null;
                foreach (Point attackPoint in AttackOverlay.AttackPoints)
                {
                    enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(attackPoint)?.IsFriendly) ?? false;
                    if (attackPoint.IsAbove(cursorPosition) && enemyPresent)
                    {
                        if (validCandidate is null)
                            validCandidate = attackPoint;
                        else if (cursorPosition.DistanceTo(attackPoint) < cursorPosition.DistanceTo(validCandidate.Value))
                            validCandidate = attackPoint;
                    }
                }
                if (validCandidate is not null)
                {
                    Cursor.SetPosition(validCandidate.Value);
                    GridState.Instance.CursorPosition = validCandidate.Value;
                }
            }
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.Down))
        {
            Point down = cursorPosition.Down();
            bool enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(down)?.IsFriendly) ?? false;
            if (AttackOverlay.AttackPoints.Contains(down) && enemyPresent)
            {
                Cursor.SetPosition(down);
                GridState.Instance.CursorPosition = cursorPosition.Down();
            }
            else
            {
                Point? validCandidate = null;
                foreach (Point attackPoint in AttackOverlay.AttackPoints)
                {
                    enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(attackPoint)?.IsFriendly) ?? false;
                    if (attackPoint.IsBelow(cursorPosition) && enemyPresent)
                    {
                        if (validCandidate is null)
                            validCandidate = attackPoint;
                        else if (cursorPosition.DistanceTo(attackPoint) < cursorPosition.DistanceTo(validCandidate.Value))
                            validCandidate = attackPoint;
                    }
                }
                if (validCandidate is not null)
                {
                    Cursor.SetPosition(validCandidate.Value);
                    GridState.Instance.CursorPosition = validCandidate.Value;
                }
            }
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.Right))
        {
            Point right = cursorPosition.Right();
            bool enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(right)?.IsFriendly) ?? false;
            if (AttackOverlay.AttackPoints.Contains(right) && enemyPresent)
            {
                Cursor.SetPosition(right);
                GridState.Instance.CursorPosition = right;
            }
            else
            {
                Point? validCandidate = null;
                foreach (Point attackPoint in AttackOverlay.AttackPoints)
                {
                    enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(attackPoint)?.IsFriendly) ?? false;
                    if (attackPoint.IsRightOf(cursorPosition) && enemyPresent)
                    {
                        if (validCandidate is null)
                            validCandidate = attackPoint;
                        else if (cursorPosition.DistanceTo(attackPoint) < cursorPosition.DistanceTo(validCandidate.Value))
                            validCandidate = attackPoint;
                    }
                }
                if (validCandidate is not null)
                {
                    Cursor.SetPosition(validCandidate.Value);
                    GridState.Instance.CursorPosition = validCandidate.Value;
                }
            }
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.Left))
        {
            Point left = cursorPosition.Left();
            bool enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(left)?.IsFriendly) ?? false;
            if (AttackOverlay.AttackPoints.Contains(left) && enemyPresent)
            {
                Cursor.SetPosition(left);
                GridState.Instance.CursorPosition = left;
            }
            else
            {
                Point? validCandidate = null;
                foreach (Point attackPoint in AttackOverlay.AttackPoints)
                {
                    enemyPresent = !(GridState.Instance.Entities.GetValueOrDefault(attackPoint)?.IsFriendly) ?? false;
                    if (attackPoint.IsLeftOf(cursorPosition) && enemyPresent)
                    {
                        if (validCandidate is null)
                            validCandidate = attackPoint;
                        else if (cursorPosition.DistanceTo(attackPoint) < cursorPosition.DistanceTo(validCandidate.Value))
                            validCandidate = attackPoint;
                    }
                }
                if (validCandidate is not null)
                {
                    Cursor.SetPosition(validCandidate.Value);
                    GridState.Instance.CursorPosition = validCandidate.Value;
                }
            }
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            if (AttackOverlay.AttackPoints.Count is 0)
            {
                Router.RouteTo(DefaultRoutes.Grid);
            }
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            AttackOverlay.Hide();
            Router.RouteTo(DefaultRoutes.ContextMenu);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        base.Draw(spriteBatch, parentBounds);

        if (!IsVisible)
            return;

        if (Performer is null)
            throw new ArgumentException($"{nameof(Performer)} not provided");
        if (Move is null)
            throw new ArgumentException($"{nameof(Move)} not provided");

        float xScale = GetXScale(parentBounds);
        float yScale = GetYScale(parentBounds);
        Vector2 scale = new(xScale, yScale);
        Vector2 movePreviewPosition = new()
        {
            X = GetPositionX(parentBounds),
            Y = GetPositionY(parentBounds)
        };

        Color textColor = Color.Black;
        Vector2 performerNamePosition = movePreviewPosition + (new Vector2(2, 2) * scale);
        spriteBatch.DrawString(Font, Performer.DisplayName, performerNamePosition, textColor);

        Vector2 statPosition = movePreviewPosition + (new Vector2(x: 50, y: 12) * scale);
        IEntity perfomer = Performer;
        List<int> stats = [
            perfomer.Health.CurrentHealth,
            Move.Damage,
            Move.HitChance,
            Move.CritChance
        ];
        foreach(int stat in stats)
        {
            spriteBatch.DrawString(Font, stat.ToString(), statPosition, Color.Black);
            statPosition += new Vector2(x: 0, y: Font.LineSpacing + yScale);
        }
    }
}
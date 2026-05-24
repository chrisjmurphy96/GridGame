using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridLibrary.UI.ContextMenu;

/// <summary>
/// First priorities:
/// - Menu positioning
/// - Move text positioning
/// - Binding. How do we get back a move to perform for the entity?
/// - Melee attacks first
/// - Ranged attacks second
/// - Fancy magic last
/// 
/// Menu items:
/// - Wait
///   - Does nothing, unit finishes move at cursor
/// - Attack
///   - Pulls up the MovePreview
/// - Items
///   - Pulls up the (unbuilt) ItemMenu
/// Unlike the wizard flow I've been building,
/// this suggests a branching flow. Perhaps each
/// wizard move could provide a "NextMove" key? Or maybe,
/// make it more like a router? Each move can load in the next
/// UI Element without needing to know the whole flow.
/// 
/// Nice to haves:
/// - Scroll logic in the menu
/// - Secondary menu popouts
/// </summary>
public class ContextMenu : UIElement, IRouteableElement
{
    public required TextureRegion FocusTexture { get; init; }
    public required SpriteFont Font { get; init; }
    public required KeyboardInfo KeyboardInfo { get; init; }
    public required Cursor Cursor { get; init; }

    private static IEntity? Performer => GridState.Instance.ActiveEntity?.entity;
    private List<IContextMenuItem> _menuItems = [];
    public int FocusIndex { get; private set; } = 0;
    public bool PreviewingMove { get; private set; } = false;

    public void Initialize()
    {
        _menuItems.Clear();
        _menuItems.Add(new WaitContextMenuItem());
        // TODO: For now I'm just always resetting the index
        // I believe FE actually keeps track of this as long as the player hasn't moved.
        // I'm not sure if this is practical to do with my current routing architecture.
        FocusIndex = 0;
        Point position = GridState.Instance.ActiveEntity?.position ?? throw new ArgumentException($"No active {nameof(position)}");
        if (Performer is null)
            throw new ArgumentException($"No {nameof(Performer)}");
        // First time opening the menu
        if (GridState.Instance.Entities.ContainsKey(position))
        {
            GridState.MoveActiveEntityToCursor();
            GridState.Instance.PotentialMove = GridState.Instance.CursorPosition;
        }
        else // we are coming back from cancelling a move
        {
            if (GridState.Instance.PotentialMove is null)
                throw new ArgumentException("No PotentialMove found");
            Cursor.SetPosition(GridState.Instance.PotentialMove.Value);
            GridState.Instance.CursorPosition = GridState.Instance.PotentialMove.Value;
        }
        GridTileList gridTiles = GridState.Instance.Tiles;
        foreach (IMove move in Performer.Moves)
        {
            HashSet<Point> attackablePoints = Dijkstra.GetAttackable(move.Range, [GridState.Instance.CursorPosition], gridTiles);
            bool enemyInRange = GridState.Instance.Entities.Any(positionAndEntity => attackablePoints.Contains(positionAndEntity.Key));
            if (enemyInRange)
            {
                _menuItems.Add(new AttackContextMenuItem());
                break;
            }
        }
    }

    public override void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
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
            if (Performer is null)
                throw new InvalidOperationException();
            _menuItems[FocusIndex].Click();
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            Router.RouteTo(DefaultRoutes.MovementArrow);
        }
    }

    private void IndexUp()
    {
        if (FocusIndex > 0)
            FocusIndex--;
    }

    private void IndexDown()
    {
        if (FocusIndex < _menuItems.Count - 1)
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
        for (int i = 0; i < _menuItems.Count; i++)
        {
            IContextMenuItem menuItem = _menuItems[i];
            spriteBatch.DrawString(
                Font,
                menuItem.Name,
                movePosition,
                Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
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
                    layerDepth: LayerDepths.StaticUIText);
            }
        }
    }
}
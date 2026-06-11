using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Input;
using GridLibrary.Routing;
using GridLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace GridLibrary.Grid;

public class MovementAnimation : UIElement, IRouteableElement
{
    private readonly static TimeSpan TILE_WALK_DELAY = TimeSpan.FromMilliseconds(150);
    private readonly Cursor _cursor;
    private TimeSpan _movementTimer;
    private float _movementProgress;
    private int _nextTileIndex = 1;
    private IEntity _entity;

    public MovementAnimation(Cursor cursor)
    {
        _cursor = cursor;
    }

    public void Initialize()
    {
        _cursor.Hide();
        _movementTimer = TimeSpan.Zero;
        _movementProgress = 0;
        _nextTileIndex = 1;
        _entity = GridState.Instance.ActiveEntity?.entity ?? throw new ArgumentException("No active entity");
        _entity.IsVisible = false;
    }

    /// <summary>
    /// I could allow the player to interrupt the move animation
    /// with the "cancel" button, but I don't think it's the end
    /// of the world to make them wait. That's what FE seems to do.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputInfo inputInfo)
    {
        // No input, just game time updates
        _movementTimer += gameTime.ElapsedGameTime;
        if (_movementTimer >= TILE_WALK_DELAY)
        {
            _movementTimer -= TILE_WALK_DELAY;
            _nextTileIndex++;
        }
        if (_nextTileIndex >= MovementState.Path.Count)
        {
            Router.RouteTo(DefaultRoutes.ContextMenu);
            return;
        }
        _movementProgress = (float)(_movementTimer.TotalSeconds / TILE_WALK_DELAY.TotalSeconds);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        base.Draw(spriteBatch, parentBounds);

        int nextIndex = _nextTileIndex;
        if (_nextTileIndex >= MovementState.Path.Count)
        {
            nextIndex = MovementState.Path.Count - 1;
        }
        int currentIndex = nextIndex;
        if (nextIndex > 0)
        {
            currentIndex = nextIndex - 1;
        }
        Debug.WriteLine(currentIndex);
        Debug.WriteLine(_movementProgress);

        int spriteSize = _entity.ActiveTexture.Width;
        Vector2 current = MovementState.Path[currentIndex].ToVector2();
        Vector2 next = MovementState.Path[nextIndex].ToVector2();
        Vector2 positionVector = Vector2.Lerp(current, next, _movementProgress) * 4 * spriteSize;
        Vector2 scale = Vector2.One * 4;
        spriteBatch.Draw(
            textureRegion: _entity.ActiveTexture,
            positionVector,
            Color.White,
            rotation: 0,
            origin: Vector2.Zero,
            scale: scale,
            SpriteEffects.None,
            layerDepth: LayerDepths.Entities);
    }
}

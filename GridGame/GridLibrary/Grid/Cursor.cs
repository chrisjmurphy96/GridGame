using GridLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class Cursor
{
    // I should consider a way to sync step size.
    // Does the camera make sense as source of truth? Or separate completely?
    public int Step { get; set; } = 64;

    public Vector2 Position => _position;
    private Vector2 _position = Vector2.Zero;
    private readonly Camera _camera;

    public AnimatedSprite CursorSprite { get; }
    public bool IsVisible { get; private set; } = true;

    public Cursor(Camera camera, AnimatedSprite cursorSprite)
    {
        _camera = camera;
        CursorSprite = cursorSprite;
    }

    public void Update(GameTime gameTime) => CursorSprite.Update(gameTime);

    /// <summary>
    /// This goes off of a grid coordinate instead of a raw
    /// position vector, so scaling should not be applied to the
    /// point. This function will handle that.
    /// </summary>
    public void SetPosition(Point point)
    {
        _position = point.ToVector2() * Step;
        FixCamera();
    }

    public void Move(Vector2 movement)
    {
        Move(movement, Step);
    }

    public void Move(Vector2 movement, int step)
    {
        _position += movement * step;
        FixPosition();
        FixCamera();
    }

    public void MoveUp() => Move(-Vector2.UnitY);
    public void MoveDown() => Move(Vector2.UnitY);
    public void MoveLeft() => Move(-Vector2.UnitX);
    public void MoveRight() => Move(Vector2.UnitX);

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private void FixPosition()
    {
        Rectangle cameraBounds = _camera.CameraBounds;
        Rectangle cursorBounds = GetBounds();
        if (cursorBounds.Top < cameraBounds.Top)
            _position.Y = 0;
        if (cursorBounds.Bottom > cameraBounds.Bottom)
            _position.Y = cameraBounds.Bottom - cursorBounds.Height;
        if (cursorBounds.Left < cameraBounds.Left)
            _position.X = 0;
        if (cursorBounds.Right > cameraBounds.Right)
            _position.X = cameraBounds.Right - cursorBounds.Width;
    }

    private void FixCamera()
    {
        Rectangle cursorBounds = GetBounds();
        // Maybe a lazy solution, but this should be so fast no one will notice
        bool cursorInView = false;
        while (!cursorInView)
        {
            Rectangle viewBounds = _camera.CurrentViewBounds();
            if (cursorBounds.Top < viewBounds.Top)
                _camera.MoveUp();
            else if (cursorBounds.Bottom > viewBounds.Bottom)
                _camera.MoveDown();
            else if (cursorBounds.Left < viewBounds.Left)
                _camera.MoveLeft();
            else if (cursorBounds.Right > viewBounds.Right)
                _camera.MoveRight();
            else
                cursorInView = true;
        }
        
    }

    private Rectangle GetBounds()
    {
        return new Rectangle((int)_position.X, (int)_position.Y, (int)CursorSprite.Width, (int)CursorSprite.Height);
    }
}
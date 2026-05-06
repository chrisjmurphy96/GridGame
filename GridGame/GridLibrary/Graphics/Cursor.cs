using Microsoft.Xna.Framework;

namespace GridLibrary.Graphics;

public class Cursor
{
    // I should consider a way to sync step size.
    // Does the camera make sense as source of truth? Or separate completely?
    public int Step { get; set; } = 64;

    public Vector2 Position = Vector2.Zero;
    public required AnimatedSprite CursorSprite { get; set; }

    public void Update(GameTime gameTime) => CursorSprite.Update(gameTime);

    public void Move(Vector2 movement, Camera camera)
    {
        Move(movement, Step, camera);
    }

    public void Move(Vector2 movement, int step, Camera camera)
    {
        Position += movement * step;
        FixPosition(camera);
        FixCamera(camera);
    }

    public void MoveUp(Camera camera) => Move(-Vector2.UnitY, camera);
    public void MoveDown(Camera camera) => Move(Vector2.UnitY, camera);
    public void MoveLeft(Camera camera) => Move(-Vector2.UnitX, camera);
    public void MoveRight(Camera camera) => Move(Vector2.UnitX, camera);

    private void FixPosition(Camera camera)
    {
        Rectangle cameraBounds = camera.CameraBounds;
        Rectangle cursorBounds = GetBounds();
        if (cursorBounds.Top < cameraBounds.Top)
            Position.Y = 0;
        if (cursorBounds.Bottom > cameraBounds.Bottom)
            Position.Y = cameraBounds.Bottom - cursorBounds.Height;
        if (cursorBounds.Left < cameraBounds.Left)
            Position.X = 0;
        if (cursorBounds.Right > cameraBounds.Right)
            Position.X = cameraBounds.Right - cursorBounds.Width;
    }

    private void FixCamera(Camera camera)
    {
        Rectangle viewBounds = camera.CurrentViewBounds();
        Rectangle cursorBounds = GetBounds();
        if (cursorBounds.Top < viewBounds.Top)
            camera.MoveUp();
        if (cursorBounds.Bottom > viewBounds.Bottom)
            camera.MoveDown();
        if (cursorBounds.Left < viewBounds.Left)
            camera.MoveLeft();
        if (cursorBounds.Right > viewBounds.Right)
            camera.MoveRight();
    }

    private Rectangle GetBounds()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, (int)CursorSprite.Width, (int)CursorSprite.Height);
    }
}
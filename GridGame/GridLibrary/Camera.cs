using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary;

/// <summary>
/// Steals a bit of code from https://github.com/MonoGame-Extended/Monogame-Extended/blob/develop/source/MonoGame.Extended/OrthographicCamera.cs
/// Their implementation is very in line with what I was already trying to do, but they also knew what each column in the vector was for.
/// Needs to handle:
/// - Movement (transform)
/// - Zoom
/// - Shake? Maybe this is better as an Effect.
/// <summary>
public class Camera(GraphicsDevice graphicsDevice)
{
    public float Zoom { get; set; } = 1.0f;
    public float ZoomStep { get; set; } = 0.1f;

    public Vector2 Position = Vector2.Zero;

    /// <summary>
    /// Multiplier for camera movement in pixels.
    /// Perhaps better to figure out a method to scale to current grid size.
    /// <summary>
    public int Step { get; set; } = 64;

    public Rectangle CameraBounds = Rectangle.Empty;

    private Matrix _transformation = Matrix.Identity;
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

    public Matrix GetViewMatrix()
    {
        Matrix viewMatrix = Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                            Matrix.CreateScale(Zoom, Zoom, zScale: 1);
        return viewMatrix;
    }

    public void Move(Vector2 movement)
    {
        Move(movement, Step);
    }

    public void Move(Vector2 movement, int step)
    {
        Position += movement * step;
        FixPosition();
    }

    public void MoveUp() => Move(-Vector2.UnitY);
    public void MoveDown() => Move(Vector2.UnitY);
    public void MoveLeft() => Move(-Vector2.UnitX);
    public void MoveRight() => Move(Vector2.UnitX);

    // TODO: fix to center properly with zoom
    public void Center()
    {
        Vector2 cameraBoundsDimensions = new(CameraBounds.Width, CameraBounds.Height);
        Vector2 center = (cameraBoundsDimensions - GetViewportDimensions()) / 2;
        Position = center;
        Matrix viewMatrix = GetViewMatrix();
        Debug.Write(viewMatrix);
    }

    public float ZoomIn()
    {
        return Zoom += ZoomStep;
    }

    public float ZoomIn(int zoomStep)
    {
        return Zoom += zoomStep;
    }

    public float ZoomOut()
    {
        return Zoom -= ZoomStep;
    }

    public float ZoomOut(int zoomStep)
    {
        return Zoom -= zoomStep;
    }

    public void SnapToGrid()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Need this to handle the translation of the screen.
    /// <summary>
    public Rectangle CurrentViewBounds()
    {
        Point position = Position.ToPoint();
        Rectangle viewBounds = _graphicsDevice.Viewport.Bounds;
        viewBounds.X = position.X;
        viewBounds.Y = position.Y;
        return viewBounds;
    }

    private Vector2 GetViewportDimensions() => new(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

    // TODO: fix for zoom
    private void FixPosition()
    {
        Rectangle viewBounds = CurrentViewBounds();
        if (viewBounds.Top < CameraBounds.Top)
            Position.Y = 0;
        if (viewBounds.Bottom > CameraBounds.Bottom)
            Position.Y = CameraBounds.Bottom - _graphicsDevice.Viewport.Height;
        if (viewBounds.Left < CameraBounds.Left)
            Position.X = 0;
        if (viewBounds.Right > CameraBounds.Right)
            Position.X = CameraBounds.Right - _graphicsDevice.Viewport.Width;
    }

    private bool PositionIsOutOfBounds()
    {
        Rectangle screenBounds = _graphicsDevice.Viewport.Bounds;
        return 
            screenBounds.Top < CameraBounds.Top ||
            screenBounds.Bottom > CameraBounds.Bottom ||
            screenBounds.Left < CameraBounds.Left ||
            screenBounds.Right > CameraBounds.Right;
    }
}
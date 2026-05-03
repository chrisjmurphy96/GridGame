using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GridLibrary.Input;

public class KeyboardInfo
{
    /// <summary>
    /// Gets the state of keyboard input during the previous update cycle.
    /// </summary>
    public KeyboardState PreviousState { get; private set; } = new KeyboardState();

    /// <summary>
    /// Gets the state of keyboard input during the current input cycle.
    /// </summary>
    public KeyboardState CurrentState { get; private set; } = Keyboard.GetState();

    private readonly Dictionary<Keys, (TimeSpan initial, TimeSpan elapsedGameTime)> _keyHolds = [];

    /// <summary>
    /// Updates the state information about keyboard input.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        PreviousState = CurrentState;
        CurrentState = Keyboard.GetState();
        Keys[] currentKeys = CurrentKeys();
        // incredibly unoptimized, but probably not an issue unless someone hold the entire keyboard.
        foreach(Keys key in _keyHolds.Keys)
        {
            if (!currentKeys.Contains(key))
                _keyHolds.Remove(key);
        }
        foreach (Keys key in currentKeys)
        {
            if (_keyHolds.TryGetValue(key, out (TimeSpan initialGameTime, TimeSpan elapsed) value))
            {
                _keyHolds[key] = (value.initialGameTime, gameTime.TotalGameTime - value.initialGameTime);
            }
            else
            {
                _keyHolds[key] = (gameTime.TotalGameTime, TimeSpan.Zero);
            }
        }
    }

    /// <summary>
    /// Returns a value that indicates if the specified key is currently down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the specified key is currently down; otherwise, false.</returns>
    public bool IsKeyDown(Keys key) => CurrentState.IsKeyDown(key);

    /// <summary>
    /// Returns a value that indicates whether the specified key is currently up.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the specified key is currently up; otherwise, false.</returns>
    public bool IsKeyUp(Keys key) => CurrentState.IsKeyUp(key);

    /// <summary>
    /// Returns a value that indicates if the specified key was just pressed on the current frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the specified key was just pressed on the current frame; otherwise, false.</returns>
    public bool WasKeyJustPressed(Keys key) => CurrentState.IsKeyDown(key) && PreviousState.IsKeyUp(key);

    /// <summary>
    /// Returns a value that indicates if the specified key was just released on the current frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the specified key was just released on the current frame; otherwise, false.</returns>
    public bool WasKeyJustReleased(Keys key) => CurrentState.IsKeyUp(key) && PreviousState.IsKeyDown(key);

    public bool IsKeyHeldDown(Keys key, TimeSpan? keyHoldLength = null)
    {
        keyHoldLength ??= TimeSpan.FromSeconds(1);
        if (_keyHolds.TryGetValue(key, out (TimeSpan _, TimeSpan elaspedGameTime) value))
        {
            return value.elaspedGameTime > keyHoldLength;
        }
        return false;
    }

    public void ResetKeyHold(Keys keys)
    {
        _keyHolds.Remove(keys);
    }

    /// <summary>
    /// For debugging. Some keys have odd key codes...
    /// </summary>
    public Keys[] CurrentKeys() => CurrentState.GetPressedKeys();
}

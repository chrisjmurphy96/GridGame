using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GridLibrary.Input;

public class GamePadInfo
{
    public GamePadState PreviousState { get; private set; } = new GamePadState();
    public GamePadState CurrentState { get; private set; } = GamePad.GetState(PlayerIndex.One);
    public bool IsConnected => CurrentState.IsConnected;
    readonly Dictionary<Buttons, (TimeSpan initial, TimeSpan elapsedGameTime)> _buttonHolds = [];

    public void Update(GameTime gameTime)
    {
        PreviousState = CurrentState;
        CurrentState = GamePad.GetState(PlayerIndex.One);
        UpdateButtonHold(gameTime, Buttons.LeftThumbstickUp);
        UpdateButtonHold(gameTime, Buttons.LeftThumbstickDown);
        UpdateButtonHold(gameTime, Buttons.LeftThumbstickLeft);
        UpdateButtonHold(gameTime, Buttons.LeftThumbstickRight);
        UpdateButtonHold(gameTime, Buttons.DPadUp);
        UpdateButtonHold(gameTime, Buttons.DPadDown);
        UpdateButtonHold(gameTime, Buttons.DPadLeft);
        UpdateButtonHold(gameTime, Buttons.DPadRight);
    }

    private void UpdateButtonHold(GameTime gameTime, Buttons button)
    {
        if (IsButtonDown(button))
        {
            if (_buttonHolds.TryGetValue(button, out (TimeSpan initialGameTime, TimeSpan elapsed) value))
            {
                _buttonHolds[button] = (value.initialGameTime, gameTime.TotalGameTime - value.initialGameTime);
            }
            else
            {
                _buttonHolds[button] = (gameTime.TotalGameTime, TimeSpan.Zero);
            }
        }
    }

    public bool IsButtonHeldDown(Buttons button, TimeSpan? holdLength = null)
    {
        holdLength ??= TimeSpan.FromSeconds(1);
        if (_buttonHolds.TryGetValue(button, out (TimeSpan _, TimeSpan elaspedGameTime) value))
        {
            return value.elaspedGameTime > holdLength;
        }
        return false;
    }

    public void ResetButtonHold(Buttons button)
    {
        _buttonHolds.Remove(button);
    }

    public bool IsButtonDown(Buttons button)
    {
        return CurrentState.IsButtonDown(button);
    }

    public bool IsButtonUp(Buttons button)
    {
        return CurrentState.IsButtonUp(button);
    }

    public bool WasButtonJustPressed(Buttons button)
    {
        return CurrentState.IsButtonDown(button) && PreviousState.IsButtonUp(button);
    }

    public bool WasButtonJustReleased(Buttons button)
    {
        return CurrentState.IsButtonUp(button) && PreviousState.IsButtonDown(button);
    }
}

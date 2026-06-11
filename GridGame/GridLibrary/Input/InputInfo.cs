using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GridLibrary.Input;

public class InputInfo
{
    private readonly GamePadInfo _gamePadInfo;
    private readonly KeyboardInfo _keyboardInfo;

    public InputInfo(GamePadInfo gamePadInfo, KeyboardInfo keyboardInfo)
    {
        _gamePadInfo = gamePadInfo;
        _keyboardInfo = keyboardInfo;
    }

    public void Update(GameTime gameTime)
    {
        _gamePadInfo.Update(gameTime);
        _keyboardInfo.Update(gameTime);
    }

    public bool UpPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.Up) || _gamePadInfo.WasButtonJustPressed(Buttons.DPadUp);
    }

    public bool DownPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.Down) || _gamePadInfo.WasButtonJustPressed(Buttons.DPadDown);
    }

    public bool RightPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.Right) || _gamePadInfo.WasButtonJustPressed(Buttons.DPadRight);
    }

    public bool LeftPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.Left) || _gamePadInfo.WasButtonJustPressed(Buttons.DPadLeft);
    }

    public bool SelectPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.Z) || _gamePadInfo.WasButtonJustPressed(Buttons.A);
    }

    public bool CancelPressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.X) || _gamePadInfo.WasButtonJustPressed(Buttons.B);
    }

    public bool TogglePressed()
    {
        return _keyboardInfo.WasKeyJustPressed(Keys.O) || _gamePadInfo.WasButtonJustPressed(Buttons.Y);
    }

    public bool UpHeld(TimeSpan? holdLength)
    {
        return _keyboardInfo.IsKeyHeldDown(Keys.Up, holdLength) || _gamePadInfo.IsButtonHeldDown(Buttons.DPadUp, holdLength);
    }

    public bool DownHeld(TimeSpan? holdLength)
    {
        return _keyboardInfo.IsKeyHeldDown(Keys.Down, holdLength) || _gamePadInfo.IsButtonHeldDown(Buttons.DPadDown, holdLength);
    }

    public bool RightHeld(TimeSpan? holdLength)
    {
        return _keyboardInfo.IsKeyHeldDown(Keys.Right, holdLength) || _gamePadInfo.IsButtonHeldDown(Buttons.DPadRight, holdLength);
    }

    public bool LeftHeld(TimeSpan? holdLength)
    {
        return _keyboardInfo.IsKeyHeldDown(Keys.Left, holdLength) || _gamePadInfo.IsButtonHeldDown(Buttons.DPadLeft, holdLength);
    }

    public void ResetUpHold()
    {
        _keyboardInfo.ResetKeyHold(Keys.Up);
        _gamePadInfo.ResetButtonHold(Buttons.DPadUp);
    }

    public void ResetDownHold()
    {
        _keyboardInfo.ResetKeyHold(Keys.Down);
        _gamePadInfo.ResetButtonHold(Buttons.DPadDown);
    }

    public void ResetRightHold()
    {
        _keyboardInfo.ResetKeyHold(Keys.Right);
        _gamePadInfo.ResetButtonHold(Buttons.DPadRight);
    }

    public void ResetLeftHold()
    {
        _keyboardInfo.ResetKeyHold(Keys.Left);
        _gamePadInfo.ResetButtonHold(Buttons.DPadLeft);
    }
}

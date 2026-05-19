using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridLibrary.UI;

public class MovePreview : UIElement
{
    public required SpriteFont Font { get; init; }

    public IMove? Move { get; private set; }
    public IEntity? Performer { get; set; }
    public IEntity? Receiver { get; set; }

    private HashSet<Point> _attackPoints = [];
    private EventHandler? _confirm;
    private EventHandler? _cancel;

    public void Open(
        IMove move,
        IEntity performer,
        HashSet<Point> attackPoints,
        Action confirmationCallback,
        Action cancellationCallback)
    {
        SetIsVisible<MovePreview>(true);
        UIRoot.Focus(this);
        Move = move;
        Performer = performer;
        _attackPoints = attackPoints;
        _confirm += (_, _) => confirmationCallback();
        _cancel += (_, _) => cancellationCallback();
    }

    public void Close()
    {
        SetIsVisible<MovePreview>(false);
        Move = null;
        Performer = null;
        Receiver = null;
        _attackPoints.Clear();
        _confirm = null;
        _cancel = null;
    }

    public override void HandleInput(KeyboardInfo keyboardInfo)
    {
        if (!IsVisible)
            return;

        if (keyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            if (_attackPoints.Count is 0)
            {
                _confirm?.Invoke(this, EventArgs.Empty);
                Close();
            }
            // TODO: need to switch to yet ANOTHER view where we let
            // the player attempt to select a valid target.
            // Maybe we can also deny the action if there are no targets in range.
        }
        else if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            _cancel?.Invoke(this, EventArgs.Empty);
            Close();
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
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI;

public class CharacterInfo : UIElement
{
    private SpriteFont? _font;
    private IEntity _hoveredEntity;

    public CharacterInfo SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public override void Update(GameTime gameTime)
    {
        IEntity? hoveredEntity = GridState.Instance.GetHoveredEntity();
        if (hoveredEntity is not null && Router.CurrentRoute != DefaultRoutes.AttackContainer)
        {
            _hoveredEntity = hoveredEntity;
            SetIsVisible(true);
        }
        else
            SetIsVisible(false);

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_font is null)
            throw new ArgumentException($"Please call {SetFont}");
        if (!IsVisible)
            return;

        base.Draw(spriteBatch, parentBounds);

        string displayName = _hoveredEntity.DisplayName;
        string hpCurrent = _hoveredEntity.Health.CurrentHealth.ToString();
        string hpMax = _hoveredEntity.Health.MaxHealth.ToString();
        string currentOverMax = $"{hpCurrent}/{hpMax}";
        string hp = "HP:";
        Vector2 position = new()
        {
            X = GetPositionX(parentBounds),
            Y = GetPositionY(parentBounds)
        };
        Vector2 dimensions = new()
        {
            X = GetWidth(parentBounds),
            Y = GetHeight(parentBounds)
        };
        Vector2 textSize = _font.MeasureString(displayName);
        Vector2 verticalCenterOfText = new()
        {
            X = 0,
            Y = textSize.Y / 2
        };
        Vector2 displayNamePosition = new()
        {
            X = position.X + (dimensions.X * 0.1f),
            Y = position.Y + (dimensions.Y * 0.5f)
        };
        Color color = _hoveredEntity.IsFriendly ? new Color(16, 43, 211) : new Color(207, 31, 31);
        spriteBatch.DrawString(
                _font,
                displayName,
                displayNamePosition,
                color,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);

        Vector2 hpPosition = new()
        {
            X = position.X + (dimensions.X * 0.6f),
            Y = position.Y + (dimensions.Y * 0.5f)
        };
        spriteBatch.DrawString(
                _font,
                hp,
                hpPosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);

        float horizontalPadding = 32;
        Vector2 currentOverMaxPosition = new()
        {
            X = hpPosition.X + horizontalPadding,
            Y = hpPosition.Y
        };
        spriteBatch.DrawString(
                _font,
                currentOverMax,
                currentOverMaxPosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
    }
}

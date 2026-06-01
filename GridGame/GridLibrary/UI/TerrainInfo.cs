using GridLibrary.Graphics;
using GridLibrary.Grid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI;

public class TerrainInfo : UIElement
{
    private SpriteFont? _font;

    public TerrainInfo SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        base.Draw(spriteBatch, parentBounds);

        if (_font is null)
            throw new ArgumentException($"Please call {SetFont}");
        TileInfo? tileInfo = GridState.Instance.ActiveTile?.TileInfo;
        if (tileInfo is not null)
        {
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
            Vector2 textSize = _font.MeasureString(tileInfo.TileType);
            Vector2 verticalCenterOfText = new()
            {
                X = 0,
                Y = textSize.Y / 2
            };
            Vector2 terrainTypePosition = new()
            {
                X = position.X + (dimensions.X * 0.1f),
                Y = position.Y + (dimensions.Y * 0.5f)
            };
            spriteBatch.DrawString(
                _font,
                tileInfo.TileType,
                terrainTypePosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);

            float horizontalPadding = 96;
            float statXPadding = position.X + (dimensions.X * 0.5f);
            float defenseYPadding = position.Y + (dimensions.Y * 0.3f);
            Vector2 defensePosition = new()
            {
                X = statXPadding,
                Y = defenseYPadding
            };
            string defense = "DEF:";
            spriteBatch.DrawString(
                _font,
                defense,
                defensePosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
            string defenseValue = $"+{tileInfo.DefenseModifier}";
            Vector2 defenseValueSize = _font.MeasureString(defenseValue);
            Vector2 defenseValuePosition = new()
            {
                X = statXPadding + horizontalPadding - defenseValueSize.X,
                Y = defensePosition.Y
            };
            spriteBatch.DrawString(
                _font,
                defenseValue,
                defenseValuePosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);

            float dodgeYPadding = position.Y + (dimensions.Y * 0.7f);
            Vector2 dodgePosition = new()
            {
                X = statXPadding,
                Y = dodgeYPadding
            };
            spriteBatch.DrawString(
                _font,
                $"AVO:",
                dodgePosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
            string dodgeValue = $"{tileInfo.DodgeModifier}%";
            Vector2 dodgeValueSize = _font.MeasureString(dodgeValue);
            Vector2 dodgeValuePosition = new()
            {
                X = statXPadding + horizontalPadding - dodgeValueSize.X,
                Y = dodgePosition.Y
            };
            spriteBatch.DrawString(
                _font,
                dodgeValue,
                dodgeValuePosition,
                Color.Black,
                rotation: 0,
                origin: verticalCenterOfText,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
        }
    }
}

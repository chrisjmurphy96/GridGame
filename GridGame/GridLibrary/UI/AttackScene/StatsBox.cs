using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GridLibrary.UI.AttackScene;

public class StatsBox : UIElement
{
    private SpriteFont? _font;
    private IMove? _move;

    public StatsBox SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public StatsBox SetMove(IMove move)
    {
        _move = move;
        return this;
    }

    /// <summary>
    /// This is still pretty rough, but it looks nice with the current textures so I'm leaving it.
    /// <para>&#160;</para>
    /// Stat names are drawn left justified, stat values are drawn right justified.
    /// 
    /// <para>&#160;</para>
    /// For the future:
    /// <para>&#160;</para>
    /// - Can have it measure the height off all rows of text and center based on the size/position of this element
    /// <para>&#160;</para>
    /// - Can have it measure the left offset based on size/position of this element.
    /// <para>&#160;</para>
    /// - Maybe have an additional offset if I want different values for left and right based ones?
    /// </summary>
    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_font is null)
            throw new ArgumentException($"Please call {nameof(SetFont)} before attempting to draw");
        if (_move is null)
            throw new ArgumentException($"Please call {nameof(SetMove)} before attempting to draw");

        base.Draw(spriteBatch, parentBounds);


        string[] stats = ["HIT", "DMG", "CRT"];
        Vector2 scale = new()
        {
            X = 4,
            Y = 4
        };
        Vector2 statsOffset = new()
        {
            X = GetPositionX(parentBounds) + (10 * scale.X),
            Y = GetPositionY(parentBounds) + (10 * scale.Y)
        };
        List<Vector2> statNameSizes = [];
        foreach(string stat in stats)
        {
            spriteBatch.DrawString(
                _font,
                stat,
                statsOffset,
                Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
            Vector2 statSize = new()
            {
                X = 0,
                Y = _font.MeasureString(stat).Y
            };
            statNameSizes.Add(statSize);
            statsOffset += statSize;
        }

        string[] statValues = [_move.HitChance.ToString(), _move.Damage.ToString(), _move.CritChance.ToString()];
        Vector2 statValuesOffset = new()
        {
            X = GetPositionX(parentBounds) + (10 * scale.X),
            Y = GetPositionY(parentBounds) + (10 * scale.Y)
        };
        for (int i = 0; i < statValues.Length; i++)
        {
            string statValue = statValues[i];
            Vector2 statNameSize = statNameSizes[i];
            Vector2 justifyOffset = new()
            {
                X = _font.MeasureString(statValue).X,
                Y = 0
            };
            Vector2 horizontalPadding = new(96, 0);
            Vector2 actualOffset = statValuesOffset + horizontalPadding - justifyOffset;
            spriteBatch.DrawString(
                _font,
                statValue,
                actualOffset,
                Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 1,
                SpriteEffects.None,
                layerDepth: LayerDepths.StaticUIText);
            Vector2 statValueSize = new()
            {
                X = 0,
                Y = _font.MeasureString(statValue).Y
            };
            statValuesOffset += statNameSize;
        }
    }
}

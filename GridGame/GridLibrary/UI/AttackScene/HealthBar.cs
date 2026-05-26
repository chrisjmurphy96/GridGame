using GridLibrary.Entities;
using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI.AttackScene;
public class HealthBar : UIElement
{
    private TextureRegion? _activeHealth;
    private TextureRegion? _inactiveHealth;
    private IEntity? _entity;
    private SpriteFont? _font;

    public HealthBar SetTextures(TextureRegion active, TextureRegion inactive)
    {
        _activeHealth = active;
        _inactiveHealth = inactive;
        // Even though I won't draw from this, it's useful for "Get" calculations in Draw
        SetTexture(active);
        return this;
    }

    public HealthBar SetEntity(IEntity entity)
    {
        _entity = entity;
        return this;
    }

    public HealthBar SetFont(SpriteFont spriteFont)
    {
        _font = spriteFont;
        return this;
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle parentBounds)
    {
        if (_activeHealth is null || _inactiveHealth is null)
            throw new ArgumentException($"Please call {SetTextures} before drawing");
        if (_entity is null)
            throw new ArgumentException($"Please call {SetEntity} before drawing");
        if (_font is null)
            throw new ArgumentException($"Please call {SetFont} before drawing");

        Vector2 healthSegmentPosition = new()
        {
            X = GetPositionX(parentBounds),
            Y = GetPositionY(parentBounds)
        };
        Vector2 scale = new()
        {
            X = 4,//GetXScale(parentBounds),
            Y = 4//GetYScale(parentBounds)
        };

        string currentHealth = _entity.Health.CurrentHealth.ToString();
        Vector2 currentHealthSize = _font.MeasureString(currentHealth);
        // Padded to the left of the health segments and centered vertically to them
        Vector2 currentHealthPosition = new()
        {
            X = healthSegmentPosition.X - ((currentHealthSize.X + scale.X)),
            Y = healthSegmentPosition.Y + ((_activeHealth.Height / 2) * scale.Y)
        };
        Vector2 verticalCenterOfText = new()
        {
            X = 0,
            Y = currentHealthSize.Y / 2
        };
        spriteBatch.DrawString(
            _font,
            currentHealth,
            currentHealthPosition,
            Color.Black,
            rotation: 0,
            origin: verticalCenterOfText,
            scale: 1,
            SpriteEffects.None,
            layerDepth: LayerDepths.StaticUIText);

        Vector2 xOffset = new(2 * scale.X, 0);
        for (int i = 0; i < _entity.Health.MaxHealth; i++)
        {
            if (i < _entity.Health.CurrentHealth)
            {
                spriteBatch.Draw(
                    textureRegion: _activeHealth,
                    position: healthSegmentPosition,
                    Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: LayerDepth);
            }
            else
            {
                spriteBatch.Draw(
                    textureRegion: _inactiveHealth,
                    position: healthSegmentPosition,
                    Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    SpriteEffects.None,
                    layerDepth: LayerDepth);
            }
            healthSegmentPosition += xOffset;
        }
    }
}

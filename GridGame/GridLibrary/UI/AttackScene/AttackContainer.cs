using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridLibrary.UI.AttackScene;

public class AttackContainer : UIElement, IRouteableElement
{
    private readonly NameBanner _enemyNameBanner = new();
    private readonly NameBanner _friendlyNameBanner = new();
    private readonly HealthBar _enemyHealthBar = new();
    private readonly HealthBar _friendlyHealthBar = new();
    private readonly UIElement _enemyHealthBanner = new();
    private readonly UIElement _friendlyHealthBanner = new();
    private readonly AttackBanner _enemyAttackBanner = new();
    private readonly AttackBanner _friendlyAttackBanner = new();
    private readonly StatsBox _enemyStatBox = new();
    private readonly StatsBox _friendlyStatBox = new();

    public AttackContainer() : base()
    {
        SetWidth(100, UIUnit.Percentage);
        SetHeight(100, UIUnit.Percentage);

        _enemyNameBanner
            .SetParent(this)
            .PadVertical(40, UIUnit.Pixels, UIVerticalPaddingOrientation.FromTop)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromLeft);
        _friendlyNameBanner
            .SetParent(this)
            .PadVertical(40, UIUnit.Pixels, UIVerticalPaddingOrientation.FromTop)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight);

        _enemyHealthBanner
            .SetParent(this)
            .PadVertical(0, UIUnit.Pixels, UIVerticalPaddingOrientation.FromBottom)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromLeft);
        _friendlyHealthBanner
            .SetParent(this)
            .PadVertical(0, UIUnit.Pixels, UIVerticalPaddingOrientation.FromBottom)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight);

        _enemyHealthBar
            .SetParent(_enemyHealthBanner)
            .PadHorizontal(20, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(60, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom)
            .SetLayerDepth(LayerDepths.StaticUIText);
        _friendlyHealthBar
            .SetParent(_friendlyHealthBanner)
            .PadHorizontal(20, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(60, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom)
            .SetLayerDepth(LayerDepths.StaticUIText);

        _enemyAttackBanner
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.05f)
            .PadHorizontal(15, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(18, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom);
        _friendlyAttackBanner
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.05f)
            .PadHorizontal(15, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(18, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom);

        _enemyStatBox
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.075f)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(18, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom);
        _friendlyStatBox
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.075f)
            .PadHorizontal(0, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(18, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom);
    }

    /// <summary>
    /// This is just for debugging until the attack/animation logic is in place.
    /// It'll let us continue to navigate around the grid.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="keyboardInfo"></param>
    public override void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
    {
        if (keyboardInfo.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Z))
        {
            Router.RouteTo(DefaultRoutes.Grid);
        }
    }

    public void Initialize()
    {
        IEntity friendly = GridState.Instance.ActiveEntity?.entity ?? throw new ArgumentException("No active entity found");
        if (!GridState.Instance.Entities.TryGetValue(GridState.Instance.CursorPosition, out IEntity? enemy))
            throw new ArgumentException("No enemy entity found");
        SetEnemy(enemy);
        SetFriendly(friendly);
    }

    public AttackContainer SetEnemy(IEntity entity)
    {
        _enemyNameBanner.SetText(entity.DisplayName);
        _enemyHealthBar.SetEntity(entity);
        _enemyAttackBanner.SetMove(entity.SelectedMove);
        _enemyStatBox.SetMove(entity.SelectedMove);
        return this;
    }

    public AttackContainer SetFriendly(IEntity entity)
    {
        _friendlyNameBanner.SetText(entity.DisplayName);
        _friendlyHealthBar.SetEntity(entity);
        _friendlyAttackBanner.SetMove(entity.SelectedMove);
        _friendlyStatBox.SetMove(entity.SelectedMove);
        return this;
    }

    public AttackContainer SetFont(SpriteFont spriteFont)
    {
        _enemyNameBanner.SetFont(spriteFont);
        _friendlyNameBanner.SetFont(spriteFont);
        _enemyHealthBar.SetFont(spriteFont);
        _friendlyHealthBar.SetFont(spriteFont);
        _enemyAttackBanner.SetFont(spriteFont);
        _friendlyAttackBanner.SetFont(spriteFont);
        _enemyStatBox.SetFont(spriteFont);
        _friendlyStatBox.SetFont(spriteFont);
        return this;
    }

    public AttackContainer SetEnemyNameBannerTexture(TextureRegion texture)
    {
        _enemyNameBanner
            .SetTexture(texture)
            .SetWidth(15, UIUnit.Percentage)
            .SetHeight(texture.Height * 4, UIUnit.Pixels);
        return this;
    }

    public AttackContainer SetFriendlyNameBannerTexture(TextureRegion texture)
    {
        _friendlyNameBanner
            .SetTexture(texture)
            .SetWidth(15, UIUnit.Percentage)
            .SetHeight(texture.Height * 4, UIUnit.Pixels);
        return this;
    }

    public AttackContainer SetHealthBarTextures(TextureRegion active, TextureRegion inactive)
    {
        _enemyHealthBar.SetTextures(active, inactive);
        _friendlyHealthBar.SetTextures(active, inactive);
        return this;
    }

    public AttackContainer SetEnemyHealthBannerTexture(TextureRegion texture)
    {
        _enemyHealthBanner
            .SetTexture(texture)
            .SetWidth(50, UIUnit.Percentage)
            .SetHeight(20, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetFriendlyHealthBannerTexture(TextureRegion texture)
    {
        _friendlyHealthBanner
            .SetTexture(texture)
            .SetWidth(50, UIUnit.Percentage)
            .SetHeight(20, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetEnemyAttackBannerTexture(TextureRegion texture)
    {
        _enemyAttackBanner
            .SetTexture(texture)
            .SetWidth(35, UIUnit.Percentage)
            .SetHeight(15, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetFriendlyAttackBannerTexture(TextureRegion texture)
    {
        _friendlyAttackBanner
            .SetTexture(texture)
            .SetWidth(35, UIUnit.Percentage)
            .SetHeight(15, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetEnemyStatBoxTexture(TextureRegion texture)
    {
        _enemyStatBox
            .SetTexture(texture)
            .SetWidth(15, UIUnit.Percentage)
            .SetHeight(20, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetFriendlyStatBoxTexture(TextureRegion texture)
    {
        _friendlyStatBox
            .SetTexture(texture)
            .SetWidth(15, UIUnit.Percentage)
            .SetHeight(20, UIUnit.Percentage);
        return this;
    }
}

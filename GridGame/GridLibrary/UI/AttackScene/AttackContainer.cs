using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Routing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

    private readonly UIElement _enemyTerrain = new();
    private readonly UIElement _friendlyTerrain = new();
    private Dictionary<string, TextureRegion> _terrainTypeToTexture = [];

    private readonly AnimatedElement _enemyAnimation = new();
    private readonly AnimatedElement _friendlyAnimation = new();

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

        _enemyTerrain
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI)
            .PadHorizontal(10, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(33, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom);
        _friendlyTerrain
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI)
            .PadHorizontal(10, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(33, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom)
            .SetSpriteEffects(SpriteEffects.FlipHorizontally);

        _enemyAnimation
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.05f)
            .PadHorizontal(25f, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(50, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom)
            .SetWidth(128 * 4 * 2, UIUnit.Pixels)
            .SetHeight(128 * 4 * 2, UIUnit.Pixels);
        _friendlyAnimation
            .SetParent(this)
            .SetLayerDepth(LayerDepths.StaticUI - 0.05f)
            .PadHorizontal(25f, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(50, UIUnit.Percentage, UIVerticalPaddingOrientation.FromBottom)
            .SetSpriteEffects(SpriteEffects.FlipHorizontally)
            .SetWidth(128 * 4 * 2, UIUnit.Pixels)
            .SetHeight(128 * 4 * 2, UIUnit.Pixels);
    }

    /// <summary>
    /// This is just for debugging until the attack/animation logic is in place.
    /// It'll let us continue to navigate around the grid.
    /// </summary>
    public override void HandleInput(GameTime gameTime, KeyboardInfo keyboardInfo)
    {
        if (keyboardInfo.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Z))
        {
            GridState.UnsetActiveEntity();
            Router.RouteTo(DefaultRoutes.Grid);
        }
    }

    public void Initialize()
    {
        (_, IEntity attacker) = GridState.Instance.ActiveEntity ?? throw new ArgumentException("No attacker found");
        Point cursorPosition = GridState.Instance.CursorPosition;
        if (!GridState.Instance.Entities.TryGetValue(cursorPosition, out IEntity? attacked))
            throw new ArgumentException("No attacked entity found");
        if (attacker.IsFriendly == attacked.IsFriendly)
            throw new ArgumentException($"No friendly fire! IsFriendly: {attacker.IsFriendly}");
        if (attacker.IsFriendly)
        {
            Point friendlyPosition = GridState.Instance.PotentialMove ?? throw new ArgumentException("No potential move found");
            GridTile friendlyTile = GridState.Instance.Tiles[friendlyPosition];
            GridTile enemyTile = GridState.Instance.Tiles[cursorPosition];
            SetFriendly(attacker, attacked, enemyTile);
            SetEnemy(attacked, attacker, friendlyTile);
            SetAnimationChain(_friendlyAnimation, _enemyAnimation);

            if (!_terrainTypeToTexture.TryGetValue(enemyTile.TileInfo.TileType, out TextureRegion? enemyTerrainTexture))
                throw new ArgumentException("No terrain mapped for enemy position");
            SetEnemyTerrain(enemyTerrainTexture);
            if (!_terrainTypeToTexture.TryGetValue(friendlyTile.TileInfo.TileType, out TextureRegion? friendlyTerrainTexture))
                throw new ArgumentException("No terrain mapped for friendly position");
            SetFriendlyTerrain(friendlyTerrainTexture);
        }
        else
        {
            //SetFriendly(attacked);
            //SetEnemy(attacker);
            //SetAnimationChain(_enemyAnimation, _friendlyAnimation);
            
            // TODO: Need to set terrain, but state checks might be different
        }
    }

    private static void SetAnimationChain(AnimatedElement attacker, AnimatedElement attacked)
    {
        attacked.ResetAnimation().Stop();
        attacker.ResetAnimation().SetOnAnimationEnd(() =>
        {
            attacker.ResetAnimation().Stop();
            attacked.Start();
        }).Start();
    }

    public AttackContainer SetEnemy(IEntity enemy, IEntity friendly, GridTile friendlyTile)
    {
        _enemyNameBanner.SetText(enemy.DisplayName);
        _enemyHealthBar.SetEntity(enemy);
        _enemyAttackBanner.SetMove(enemy.SelectedMove);
        _enemyStatBox.SetMove(enemy.SelectedMove);
        _enemyAnimation
            .SetAnimation(enemy.AttackAnimation)
            .SetFrameTrigger(enemy.SelectedMove.ContactFrame, () =>
            {
                // TODO: lots of logic around crit/dodge animations, all that jazz
                AttackResult attackResult = EntityAttackSimulator.Simulate(enemy, friendly, friendlyTile.TileInfo);
                Debug.WriteLine(attackResult);
                friendly.Health.Subtract(attackResult.Damage);
            });
        return this;
    }

    public AttackContainer SetFriendly(IEntity friendly, IEntity enemy, GridTile enemyTile)
    {
        _friendlyNameBanner.SetText(friendly.DisplayName);
        _friendlyHealthBar.SetEntity(friendly);
        _friendlyAttackBanner.SetMove(friendly.SelectedMove);
        _friendlyStatBox.SetMove(friendly.SelectedMove);
        _friendlyAnimation
            .SetAnimation(friendly.AttackAnimation)
            .SetFrameTrigger(friendly.SelectedMove.ContactFrame, () =>
            {
                // TODO: lots of logic around crit/dodge animations, all that jazz
                AttackResult attackResult = EntityAttackSimulator.Simulate(friendly, enemy, enemyTile.TileInfo);
                Debug.WriteLine(attackResult);
                enemy.Health.Subtract(attackResult.Damage);
            });
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

    public AttackContainer SetEnemyTerrain(TextureRegion texture)
    {
        _enemyTerrain
            .SetTexture(texture)
            .SetWidth(40, UIUnit.Percentage)
            .SetHeight(40, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetFriendlyTerrain(TextureRegion texture)
    {
        _friendlyTerrain
            .SetTexture(texture)
            .SetWidth(40, UIUnit.Percentage)
            .SetHeight(40, UIUnit.Percentage);
        return this;
    }

    public AttackContainer SetTerrainTypeToTexture(Dictionary<string, TextureRegion> terrainTypeToTexture)
    {
        _terrainTypeToTexture = terrainTypeToTexture;
        return this;
    }
}

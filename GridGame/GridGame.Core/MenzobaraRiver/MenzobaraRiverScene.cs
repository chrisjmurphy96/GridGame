using GridGame.Core.Entities;
using GridLibrary;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Graphics.TextureAtlas;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Ldtk;
using GridLibrary.Routing;
using GridLibrary.Scenes;
using GridLibrary.UI;
using GridLibrary.UI.AttackScene;
using GridLibrary.UI.ContextMenu;
using GridLibrary.UI.MapMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GridGame.Core.MenzobaraRiver;

public class MenzobaraRiverScene(
    AudioController audioController,
    SpriteBatch spriteBatch,
    GraphicsDevice graphicsDevice,
    SceneManager sceneManager,
    LdtkImporter ldtkImporter,
    Camera camera,
    KeyboardInfo keyboardInfo,
    AssetManager assetManager,
    UIRoot uiFactory,
    TextureAtlasLoader atlasLoader) : Scene
{
    private readonly AudioController _audio = audioController;
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    private readonly SceneManager _sceneManager = sceneManager;
    private readonly LdtkImporter _ldtkImporter = ldtkImporter;
    private readonly Camera _camera = camera;
    private readonly KeyboardInfo _keyboardInfo = keyboardInfo;
    private readonly AssetManager _assetManager = assetManager;
    private readonly UIRoot _uiRoot = uiFactory;
    private readonly TextureAtlasLoader _atlasLoader = atlasLoader;
    private readonly FrameCounter _frameCounter = new();
    private SpriteFont _font;

    private bool _showGrid = false;
    private AttackContainer _attackContainer;
    private Effect _darkenEffect;

    public override void Initialize()
    {
        _camera.Step = 64;
        base.Initialize();
    }

    public override void LoadContent()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        GridState.Save();

        _font = _assetManager.Load<MenzobaraRiverScene, SpriteFont>(Path.Combine("Fonts", "Hud"));
        LdtkProjectFile ldtkProjectFile = _ldtkImporter.Import(Path.Combine("Images", "basic-map.ldtk"));
        string levelName = "Menzobara_River";
        LdtkLevel level = ldtkProjectFile.GetLevelByName(levelName);
        LdtkLayerInstance layerInstance = level.GetTileLayer();
        string atlasName = Path.GetFileNameWithoutExtension(layerInstance.TilesetRelPath);
        Texture2D atlas = _assetManager.Load<MenzobaraRiverScene, Texture2D>(Path.Combine("Images", atlasName));
        TextureAtlas placeholderAtlas = _atlasLoader.Load<MenzobaraRiverScene>("Images", "placeholder-atlas-definition.json");
        AnimatedSprite cursorSprite = new()
        {
            Animation = placeholderAtlas.GetAnimation("cursor"),
            Scale = Vector2.One * 4,
            LayerDepth = LayerDepths.MovementArrow
        };
        Cursor cursor = new (camera, cursorSprite);
        TextureRegion attackOverlayTexture = placeholderAtlas.GetRegion("attackOverlay");
        MoveOverlay moveOverlay = new ()
        {
            MovementTexture = placeholderAtlas.GetRegion("movementOverlay"),
            AttackTexture = attackOverlayTexture
        };

        MovementArrow movementArrow = new(cursor, moveOverlay)
        {
            HeadTexture = placeholderAtlas.GetRegion("arrowHead"),
            StraightTexture = placeholderAtlas.GetRegion("arrowBody"),
            BendTexture = placeholderAtlas.GetRegion("arrowBend"),
            StartTexture = placeholderAtlas.GetRegion("arrowStart")
        };
        UIRoot
            .RootToCamera(movementArrow)
            .SetLayerDepth(LayerDepths.MovementArrow)
            .SetIsVisible(false);

        TextureAtlas goblinAttackAnimations = _atlasLoader.Load<MenzobaraRiverScene>("Images", "goblin-attack-placeholder-definition.json");
        Animation goblinAttackAnimation = goblinAttackAnimations.GetAnimation("regularAttack");
        Animation goblinCritAnimation = goblinAttackAnimations.GetAnimation("critAttack");
        Animation goblinDodgeAnimation = goblinAttackAnimations.GetAnimation("dodge");
        AnimationPool.Add("goblinRegularAttack", goblinAttackAnimation);
        AnimationPool.Add("goblinCritAttack", goblinCritAnimation);
        AnimationPool.Add("goblinDodge", goblinDodgeAnimation);

        TextureAtlas gigoughAttackAnimations = _atlasLoader.Load<MenzobaraRiverScene>("Images", "gigough-attack-v2-definition.json");
        Animation gigoughAttackAnimation = gigoughAttackAnimations.GetAnimation("regularAttack");
        Animation gigoughCritAnimation = gigoughAttackAnimations.GetAnimation("critAttack");
        Animation gigoughDodgeAnimation = gigoughAttackAnimations.GetAnimation("dodge");
        AnimationPool.Add("gigoughRegularAttack", gigoughAttackAnimation);
        AnimationPool.Add("gigoughCritAttack", gigoughCritAnimation);
        AnimationPool.Add("gigoughDodge", gigoughDodgeAnimation);
        LdtkLayerInstance entityLayer = level.GetEntityLayer();
        Animation gobboAnimation = new() { Frames = [placeholderAtlas.GetRegion("goblin")] };
        EntityMapAnimations goblinAnimations = new(gobboAnimation, gobboAnimation, gobboAnimation, gobboAnimation, gobboAnimation);
        Animation fighterAnim = new() { Frames = [placeholderAtlas.GetRegion("fighter")] };
        EntityMapAnimations fighterAnimations = new(fighterAnim, fighterAnim, fighterAnim, fighterAnim, fighterAnim);
        Dictionary<string, EntityMapAnimations> identifierToAnimations = new()
        {
            { Goblin.LdtkIdentifier, goblinAnimations },
            { Fighter.LdtkIdentifier, fighterAnimations }
        };
        Dictionary<Point, IEntity> entities = EntityFactory.CreateLayerEntities(entityLayer, identifierToAnimations);
        GridState.Instance.Entities = entities;

        Sprite attackOverlaySprite = new ()
        {
            TextureRegion = attackOverlayTexture,
            Scale = Vector2.One * 4,
            LayerDepth = LayerDepths.MoveOverlay
        };
        AttackOverlay attackOverlay = new()
        {
            AttackOverlaySprite = attackOverlaySprite
        };
        UIRoot
            .RootToCamera(attackOverlay)
            .SetIsVisible(false);

        TextureRegion menuTexture = placeholderAtlas.GetRegion("contextMenu");
        int menuScalar = 4;

        TextureRegion movePreviewTexture = placeholderAtlas.GetRegion("movePreview");

        MovePreview movePreview = new()
        {
            Font = _font,
            AttackOverlay = attackOverlay,
            Cursor = cursor
        };
        UIRoot
            .RootToScreen(movePreview)
            .SetTexture(movePreviewTexture)
            .SetWidth(movePreviewTexture.Width * menuScalar, UIUnit.Pixels)
            .SetHeight(movePreviewTexture.Height * menuScalar, UIUnit.Pixels)
            .PadHorizontal(4, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(25, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetIsVisible(false);

        ContextMenu contextMenu = new()
        {
            FocusTexture = placeholderAtlas.GetRegion("focus"),
            Font = _font,
            Cursor = cursor
        };
        UIRoot
            .RootToScreen(contextMenu)
            .SetLayerDepth(LayerDepths.StaticUI)
            .SetTexture(menuTexture)
            .SetWidth(menuScalar * menuTexture.Width, UIUnit.Pixels)
            .SetHeight(menuScalar * menuTexture.Height, UIUnit.Pixels)
            .PadHorizontal(4, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(25, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetIsVisible(false);

        Dictionary<string, TileInfo> enumNameToTileInfo = new()
        {
            { "Forest", new TileInfo("Forest") { DodgeModifier = 20, DefenseModifier = 1 } },
            { "River", new TileInfo("River") { CanWalk = false } },
            { "Bridge", new TileInfo("Bridge") },
            { "Grass", new TileInfo("Grass") }
        };

        GridTileList tiles = GridTileList.FromLevel(ldtkProjectFile, levelName, atlas, enumNameToTileInfo);
        GridState.Instance.Tiles = tiles;
        TileGrid tileGrid = new TileGrid(
            placeholderAtlas.GetRegion("gridOverlay"),
            scalar: 4,
            cursor,
            movementArrow,
            moveOverlay,
            placeholderAtlas.GetRegion("enemyMoveOverlay"),
            debugFont: null);
        tileGrid.SetLayerDepth(LayerDepths.Tiles);
        UIRoot.RootToCamera(tileGrid);
        //UIRoot.Focus(tileGrid);
        Router.AddDefaultRoutes(tileGrid, movementArrow, contextMenu, movePreview);
        
        _camera.CameraBounds = new()
        {
            X = 0,
            Y = 0,
            Width = level.LayerWidth * tileGrid.Scalar,
            Height = level.LayerHeight * tileGrid.Scalar
        };

        TextureAtlas attackSceneTerrainAtlas = _atlasLoader.Load<MenzobaraRiverScene>("Images", "attack-scene-terrain-definition.json");
        Dictionary<string, TextureRegion> terrainTypeToTexture = new()
        {
            { "Forest", attackSceneTerrainAtlas.GetRegion("forest") },
            { "Grass", attackSceneTerrainAtlas.GetRegion("grass") }
        };
        TextureAtlas attackSceneAtlas = _atlasLoader.Load<MenzobaraRiverScene>("Images", "attack-scene-definition.json");
        AttackContainer attackContainer = new ();
        attackContainer
            .SetEnemyNameBannerTexture(attackSceneAtlas.GetRegion("enemyNameBanner"))
            .SetFriendlyNameBannerTexture(attackSceneAtlas.GetRegion("friendlyNameBanner"))
            .SetHealthBarTextures(attackSceneAtlas.GetRegion("healthBarActive"), attackSceneAtlas.GetRegion("healthBarInactive"))
            .SetEnemyHealthBannerTexture(attackSceneAtlas.GetRegion("enemyHealthBanner"))
            .SetFriendlyHealthBannerTexture(attackSceneAtlas.GetRegion("friendlyHealthBanner"))
            .SetEnemyAttackBannerTexture(attackSceneAtlas.GetRegion("enemyAttackBanner"))
            .SetFriendlyAttackBannerTexture(attackSceneAtlas.GetRegion("friendlyAttackBanner"))
            .SetEnemyStatBoxTexture(attackSceneAtlas.GetRegion("enemyStatBox"))
            .SetFriendlyStatBoxTexture(attackSceneAtlas.GetRegion("friendlyStatBox"))
            .SetTerrainTypeToTexture(terrainTypeToTexture)
            .SetFont(_font)
            .SetIsVisible(false);
        UIRoot.RootToScreen(attackContainer);
        Router.RegisterRoute(DefaultRoutes.AttackContainer, attackContainer);
        _attackContainer = attackContainer;

        MovementAnimation movementAnimation = new(cursor);
        movementAnimation.SetIsVisible(false);
        UIRoot.RootToCamera(movementAnimation);
        Router.RegisterRoute(DefaultRoutes.MovementAnimation, movementAnimation);

        _darkenEffect = _assetManager.Load<MenzobaraRiverScene, Effect>(Path.Combine("Effects", "Darken"));

        TextureRegion mapInfoTexture = placeholderAtlas.GetRegion("mapInfo");
        TerrainInfo terrainInfo = new();
        terrainInfo
            .SetFont(_font)
            .SetTextureNoDefaults(mapInfoTexture)
            .PadHorizontal(16, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(16, UIUnit.Pixels, UIVerticalPaddingOrientation.FromTop)
            .SetWidth(64 * 4, UIUnit.Pixels)
            .SetHeight(16 * 4, UIUnit.Pixels)
            .SetOpacity(0.85f);
        UIRoot.RootToScreen(terrainInfo);

        CharacterInfo characterInfo = new();
        characterInfo
            .SetFont(_font)
            .SetTextureNoDefaults(mapInfoTexture)
            .PadHorizontal(16, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical(16, UIUnit.Pixels, UIVerticalPaddingOrientation.FromTop)
            .SetWidth(64 * 4, UIUnit.Pixels)
            .SetHeight(16 * 4, UIUnit.Pixels)
            .SetOpacity(0.85f);
        UIRoot.RootToScreen(characterInfo);

        TextureAtlas phaseBannersAtlas = _atlasLoader.Load<MenzobaraRiverScene>("Images", "phase-banners.json");
        PhaseBanner playerPhaseBanner = new();
        playerPhaseBanner
            .SetFont(_font)
            .SetText("Player Phase")
            .SetTransitionTimeSpan(TimeSpan.FromSeconds(3))
            .SetTextureNoDefaults(phaseBannersAtlas.GetRegion("playerPhase"))
            .PadHorizontal(0, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .SetWidth(100, UIUnit.Percentage)
            .PadVertical(40, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetHeight(20, UIUnit.Percentage)
            .SetOpacity(0.85f)
            .SetIsVisible(true);
        Router.RegisterRoute(DefaultRoutes.PlayerPhaseBanner, playerPhaseBanner);
        UIRoot.RootToScreen(playerPhaseBanner);

        PhaseBanner enemyPhaseBanner = new();
        enemyPhaseBanner
            .SetFont(_font)
            .SetText("Enemy Phase")
            .SetTransitionTimeSpan(TimeSpan.FromSeconds(3))
            .SetTextureNoDefaults(phaseBannersAtlas.GetRegion("enemyPhase"))
            .PadHorizontal(0, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .SetWidth(100, UIUnit.Percentage)
            .PadVertical(40, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetHeight(20, UIUnit.Percentage)
            .SetOpacity(0.85f)
            .SetIsVisible(false);
        Router.RegisterRoute(DefaultRoutes.EnemyPhaseBanner, enemyPhaseBanner);
        UIRoot.RootToScreen(enemyPhaseBanner);

        TextureAtlas staticUIAtlas = _atlasLoader.Load<MenzobaraRiverScene>("Images", "static-ui.json");
        MapMenu mapMenu = new();
        mapMenu
            .SetFont(_font)
            .SetFocusTexture(staticUIAtlas.GetRegion("focus"))
            .SetTexture(staticUIAtlas.GetRegion("mapMenu"))
            .SetOpacity(0.85f)
            .SetIsVisible(false)
            .PadHorizontal(45, UIUnit.Percentage, UIHorizontalPaddingOrientation.FromLeft)
            .PadVertical(42, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetHeight(32 * 4, UIUnit.Pixels)
            .SetWidth(32 * 4, UIUnit.Pixels);
        Router.RegisterRoute(DefaultRoutes.MapMenu, mapMenu);
        UIRoot.RootToScreen(mapMenu);

        stopwatch.Stop();
        Router.RouteWithoutHistory(DefaultRoutes.PlayerPhaseBanner);

        Debug.WriteLine(stopwatch.Elapsed);
    }

    public override void UnloadContent()
    {
        _assetManager.Unload<MenzobaraRiverScene>();
    }

    public override void Update(GameTime gameTime)
    {
        if (_keyboardInfo.WasKeyJustPressed(Keys.H))
            _sceneManager.ChangeScene<OtherScene>();
                
        // if (_keyboardInfo.WasKeyJustPressed(Keys.C))
        //     _camera.Center();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemPlus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Add))
        //     _camera.ZoomIn();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemMinus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Subtract))
        //     _camera.ZoomOut();

        // TODO: showing the grid can be a menu item. I don't think it's something you need to be able to quickly toggle
        //if (_inputInfo.TogglePressed())
        //{
        //    ToggleGrid();
        //}
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.MonoGameOrange);

        Effect? effect = _attackContainer.IsVisible ? _darkenEffect : null;
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetViewMatrix(),
            sortMode: SpriteSortMode.BackToFront,
            effect: effect);
        _uiRoot.DrawCameraElements();
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.BackToFront);
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);
        string fps = string.Format("FPS: {0}", (int)_frameCounter.AverageFramesPerSecond);
        _spriteBatch.DrawString(_font, fps, new Vector2(x: 500, y: 0), Color.White);
        _uiRoot.DrawScreenElements();
        _spriteBatch.End();
    }

    private void ToggleGrid() => _showGrid = !_showGrid;
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GridGame.Core.Entities;
using GridLibrary;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Ldtk;
using GridLibrary.Routing;
using GridLibrary.Scenes;
using GridLibrary.UI;
using GridLibrary.UI.AttackScene;
using GridLibrary.UI.ContextMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;

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
    UIRoot uiFactory) : Scene
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
    private readonly FrameCounter _frameCounter = new();
    private TileGrid _grid;
    private SpriteFont _font;
    private TileInfo _activeTileInfo;

    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(100);
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
        TextureRegion cursorFrameOne = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 48, y: 16, width: 16, height: 16)
        };
        TextureRegion cursorFrameTwo = new ()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 48, y: 32, width: 16, height: 16)
        };
        Animation cursorAnimation = new()
        {
            Frames = [cursorFrameOne, cursorFrameTwo],
            Delay = TimeSpan.FromMilliseconds(500)
        };
        AnimatedSprite cursorSprite = new()
        {
            Animation = cursorAnimation,
            Scale = Vector2.One * 4,
            LayerDepth = LayerDepths.MovementArrow
        };
        Cursor cursor = new (camera, cursorSprite);
        TextureRegion gridOverlayTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 0, y: 32, width: 16, height: 16)
        };   

        TextureRegion movementOverlayTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(48, 48, 16, 16)
        };
        TextureRegion attackOverlayTexture = new ()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(32, 48, 16, 16)
        };

        MoveOverlay moveOverlay = new ()
        {
            MovementTexture = movementOverlayTexture,
            AttackTexture = attackOverlayTexture
        };

        TextureRegion arrowHead = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 32, y: 0, width: 16, height: 16)
        };
        TextureRegion arrowBody = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 48, y: 0, width: 16, height: 16)
        };
        TextureRegion arrowBend = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 32, y: 32, width: 16, height: 16)
        };
        TextureRegion arrowStart = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 32, y: 16, width: 16, height: 16)
        };
        MovementArrow movementArrow = new(cursor, moveOverlay)
        {
            HeadTexture = arrowHead,
            StraightTexture = arrowBody,
            BendTexture = arrowBend,
            StartTexture = arrowStart
        };
        UIRoot
            .RootToCamera(movementArrow)
            .SetLayerDepth(LayerDepths.MovementArrow)
            .SetIsVisible(false);

        LdtkLayerInstance entityLayer = level.GetEntityLayer();
        TextureRegion goblinTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(0, 48, 16, 16)
        };
        TextureRegion fighterTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(16, 48, 16, 16)
        };
        Dictionary<string, TextureRegion> identifierToTexture = new()
        {
            { Goblin.LdtkIdentifier, goblinTexture },
            { Fighter.LdtkIdentifier, fighterTexture }
        };
        Dictionary<Point, IEntity> entities = EntityFactory.CreateLayerEntities(entityLayer, identifierToTexture);
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

        TextureRegion menuTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(64, 0, 32, 48)
        };
        int menuScalar = 4;

        TextureRegion movePreviewTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(64, 48, 64, 48)
        };

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
        
        TextureRegion focusTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(96, 0, 28, 1)
        };
        ContextMenu contextMenu = new()
        {
            FocusTexture = focusTexture,
            KeyboardInfo = _keyboardInfo,
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

        TextureRegion enemyMoveOverlayTexture = new ()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(48, 64, 16, 16)
        };

        Dictionary<string, TileInfo> enumNameToTileInfo = new()
        {
            { "Forest", new TileInfo("Forest") { DodgeModifier = 20, ArmorModifier = 1 } },
            { "River", new TileInfo("River") { CanWalk = false } },
            { "Bridge", new TileInfo("Bridge") },
            { "Grass", new TileInfo("Grass") }
        };

        GridTileList tiles = GridTileList.FromLevel(ldtkProjectFile, levelName, atlas, enumNameToTileInfo);
        GridState.Instance.Tiles = tiles;
        _grid = new TileGrid(
            gridOverlayTexture,
            scalar: 4,
            cursor,
            movementArrow,
            moveOverlay,
            enemyMoveOverlayTexture,
            debugFont: null);
        // don't attach Grid to the root, we want to control how it's drawn
        UIRoot.Focus(_grid);
        Router.AddDefaultRoutes(_grid, movementArrow, contextMenu, movePreview);
        
        _camera.CameraBounds = new()
        {
            X = 0,
            Y = 0,
            Width = level.LayerWidth * _grid.Scalar,
            Height = level.LayerHeight * _grid.Scalar
        };

        Texture2D attackSceneAtlas = _assetManager.Load<MenzobaraRiverScene, Texture2D>(Path.Combine("Images", "attack-scene"));
        TextureRegion enemyNameBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 0, 48, 16)
        };
        TextureRegion friendlyNameBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(48, 0, 48, 16)
        };
        TextureRegion healthBarActiveTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 96, 3, 9)
        };
        TextureRegion healthBarInactiveTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(3, 96, 3, 9)
        };
        TextureRegion enemyHealthBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 64, 96, 16)
        };
        TextureRegion friendlyHealthBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 80, 96, 16)
        };
        TextureRegion enemyAttackBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 16, 48, 16)
        };
        TextureRegion friendlyAttackBannerTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(48, 16, 48, 16)
        };
        TextureRegion enemyStatBoxTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(0, 32, 25, 25)
        };
        TextureRegion friendlyStatBoxTexture = new()
        {
            Texture = attackSceneAtlas,
            SourceRectangle = new Rectangle(71, 32, 25, 25)
        };
        Texture2D attackSceneTerrainAtlas = _assetManager.Load<MenzobaraRiverScene, Texture2D>(Path.Combine("Images", "attack-scene-terrain"));
        TextureRegion forestTerrainTexture = new()
        {
            Texture = attackSceneTerrainAtlas,
            SourceRectangle = new Rectangle(0, 0, 32, 19)
        };
        TextureRegion grassTerrainTexture = new()
        {
            Texture = attackSceneTerrainAtlas,
            SourceRectangle = new Rectangle(32, 0, 32, 19)
        };
        Dictionary<string, TextureRegion> terrainTypeToTexture = new()
        {
            { "Forest", forestTerrainTexture },
            { "Grass", grassTerrainTexture }
        };
        AttackContainer attackContainer = new ();
        attackContainer
            //.SetEnemy(GridState.Instance.Entities.ElementAt(0).Value)
            //.SetFriendly(GridState.Instance.Entities.ElementAt(1).Value)
            .SetEnemyNameBannerTexture(enemyNameBannerTexture)
            .SetFriendlyNameBannerTexture(friendlyNameBannerTexture)
            .SetHealthBarTextures(healthBarActiveTexture, healthBarInactiveTexture)
            .SetEnemyHealthBannerTexture(enemyHealthBannerTexture)
            .SetFriendlyHealthBannerTexture(friendlyHealthBannerTexture)
            .SetEnemyAttackBannerTexture(enemyAttackBannerTexture)
            .SetFriendlyAttackBannerTexture(friendlyAttackBannerTexture)
            .SetEnemyStatBoxTexture(enemyStatBoxTexture)
            .SetFriendlyStatBoxTexture(friendlyStatBoxTexture)
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

        stopwatch.Stop();


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
        
        _activeTileInfo = GridState.Instance.ActiveTile?.TileInfo ?? new(string.Empty);
        
        // if (_keyboardInfo.WasKeyJustPressed(Keys.C))
        //     _camera.Center();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemPlus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Add))
        //     _camera.ZoomIn();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemMinus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Subtract))
        //     _camera.ZoomOut();
        if (_keyboardInfo.WasKeyJustPressed(Keys.G))
        {
            ToggleGrid();
        }
        _grid.Update(gameTime);
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
        _grid.Draw(_spriteBatch, _camera.CameraBounds);
        _uiRoot.DrawCameraElements();
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.BackToFront);
        //_spriteBatch.DrawString(_font, _activeTileInfo.ToString(_grid.Cursor.Position), Vector2.Zero, Color.White);
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);
        string fps = string.Format("FPS: {0}", (int)_frameCounter.AverageFramesPerSecond);
        _spriteBatch.DrawString(_font, fps, new Vector2(x: 500, y: 0), Color.White);
        _uiRoot.DrawScreenElements();
        _spriteBatch.End();
    }

    private void ToggleGrid() => _showGrid = !_showGrid;
}

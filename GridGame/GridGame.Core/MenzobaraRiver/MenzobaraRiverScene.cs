using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GridGame.Core.Entities;
using GridGame.Core.Tiles;
using GridLibrary;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Ldtk;
using GridLibrary.Scenes;
using GridLibrary.UI;
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
    private Grid<TileType> _grid;
    private SpriteFont _font;
    private TileInfo _activeTileInfo;

    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(100);
    private bool _showGrid = false;
    private UIElement _testUIElement;

    public override void Initialize()
    {
        _camera.Step = 64;
        base.Initialize();
    }

    public override void LoadContent()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
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
            Scale = Vector2.One * 4
        };
        Cursor cursor = new ()
        {
            CursorSprite = cursorSprite
        };
        TextureRegion gridOverlayTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 0, y: 32, width: 16, height: 16)
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

        MovementArrow<TileType> movementArrow = new()
        {
            HeadTexture = arrowHead,
            StraightTexture = arrowBody,
            BendTexture = arrowBend,
            StartTexture = arrowStart
        };

        TextureRegion movementOverlay = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(48, 48, 16, 16)
        };
        TextureRegion attackOverlay = new ()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(32, 48, 16, 16)
        };

        MoveOverlay<TileType> moveOverlay = new ()
        {
            MovementTexture = movementOverlay,
            AttackTexture = attackOverlay
        };

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
            Font = _font
        };
        _uiRoot
            .AddToRoot<MovePreview>(movePreview)
            .SetTexture<MovePreview>(movePreviewTexture)
            .SetWidth<MovePreview>(movePreviewTexture.Width * menuScalar, UIUnit.Pixels)
            .SetHeight<MovePreview>(movePreviewTexture.Height * menuScalar, UIUnit.Pixels)
            .PadHorizontal<MovePreview>(4, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical<MovePreview>(25, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetIsVisible<MovePreview>(false);
        
        TextureRegion focusTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(96, 0, 28, 1)
        };
        ContextMenu contextMenu = new()
        {
            FocusTexture = focusTexture,
            KeyboardInfo = _keyboardInfo,
            MovePreview = movePreview,
            Font = _font,
        };
        _uiRoot
            .AddToRoot<ContextMenu>(contextMenu)
            .SetTexture<ContextMenu>(menuTexture)
            .SetWidth<UIElement>(menuScalar * menuTexture.Width, UIUnit.Pixels)
            .SetHeight<UIElement>(menuScalar * menuTexture.Height, UIUnit.Pixels)
            .PadHorizontal<ContextMenu>(4, UIUnit.Pixels, UIHorizontalPaddingOrientation.FromRight)
            .PadVertical<ContextMenu>(25, UIUnit.Percentage, UIVerticalPaddingOrientation.FromTop)
            .SetIsVisible<ContextMenu>(false);

        TextureRegion enemyMoveOverlayTexture = new ()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(48, 64, 16, 16)
        };

        _grid = new Grid<TileType>(
            ldtkProjectFile,
            levelName,
            atlas,
            gridOverlayTexture,
            scalar: 4,
            cursor,
            movementArrow,
            moveOverlay,
            enemyMoveOverlayTexture,
            contextMenu,
            entities,
            _uiRoot,
            (point, texture, tileType) => new RiverGridTile(point, texture, tileType),
            (point, animation, tileType) => new AnimatedRiverGridTile(point, animation, tileType));
        
        _camera.CameraBounds = new()
        {
            X = 0,
            Y = 0,
            Width = level.LayerWidth * _grid.Scalar,
            Height = level.LayerHeight * _grid.Scalar
        };

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
        
        TileType tileType = _grid.ActiveTile.TileType;
        _activeTileInfo = tileType.GetTileInfo();
        
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
        _grid.Update(gameTime, _keyboardInfo, _camera);
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.MonoGameOrange);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        _spriteBatch.Draw(_grid, _showGrid);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.DrawString(_font, _activeTileInfo.ToString(_grid.Cursor.Position), Vector2.Zero, Color.White);
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);
        string fps = string.Format("FPS: {0}", (int)_frameCounter.AverageFramesPerSecond);
        _spriteBatch.DrawString(_font, fps, new Vector2(x: 500, y: 0), Color.White);
        _spriteBatch.End();
    }

    private void ToggleGrid() => _showGrid = !_showGrid;
}

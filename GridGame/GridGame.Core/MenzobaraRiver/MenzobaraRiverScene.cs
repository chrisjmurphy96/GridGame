using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GridGame.Core.Tiles;
using GridLibrary;
using GridLibrary.Graphics;
using GridLibrary.Grid;
using GridLibrary.Input;
using GridLibrary.Ldtk;
using GridLibrary.Scenes;
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
    AssetManager assetManager) : Scene
{
    private readonly AudioController _audio = audioController;
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    private readonly SceneManager _sceneManager = sceneManager;
    private readonly LdtkImporter _ldtkImporter = ldtkImporter;
    private readonly Camera _camera = camera;
    private readonly KeyboardInfo _keyboardInfo = keyboardInfo;
    private readonly AssetManager _assetManager = assetManager;
    private readonly FrameCounter _frameCounter = new();
    private Grid<TileType> _grid;
    private SpriteFont _font;
    private TileInfo _activeTileInfo;

    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(100);
    private bool _showGrid = false;

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
        LdtkLayerInstance layerInstance = level.GetDefaultLayer();
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

        MovementArrow<TileType> movementArrow = new()
        {
            HeadTexture = gridOverlayTexture,
            StraightTexture = gridOverlayTexture,
            BendTexture = gridOverlayTexture,
            StartTexture = gridOverlayTexture
        };

        _grid = new Grid<TileType>(ldtkProjectFile, levelName, atlas, gridOverlayTexture, scalar: 4, cursor, movementArrow);
        
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
        if (_keyboardInfo.WasKeyJustPressed(Keys.Down) ||
            _keyboardInfo.IsKeyHeldDown(Keys.Down, MoveDelay))
        {
            _keyboardInfo.ResetKeyHold(Keys.Down);
            _grid.MoveCursorDown(_camera);
        }
        if (_keyboardInfo.WasKeyJustPressed(Keys.Up) ||
            _keyboardInfo.IsKeyHeldDown(Keys.Up, MoveDelay))
        {
            _keyboardInfo.ResetKeyHold(Keys.Up);
            _grid.MoveCursorUp(_camera);
        }
        if (_keyboardInfo.WasKeyJustPressed(Keys.Right) ||
            _keyboardInfo.IsKeyHeldDown(Keys.Right, MoveDelay))
        {   
            _keyboardInfo.ResetKeyHold(Keys.Right);
            _grid.MoveCursorRight(_camera);
        }
        if (_keyboardInfo.WasKeyJustPressed(Keys.Left) ||
            _keyboardInfo.IsKeyHeldDown(Keys.Left, MoveDelay))
        {
            _keyboardInfo.ResetKeyHold(Keys.Left);
            _grid.MoveCursorLeft(_camera);
        }
        if (_keyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            _grid.StartPath();
        }
        if (_keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            _grid.CancelPath();
        }
        TileType tileType = _grid.ActiveTile.TileType;
        // _activeTileInfo = tileType.GetTileInfo();
        _grid.Update(gameTime);
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
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.MonoGameOrange);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        _spriteBatch.Draw(_grid, _showGrid);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
        // _spriteBatch.DrawString(_font, _activeTileInfo.ToString(), Vector2.Zero, Color.White);
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);
        string fps = string.Format("FPS: {0}", (int)_frameCounter.AverageFramesPerSecond);
        _spriteBatch.DrawString(_font, fps, new Vector2(x: 500, y: 0), Color.White);
        _spriteBatch.End();
    }

    private void ToggleGrid() => _showGrid = !_showGrid;
}

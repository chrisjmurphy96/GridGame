using System;
using System.Collections.Generic;
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
    private Grid _grid;
    private SpriteFont _font;

    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(100);
    private bool _showGrid = false;

    private readonly Dictionary<TileType, TileInfo> _tileInfos = new()
    {
        [TileType.Forest] = new TileInfo { DodgeChanceModifier = 20 },
        [TileType.River] = new TileInfo { CanWalk = false },
        [TileType.Bridge] = new TileInfo(),
        [TileType.Grass] = new TileInfo()
    };

    public override void Initialize()
    {
        _camera.Step = 64;
        base.Initialize();
    }

    public override void LoadContent()
    {
        _font = _assetManager.Load<MenzobaraRiverScene, SpriteFont>(Path.Combine("Fonts", "Hud"));
        LdtkProjectFile ldtkProjectFile = _ldtkImporter.Import(Path.Combine("Images", "basic-map.ldtk"));
        LdtkLevel level = ldtkProjectFile.GetLevelByName("Menzobara_River");
        LdtkLayerInstance layerInstance = level.GetDefaultLayer();
        string atlasName = Path.GetFileNameWithoutExtension(layerInstance.TilesetRelPath);
        Texture2D atlas = _assetManager.Load<MenzobaraRiverScene, Texture2D>(Path.Combine("Images", atlasName));
        TextureRegion cursorFrame = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 48, y: 16, width: 16, height: 16)
        };
        Animation cursorAnimation = new()
        {
            Frames = [cursorFrame]
        };
        AnimatedSprite cursorSprite = new()
        {
            Animation = cursorAnimation,
            Scale = Vector2.One * 4
        };
        TextureRegion gridOverlayTexture = new()
        {
            Texture = atlas,
            SourceRectangle = new Rectangle(x: 0, y: 32, width: 16, height: 16)
        };

        _grid = new Grid(map: level, atlas, gridOverlayTexture, scalar: 4, cursorSprite);
        
        _camera.CameraBounds = new()
        {
            X = 0,
            Y = 0,
            Width = level.LayerWidth * _grid.Scalar,
            Height = level.LayerHeight * _grid.Scalar
        };
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
        // if (_keyboardInfo.WasKeyJustPressed(Keys.C))
        //     _camera.Center();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemPlus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Add))
        //     _camera.ZoomIn();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.OemMinus) ||
        //     _keyboardInfo.WasKeyJustPressed(Keys.Subtract))
        //     _camera.ZoomOut();
        // if (_keyboardInfo.WasKeyJustPressed(Keys.G))
        // {
        //     ToggleGrid();
        // }
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.MonoGameOrange);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        _spriteBatch.Draw(_grid);
        _spriteBatch.End();
        _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
        TileType tileType = (TileType)_grid.ActiveTile.TileType;
        if (!Enum.IsDefined<TileType>(tileType))
        {
            _spriteBatch.DrawString(_font, $"{_grid.ActiveTile.TileType} is not a valid {nameof(TileType)}", Vector2.Zero, Color.White);
        }
        else
        {
            TileInfo info = _tileInfos[(TileType)_grid.ActiveTile.TileType];
            _spriteBatch.DrawString(_font, info.ToString(), Vector2.Zero, Color.White);
        }
        _spriteBatch.End();
    }

    private void ToggleGrid() => _showGrid = !_showGrid;
}

using System;
using System.IO;
using System.Linq;
using GridGame.Core.MenzobaraRiver;
using GridLibrary;
using GridLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridGame.Core;

public class OtherScene(
    SpriteBatch spriteBatch,
    GraphicsDevice graphicsDevice,
    SceneManager sceneManager,
    AssetManager assetManager) : Scene
{
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    private readonly SceneManager _sceneManager = sceneManager;
    private readonly AssetManager _assetManager = assetManager;
    private SpriteFont _font;

    public override void LoadContent()
    {
        _font = _assetManager.Load<OtherScene, SpriteFont>(Path.Combine("Fonts", "Hud"));
    }

    public override void UnloadContent()
    {
        _assetManager.Unload<OtherScene>();
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.T))
            _sceneManager.ChangeScene<MenzobaraRiverScene>();
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.MonoGameOrange);
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, string.Join(string.Empty, "Horse horse horse!".Reverse()), Vector2.Zero, Color.White);
        _spriteBatch.End();
    }
}
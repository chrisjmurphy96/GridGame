using System;
using GridLibrary.Input;
using GridLibrary.Scenes;
using GridLibrary.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

namespace GridLibrary;

public class Core : Game
{
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    internal ServiceCollection ServiceCollection = [];

    protected SceneManager _sceneManager;
    protected KeyboardInfo _keyboardInfo;
    private UIRoot _uiRoot;


    public Core()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this)
        {
            // Set the graphics defaults.
            PreferredBackBufferWidth = 1920,
            PreferredBackBufferHeight = 1080,
            IsFullScreen = true
        };

        // Creates the GraphicsDevice.
        _graphicsDeviceManager.ApplyChanges();

        // Share GraphicsDeviceManager as a service.
        Services.AddService<GraphicsDeviceManager>(_graphicsDeviceManager);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    public void SetScreen(string title, int width, int height, bool fullScreen)
    {
        _graphicsDeviceManager.PreferredBackBufferWidth = width;
        _graphicsDeviceManager.PreferredBackBufferHeight = height;
        _graphicsDeviceManager.IsFullScreen = fullScreen;
        _graphicsDeviceManager.ApplyChanges();

        Window.Title = title;
    }

    /// <summary>
    /// Takes an initial <see cref="Scene" /> as a type argument.
    /// </summary>
    internal ServiceProvider BuildServiceProvider<T>() where T : Scene
    {
        ServiceCollection.AddSingleton<SceneManager>();
        ServiceCollection.AddSingleton<KeyboardInfo>();
        ServiceCollection.AddSingleton<UIRoot>();
        ServiceProvider serviceProvider = ServiceCollection.BuildServiceProvider();
        _sceneManager = serviceProvider.GetService<SceneManager>() ?? throw new Exception($"Failed to create {nameof(SceneManager)}");
        _keyboardInfo = serviceProvider.GetService<KeyboardInfo>() ?? throw new Exception($"Failed to create {nameof(KeyboardInfo)}");
        _uiRoot = serviceProvider.GetService<UIRoot>() ?? throw new Exception($"Failed to create {nameof(UIRoot)}");
        _sceneManager.ChangeScene<T>();
        return serviceProvider;
    } 

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent() { }

    protected override void Update(GameTime gameTime)
    {
        // update keyboard first since scenes will use it
        _keyboardInfo.Update(gameTime);
        _sceneManager.Update(gameTime);
        _uiRoot.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        _sceneManager.Draw(gameTime);
        // If there are any UI elements, draw them.
        _uiRoot.Draw();

        base.Draw(gameTime);
    }
}

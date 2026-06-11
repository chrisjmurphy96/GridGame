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
    private InputInfo _inputInfo;
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

        // This setting (VSync) is true by default. So our frame rate won't exceed the
        // monitor's refresh rate. Setting it to false allows our framerate to boost to the
        // moon, but for no real benefit. This is an interesting way to test actual performance though!
        // _graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;

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
        ServiceCollection.AddSingleton<GamePadInfo>();
        ServiceCollection.AddSingleton<InputInfo>();
        ServiceCollection.AddSingleton<UIRoot>();
        ServiceProvider serviceProvider = ServiceCollection.BuildServiceProvider();
        _sceneManager = serviceProvider.GetService<SceneManager>() ?? throw new Exception($"Failed to create {nameof(SceneManager)}");
        _inputInfo = serviceProvider.GetService<InputInfo>() ?? throw new Exception($"Failed to create {nameof(InputInfo)}");
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
        // update input first since scenes will use it
        _inputInfo.Update(gameTime);
        _sceneManager.Update(gameTime);
        _uiRoot.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        _sceneManager.Draw(gameTime);

        base.Draw(gameTime);
    }
}

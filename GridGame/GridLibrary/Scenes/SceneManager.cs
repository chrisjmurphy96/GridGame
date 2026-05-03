using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;

namespace GridLibrary.Scenes;

public class SceneManager(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private Scene? _activeScene = null;
    private Scene? _nextScene = null;

    public void ChangeScene<T>() where T : Scene
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (_activeScene?.GetType() != typeof(T))
        {
            _nextScene = GetScene<T>();
        }
    }

    public void Update(GameTime gameTime)
    {
        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (_nextScene is not null)
            TransitionScene();

        _activeScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime) => _activeScene?.Draw(gameTime);

    private T GetScene<T>() where T : Scene => _serviceProvider.GetService<T>() ?? throw new ArgumentException($"{typeof(T)} could not be created");

    private void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        _activeScene?.UnloadContent();

        // Force the garbage collector to collect to ensure memory is cleared.
        // I don't think we need this. Even with the way this was originally written.
        // GC.Collect();

        // Change the currently active scene to the new scene.
        _activeScene = _nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        _nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        _activeScene?.Initialize();
    }
}
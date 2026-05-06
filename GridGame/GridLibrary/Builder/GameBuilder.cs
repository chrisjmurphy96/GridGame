using Microsoft.Extensions.DependencyInjection;
using GridLibrary.Scenes;

namespace GridLibrary.Builder;

public class GameBuilder<T> where T : Core, new()
{
    public ServiceCollection Services => _game.ServiceCollection; //= [];
    private readonly T _game;

    public GameBuilder()
    {
        _game = new ();
        _game.ServiceCollection.AddSingleton(_game.GraphicsDevice);
        _game.ServiceCollection.AddSingleton(_game.Content);
    }

    public void SetScreen(string title, int width, int height, bool fullScreen)
    {
        _game.SetScreen(title, width, height, fullScreen);
    } 

    /// <summary>
    /// Takes a <see cref="Scene" /> as a type argument. This will be the initial scene of the game.
    /// </summary>
    public T Build<S>() where S : Scene
    {
        _game.BuildServiceProvider<S>();
        return _game;
    }
}
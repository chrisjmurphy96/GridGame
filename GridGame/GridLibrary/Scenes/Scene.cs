using Microsoft.Xna.Framework;

namespace GridLibrary.Scenes;

/// <summary>
/// The idea of scenes is nice, however, the way it currently plays
/// with ContentManager is less nice. In lieu of a better solution,
/// I have a list of Assets I keep track of so I can unload them all
/// when we're done, similar to subscription tracking in Angular.
/// </summary>
public abstract class Scene
{
    public virtual void Initialize()
    {
        LoadContent();
    }

    public abstract void LoadContent();

    /// <summary>
    /// This could break if more than one scene is alive and share assets.
    /// Then when one of the scenes is unloaded, the references will break.
    /// If that ends up being an issue, could look at changing to something like
    /// a dictionary that holds a list of current subscribers?
    /// </summary>
    public abstract void UnloadContent();

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);
}
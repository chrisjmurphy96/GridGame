using GridGame.Core;

internal class Program
{
    /// <summary>
    /// The main entry point for the application. 
    /// This creates an instance of your game and calls it's Run() method 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        // using var game = new GridGame.Core.GridGame();
        // game.Run();
        // GridGame.Core.GridGame.Instance.Run();
        RegisterServices.Register().Run();
    }
}
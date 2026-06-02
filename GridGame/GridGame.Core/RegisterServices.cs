using GridGame.Core.MenzobaraRiver;
using GridLibrary;
using GridLibrary.Builder;
using GridLibrary.Graphics.TextureAtlas;
using GridLibrary.Grid;
using GridLibrary.Ldtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Audio;

namespace GridGame.Core;

public class RegisterServices
{
    public static GridGame Register()
    {
        GameBuilder<GridGame> gameBuilder = new();
        
        gameBuilder.Services.AddSingleton<AssetManager>();
        gameBuilder.Services.AddTransient<MenzobaraRiverScene>();
        gameBuilder.Services.AddTransient<OtherScene>();
        gameBuilder.Services.AddTransient<LdtkImporter>();
        gameBuilder.Services.AddSingleton<Camera>();
        gameBuilder.Services.AddSingleton<GridState>();
        gameBuilder.Services.AddTransient<TextureAtlasLoader>();
        gameBuilder.SetScreen(title: "Menzobara's Revenge", width: 1280, height: 720, fullScreen: false);
        GridGame gridGame = gameBuilder.Build<MenzobaraRiverScene>();

        return gridGame;
    }
}
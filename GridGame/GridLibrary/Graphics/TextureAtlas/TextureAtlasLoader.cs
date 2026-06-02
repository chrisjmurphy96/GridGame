using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Text.Json;

namespace GridLibrary.Graphics.TextureAtlas;

public class TextureAtlasLoader
{
    private readonly AssetManager _assetManager;
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    public TextureAtlasLoader(AssetManager contentManager)
    {
        _assetManager = contentManager;
    }

    public TextureAtlas Load<Owner>(string directory, string fileName)
    {
        string fieldsPath = Path.Combine(_assetManager.RootDirectory, directory, fileName);
        using FileStream fileStream = File.OpenRead(fieldsPath);
        TextureAtlasFields textureAtlasFields = 
            JsonSerializer.Deserialize<TextureAtlasFields>(fileStream, JsonSerializerOptions) ??
            throw new ArgumentException($"No valid atlas at {fieldsPath}");
        string imagePath = Path.Combine(textureAtlasFields.Directory, textureAtlasFields.FileName);
        Texture2D atlasTexture = _assetManager.Load<Owner, Texture2D>(imagePath);
        TextureAtlas textureAtlas = new(atlasTexture);
        foreach ((string regionName, Rectangle source) in textureAtlasFields.RegionNameToSourceRectangle)
        {
            textureAtlas.AddRegion(regionName, source);
        }
        return textureAtlas;
    }
}
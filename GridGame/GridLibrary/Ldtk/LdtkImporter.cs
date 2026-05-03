using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Content;

namespace GridLibrary.Ldtk;

public class LdtkImporter(ContentManager contentManager)
{
    private readonly ContentManager _contentManager = contentManager;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LdtkProjectFile Import(string assetName)
    {
        string filePath = Path.Join(_contentManager.RootDirectory, assetName);
        using FileStream fileStream = File.Open(filePath, FileMode.Open);
        LdtkProjectFile ldtkProjectFile = JsonSerializer.Deserialize<LdtkProjectFile>(fileStream, _jsonSerializerOptions);
        return ldtkProjectFile;
    }
}
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace GridLibrary.Ldtk;

public struct LdtkEntityInstance
{
    [JsonPropertyName("__identifier")]
    public string Identifier { get; set; }
    // Might be worth making a custom converter to turn these obnoxious
    // arrays directly into Points
    [JsonPropertyName("__grid")]
    public int[] Grid { get; set; }

    public readonly Point Position => new(Grid[0], Grid[1]);

    // I could have the texture coordinates be part of custom data.
    // Maybe if it gets annoying managing it in the codebase.
}

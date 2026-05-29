using GridLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GridLibrary.UI;

public class TextureAtlas
{
    private readonly Texture2D _texture;
    private readonly Dictionary<string, TextureRegion> _regions = [];

    public TextureAtlas(Texture2D texture)
    {
        _texture = texture;
    }

    public TextureRegion AddRegion(string name, Rectangle source)
    {
        TextureRegion textureRegion = new()
        {
            Texture = _texture,
            SourceRectangle = source
        };
        _regions.Add(name, textureRegion);
        return textureRegion;
    }

    public TextureRegion GetRegion(string name) => _regions[name];
}

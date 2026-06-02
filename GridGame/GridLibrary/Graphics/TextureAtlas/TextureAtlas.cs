using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GridLibrary.Graphics.TextureAtlas;

public class TextureAtlas
{
    private readonly Texture2D _texture;
    private readonly Dictionary<string, TextureRegion> _regions = [];
    private readonly Dictionary<string, Animation> _animations = [];

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

    public Animation AddAnimation(string name, int delayInMilliseconds, int width, int height, Point[] sources)
    {
        List<TextureRegion> frames = [];
        foreach(Point source in sources)
        {
            Rectangle sourceRectangle = new(source.X, source.Y, width, height);
            frames.Add(new()
            {
                Texture = _texture,
                SourceRectangle = sourceRectangle
            });
        }
        Animation animation = new()
        {
            Delay = TimeSpan.FromMilliseconds(delayInMilliseconds),
            Frames = frames
        };
        _animations.Add(name, animation);
        return animation;
    }

    public Animation GetAnimation(string name) => _animations[name];
}

using GridLibrary.Graphics;
using System.Collections.Generic;

namespace GridLibrary.Entities;

public static class AnimationPool
{
    private static Dictionary<string, Animation> _animations = [];
    public static Animation Get(string key) => _animations[key];
    public static void Add(string key, Animation value) => _animations.Add(key, value);
    public static void Clear() => _animations.Clear();
}

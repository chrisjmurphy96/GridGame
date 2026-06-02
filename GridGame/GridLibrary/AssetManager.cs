using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace GridLibrary;

/// <summary>
/// This class *should* allow every other class to manage their own assets
/// without workarounds, wasteful unloading, or collisions.
/// </summary>
public class AssetManager(ContentManager contentManager)
{
    private readonly ContentManager _contentManager = contentManager;

    private readonly Dictionary<string, List<Type>> _assets = [];

    public string RootDirectory => _contentManager.RootDirectory;

    public T Load<Owner, T>(string assetName)
    {
        Type ownerType = typeof(Owner);
        T asset = _contentManager.Load<T>(assetName);
        if (_assets.TryGetValue(assetName, out List<Type>? owners) && owners is not null)
        {
            if (!owners.Contains(ownerType))
                _assets[assetName].Add(ownerType);
        }
        else
            _assets[assetName] = [ownerType];
        return asset;
    }

    public void Unload<Owner>()
    {
        Type ownerType = typeof(Owner);
        List<string> assetNames = [];
        foreach (KeyValuePair<string, List<Type>> asset in _assets)
        {
            if (asset.Value.Remove(ownerType) && asset.Value.Count is 0)
                assetNames.Add(asset.Key);
        }
        _contentManager.UnloadAssets(assetNames);
    }
}

public class Asset
{
    public required string Name { get; init; }
    public List<Type> Owners { get; } = [];
}
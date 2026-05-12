using System;
using System.Collections.Generic;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;

namespace GridGame.Core.Entities;

public static class EntityFactory
{
    /// <summary>
    /// I can't be bothered trying to make enums work a second time, so this is
    /// taking the lazy approach of just having the user handle passing in the correct identifiers
    /// </summary>
    public static Dictionary<Point, IEntity> CreateLayerEntities(LdtkLayerInstance layerInstance, Dictionary<string, TextureRegion> identifierToTexture)
    {
        Dictionary<Point, IEntity> entities = [];
        foreach (LdtkEntityInstance entityInstance in layerInstance.EntityInstances)
        {
            entities.Add(entityInstance.Position, Create(entityInstance, identifierToTexture));
        }
        return entities;
    }

    public static IEntity Create(LdtkEntityInstance entityInstance, Dictionary<string, TextureRegion> identifierToTexture)
    {
        if (!identifierToTexture.TryGetValue(entityInstance.Identifier, out TextureRegion? textureRegion))
            throw new ArgumentException($"No texture mapped for {entityInstance.Identifier}");

        if (entityInstance.Identifier == Fighter.LdtkIdentifier)
        {
            return new Fighter(textureRegion);
        }
        else if (entityInstance.Identifier == Goblin.LdtkIdentifier)
        {
            return new Goblin(textureRegion);
        }

        throw new NotImplementedException();
    }
}
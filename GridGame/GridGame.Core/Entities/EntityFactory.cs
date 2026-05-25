using System;
using System.Collections.Generic;
using System.Linq;
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

        IEntity entity;
        if (entityInstance.Identifier == Fighter.LdtkIdentifier)
        {
            entity = new Fighter(textureRegion);
        }
        else if (entityInstance.Identifier == Goblin.LdtkIdentifier)
        {
            entity = new Goblin(textureRegion);
        }
        else
            throw new NotImplementedException();

        IEnumerable<LdtkFieldInstance> healthInstances = entityInstance.FieldInstances.Where(fieldInstance => fieldInstance.Identifier == "StartingHealth");
        if (healthInstances.Count() is 1)
        {
            entity.Health.SetMaxAndCurrent(healthInstances.First().Value.GetValue<int>());
        }
        else if (healthInstances.Count() > 1)
        {
            throw new ArgumentException($"Cannot have more than one StartingHealth. {entityInstance.Identifier}, {entityInstance.Position}");
        }
        return entity;
    }
}
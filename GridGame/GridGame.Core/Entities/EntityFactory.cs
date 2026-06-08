using GridLibrary.Entities;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GridGame.Core.Entities;

public static class EntityFactory
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    /// <summary>
    /// I can't be bothered trying to make enums work a second time, so this is
    /// taking the lazy approach of just having the user handle passing in the correct identifiers
    /// </summary>
    public static Dictionary<Point, IEntity> CreateLayerEntities(
        LdtkLayerInstance layerInstance,
        Dictionary<string, EntityMapAnimations> identifierToAnimations)
    {
        Dictionary<Point, IEntity> entities = [];
        foreach (LdtkEntityInstance entityInstance in layerInstance.EntityInstances)
        {
            entities.Add(entityInstance.Position, Create(entityInstance, identifierToAnimations));
        }
        return entities;
    }

    public static IEntity Create(
        LdtkEntityInstance entityInstance,
        Dictionary<string, EntityMapAnimations> identifierToAnimations)
    {
        if (!identifierToAnimations.TryGetValue(entityInstance.Identifier, out EntityMapAnimations? animations))
            throw new ArgumentException($"No texture mapped for {entityInstance.Identifier}");

        IEntity entity;
        if (entityInstance.Identifier == Fighter.LdtkIdentifier)
        {
            entity = new Fighter(animations);
        }
        else if (entityInstance.Identifier == Goblin.LdtkIdentifier)
        {
            entity = new Goblin(animations);
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
        IEnumerable<LdtkFieldInstance> moveInstances = entityInstance.FieldInstances.Where(fieldInstance => fieldInstance.Identifier == "Moves");
        if (moveInstances.Count() is 1)
        {
            string movesJson = moveInstances.First().Value.GetValue<string>();
            IEnumerable<IMove> moves = JsonSerializer
                .Deserialize<List<Move>>(movesJson, _jsonSerializerOptions)?
                .Cast<IMove>() ??
                throw new ArgumentException("Invalid JSON");
            entity.Moves.AddRange(moves);
            // TODO: add some sort of index or flag to indicate the starting selected move
            entity.SelectedMove = entity.Moves.First();
        }
        else if (moveInstances.Count() > 1)
        {
            throw new ArgumentException($"Cannot have more than one Moves. {entityInstance.Identifier}, {entityInstance.Position}");
        }
        IEnumerable<LdtkFieldInstance> dodgeAnimationKeyInstances = entityInstance.FieldInstances.Where(fieldInstance => fieldInstance.Identifier == "DodgeAnimationKey");
        if (dodgeAnimationKeyInstances.Count() is 1)
        {
            string dodgeAnimationKey = dodgeAnimationKeyInstances.First().Value.GetValue<string>();
            entity.DodgeAnimationKey = dodgeAnimationKey;
        }
        else
        {
            throw new ArgumentException($"Need exactly one dodge animation. {entityInstance.Identifier}, {entityInstance.Position}");
        }
        return entity;
    }
}
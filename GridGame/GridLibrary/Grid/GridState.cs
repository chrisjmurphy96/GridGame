using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GridLibrary.Entities;
using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

/// <summary>
/// There's been entirely too much arbitray passing data around.
/// Having a State object and a data service to pass it around
/// could end up making my life a lot easier. Even if I start
/// using the "evil" static class patterns. It's becoming obvious
/// that in a game engine, global data has its place.
/// 
/// When saving, only include properties that can change.
/// - Entities (because they have positions and health)
/// - CursorPosition (could be skipped, but sounds nice)
/// - If I add the idea of more than one scene, need to save which one is active
/// - Any global stats like item usages remaining, currency, etc.
/// </summary>
public class GridState
{
    [JsonIgnore]
    public GridTile? ActiveTile => Tiles.InBounds(CursorPosition) ? Tiles[CursorPosition] : null;
    public IEntity? GetHoveredEntity() => Entities.GetValueOrDefault(CursorPosition);
    [JsonIgnore]
    public GridTileList Tiles { get; set; } = new(0);
    public Dictionary<Point, IEntity> Entities { get; set; } = [];
    public Point CursorPosition = Point.Zero;
    // Cursor position can't really substitute the CameraPosition.
    // Otherwise the camera would always start with the cursor in the corner.
    public Vector2 CameraPosition = Vector2.Zero;
    [JsonIgnore]
    public (Point position, IEntity entity)? ActiveEntity { get; private set; } = null;
    [JsonIgnore]
    public Point? PotentialMove = null;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IncludeFields = true
    };

    public static GridState Instance { get; private set; } = new();

    private GridState() { }

    public static void MoveActiveEntityToCursor()
    {
        (Point position, IEntity entity)? activeEntity = Instance.ActiveEntity;
        if (activeEntity is null)
            throw new ArgumentException($"No {nameof(activeEntity)}");
        Point cursorPosition = Instance.CursorPosition;
        if (Instance.Entities.TryAdd(cursorPosition, activeEntity.Value.entity))
            Instance.Entities.Remove(activeEntity.Value.position);
    }

    public static void ResetActiveEntityPosition()
    {
        (Point position, IEntity entity)? activeEntity = Instance.ActiveEntity;
        if (activeEntity is null)
            throw new ArgumentException($"No {nameof(activeEntity)}");
        if (!Instance.Entities.TryAdd(activeEntity.Value.position, activeEntity.Value.entity))
            return;
        else
        {
            Point cursorPosition = Instance.CursorPosition;
            Instance.Entities.Remove(cursorPosition);
        }
    }

    public static void SetActiveEntity()
    {
        Point position = Instance.CursorPosition;
        IEntity entity = Instance.Entities[position];
        Instance.ActiveEntity = (position, entity);
    }

    public static void UnsetActiveEntity()
    {
        Instance.ActiveEntity = null;
    }

    // Ideas for additional fields for saving state to disk:
    // - dead characters (friendly and enemy)
    // - turns remaining
    // - inventories
    // - if I added multiple levels, what's been completed/story flags have triggered
    // - active scene if it's not the grid

    /// <summary>
    /// This is really just a POC. A real save system should probably
    /// encrypt its data, so at least it's annoying to mess with?
    /// Maybe that needlessly hinders player agency though.
    /// Would also probably want a Saves directory and allow more than one save file.
    /// 
    /// We first read into a MemoryStream, just to make sure deserialization happens without errors.
    /// </summary>
    public static void Save()
    {
        using MemoryStream intermediateStream = new();
        JsonSerializer.Serialize(intermediateStream, Instance, _jsonSerializerOptions);
        intermediateStream.Position = 0;
        using FileStream fileStream = File.Open("save.json", FileMode.Create, FileAccess.ReadWrite);
        intermediateStream.CopyTo(fileStream);
    }

    public static void Load()
    {
        using FileStream fileStream = File.OpenRead("save.json");
        Instance = JsonSerializer.Deserialize<GridState>(fileStream, _jsonSerializerOptions) ?? throw new ArgumentException($"Failed to load save.json");
    }
}
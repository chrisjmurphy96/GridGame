using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GridLibrary.Graphics;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

/// <summary>
/// This could probably have stricter validation to make sure we have a valid rectangle.
/// Honestly for right now I only care about it for the utility of always being able to grab the index
/// without re-implementing the index every time.
/// </summary>
public class GridTileList : List<GridTile>
{
    /// <summary>
    /// Gets the grid tile at column and row.
    /// </summary>
    public GridTile this[int column, int row] => this[GetIndex(column, row)];

    public GridTile this[Point point] => this[point.X, point.Y];

    public bool InBounds(Point point) => point.Y >= 0 && point.Y < Rows && point.X >= 0 && point.X < Columns;

    public int Columns { get; }
    public int Rows => Count > 0 ? Count / Columns : 0;

    public GridTileList(int columns) : base()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        Columns = columns;
    }

    private int GetIndex(int column, int row) => (row * Columns) + column;

    public static GridTileList FromLevel(
        LdtkProjectFile projectFile,
        string levelName,
        Texture2D levelAtlas,
        Dictionary<string, TileInfo> enumNameToTileInfo)
    {
        LdtkLevel level = projectFile.GetLevelByName(levelName);
        LdtkLayerInstance layerInstance = level.GetTileLayer();
        int tilesetUid = layerInstance.TilesetDefUid ?? throw new ArgumentException($"No {layerInstance.TilesetDefUid} found");
        LdtkTileset tileset = projectFile.Defs.Tilesets.Single(tileset => tileset.Uid == tilesetUid);
        int tileSize = tileset.TileGridSize;
        Dictionary<int, TileInfo> tileIdToTileInfo = [];
        foreach (LdtkEnumTag enumTag in tileset.EnumTags)
        {
            // Explicit enums should have tags. Let it throw if there isn't, means I messed up.
            TileInfo tileInfo = enumNameToTileInfo[enumTag.EnumValueId];
            foreach(int tileId in enumTag.TileIds)
            {
                // Explicitly call add so we throw on collisions.
                // LDTK seems to support multiple enums on a tile, but I'm
                // not sure I want to.
                tileIdToTileInfo.Add(tileId, tileInfo);
            }
        }

        Dictionary<int, FrameData> tileIdToFrameData = [];
        foreach (LdtkCustomData customData in tileset.CustomData)
        {
            FrameData frameData = JsonSerializer.Deserialize<FrameData>(customData.Data, LdtkImporter.JsonSerializerOptions);
            tileIdToFrameData.Add(customData.TileId, frameData);
        }

        LdtkGridTile[] ldtkGridTiles = layerInstance.GridTiles;
        GridTileList tiles = new(level.GetTileLayer().Columns);
        foreach(LdtkGridTile ldtkGridTile in ldtkGridTiles)
        {
            if (tileIdToFrameData.TryGetValue(ldtkGridTile.TileId, out FrameData frameData))
            {
                tiles.Add(CreateAnimatedGridTile(ldtkGridTile, levelAtlas, layerInstance, tileIdToTileInfo, frameData));
            }
            else
            {
                tiles.Add(CreateGridTile(ldtkGridTile, levelAtlas, layerInstance, tileIdToTileInfo));
            }
        }

        return tiles;
    }

    private static AnimatedGridTile CreateAnimatedGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D levelAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        Dictionary<int, TileInfo> tileIdToTileInfo,
        FrameData frameData)
    {
        int gridSize = ldtkLayerInstance.GridSize;
        Animation animation = Animation.FromFrameData(levelAtlas, frameData, gridSize, gridSize);
        TileInfo tileInfo = tileIdToTileInfo.GetValueOrDefault(ldtkGridTile.TileId) ?? new();
        return new AnimatedGridTile(ldtkGridTile.Position, animation, tileInfo);
    }

    private static GridTile CreateGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D levelAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        Dictionary<int, TileInfo> tileIdToTileInfo)
    {
        TextureRegion texture = new()
        {
            Texture = levelAtlas,
            SourceRectangle = new Rectangle
            {
                X = ldtkGridTile.TextureOriginX,
                Y = ldtkGridTile.TextureOriginY,
                Width = ldtkLayerInstance.GridSize,
                Height = ldtkLayerInstance.GridSize
            }
        };
        TileInfo tileInfo = tileIdToTileInfo.GetValueOrDefault(ldtkGridTile.TileId) ?? new();
        return new GridTile(ldtkGridTile.Position, texture, tileInfo);
    }
}
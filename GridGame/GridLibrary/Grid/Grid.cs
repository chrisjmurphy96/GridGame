using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GridLibrary.Graphics;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

public abstract class BaseGrid
{
    public abstract BaseGridTile[] Tiles { get; }
}

public class Grid<T> : BaseGrid where T : struct, Enum
{
    public override GridTile<T>[] Tiles { get; }
    public LdtkLevel Map { get; }
    public Texture2D MapAtlas { get; }
    public TextureRegion GridOverlay { get; }
    public int Scalar { get; }
    public Cursor Cursor { get; }
    public MovementArrow<T> MovementArrow { get; }

    public int Columns => Map.GetDefaultLayer().Columns;
    public int Rows => Map.GetDefaultLayer().Rows;

    private Point _cursorPosition = Point.Zero;
    public GridTile<T> ActiveTile => this[_cursorPosition];
    
    public Grid(
        LdtkProjectFile projectFile,
        string levelName,
        Texture2D mapAtlas,
        TextureRegion gridOverlayTexture,
        int scalar,
        Cursor cursor,
        MovementArrow<T> movementArrow)
    {
        Map = projectFile.GetLevelByName(levelName);
        MapAtlas = mapAtlas;
        GridOverlay = gridOverlayTexture;
        Scalar = scalar;
        Cursor = cursor;
        MovementArrow = movementArrow;

        LdtkLayerInstance layerInstance = Map.GetDefaultLayer();
        int tilesetUid = layerInstance.TilesetDefUid;
        LdtkTileset tileset = projectFile.Defs.Tilesets.Single(tileset => tileset.Uid == tilesetUid);
        
        List<(T tileType, int[] tileIds)> enumTags = [];
        foreach (LdtkEnumTag enumTag in tileset.EnumTags)
        {
            T enumValue = Enum.Parse<T>(enumTag.EnumValueId);
            enumTags.Add((enumValue, enumTag.TileIds));
        }

        Dictionary<int, FrameData> tileIdToFrameData = [];
        foreach (LdtkCustomData customData in tileset.CustomData)
        {
            FrameData frameData = JsonSerializer.Deserialize<FrameData>(customData.Data, LdtkImporter.JsonSerializerOptions);
            tileIdToFrameData.Add(customData.TileId, frameData);
        }

        LdtkGridTile[] ldtkGridTiles = layerInstance.GridTiles;
        Tiles = new GridTile<T>[ldtkGridTiles.Length];
        for (int i = 0; i < ldtkGridTiles.Length; i++)
        {
            if (tileIdToFrameData.TryGetValue(ldtkGridTiles[i].TileId, out FrameData frameData))
            {
                Tiles[i] = CreateAnimatedGridTile(ldtkGridTiles[i], mapAtlas, layerInstance, enumTags, frameData);
            }
            else
            {
                Tiles[i] = CreateGridTile(ldtkGridTiles[i], mapAtlas, layerInstance, enumTags);
            }
        }
    }

    private static AnimatedGridTile<T> CreateAnimatedGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D mapAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        List<(T tileType, int[] tileIds)> enumTags,
        FrameData frameData)
    {
        T tileType = enumTags.Single(e => e.tileIds.Contains(ldtkGridTile.TileId)).tileType;
        int gridSize = ldtkLayerInstance.GridSize;
        Animation animation = Animation.FromFrameData(mapAtlas, frameData, gridSize, gridSize);
        return new AnimatedGridTile<T>(ldtkGridTile.Position, tileType, animation);
    }

    private static GridTile<T> CreateGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D mapAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        List<(T tileType, int[] tileIds)> enumTags)
    {
        T tileType = enumTags.Single(e => e.tileIds.Contains(ldtkGridTile.TileId)).tileType;
        TextureRegion texture = new()
        {
            Texture = mapAtlas,
            SourceRectangle = new Rectangle
            {
                X = ldtkGridTile.TextureOriginX,
                Y = ldtkGridTile.TextureOriginY,
                Width = ldtkLayerInstance.GridSize,
                Height = ldtkLayerInstance.GridSize
            }
        };
        return new GridTile<T>(ldtkGridTile.Position, texture, tileType);
    }

    /// <summary>
    /// Gets the grid tile
    /// </summary>
    /// <param name="index">The index of the grid tile</param>
    public GridTile<T> this[int index] => this.Tiles[index];

    /// <summary>
    /// Gets the grid tile at column and row.
    /// </summary>
    public GridTile<T> this[int column, int row] => this.Tiles[GetIndex(column, row)];

    public GridTile<T> this[Point point] => this.Tiles[GetIndex(point.X, point.Y)];

    private int GetIndex(int column, int row) => (row * Columns) + column;

    public void Update(GameTime gameTime)
    {
        Cursor.Update(gameTime);
        // MovementArrow.Update(_cursorPosition);
        foreach (GridTile<T> gridTile in Tiles)
            gridTile.Update(gameTime);
    }

    public void MoveCursorUp(Camera camera)
    {
        if (_cursorPosition.Y > 0)
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X,
                Y = _cursorPosition.Y - 1
            };
            Cursor.MoveUp(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorDown(Camera camera)
    {
        if (_cursorPosition.Y < (Rows - 1))
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X,
                Y = _cursorPosition.Y + 1
            };
            Cursor.MoveDown(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorRight(Camera camera)
    {
        if (_cursorPosition.X < (Columns - 1))
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X + 1,
                Y = _cursorPosition.Y
            };
            Cursor.MoveRight(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorLeft(Camera camera)
    {
        if (_cursorPosition.X > 0)
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X - 1,
                Y = _cursorPosition.Y
            };
            Cursor.MoveLeft(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void StartPath()
    {
        // I might just need to bite the bullet and add the generic typing.
        // I can't figure out a way to ditch it and still keep the constructor logic.
        MovementArrow.Start(Columns, Rows, 10, _cursorPosition, Tiles);//.Select(tile => (BaseGridTile)tile));
    }

    public void CancelPath()
    {
        MovementArrow.Cancel();
    }
}
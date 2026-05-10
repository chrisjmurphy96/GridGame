using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GridLibrary.Graphics;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

public class Grid<tileType> where tileType : struct, Enum
{
    public GridTileList<tileType> Tiles { get; }
    public LdtkLevel Map { get; }
    public Texture2D MapAtlas { get; }
    public TextureRegion GridOverlay { get; }
    public int Scalar { get; }
    public int TileSize { get; set; }
    public Cursor Cursor { get; }
    public MovementArrow<tileType> MovementArrow { get; }

    public int Columns => Map.GetDefaultLayer().Columns;
    public int Rows => Map.GetDefaultLayer().Rows;

    private Point _cursorPosition = Point.Zero;
    private readonly Func<Point, TextureRegion, tileType, GridTile<tileType>> _gridTileFactory;
    private readonly Func<Point, Animation, tileType, AnimatedGridTile<tileType>> _animatedGridTileFactory;

    public GridTile<tileType> ActiveTile => Tiles[_cursorPosition];
    
    public Grid(
        LdtkProjectFile projectFile,
        string levelName,
        Texture2D mapAtlas,
        TextureRegion gridOverlayTexture,
        int scalar,
        Cursor cursor,
        MovementArrow<tileType> movementArrow,
        Func<Point, TextureRegion, tileType, GridTile<tileType>> gridTileFactory,
        Func<Point, Animation, tileType, AnimatedGridTile<tileType>> animatedGridTileFactory)
    {
        Map = projectFile.GetLevelByName(levelName);
        MapAtlas = mapAtlas;
        GridOverlay = gridOverlayTexture;
        Scalar = scalar;
        Cursor = cursor;
        MovementArrow = movementArrow;
        _gridTileFactory = gridTileFactory;
        _animatedGridTileFactory = animatedGridTileFactory;
        LdtkLayerInstance layerInstance = Map.GetDefaultLayer();
        int tilesetUid = layerInstance.TilesetDefUid;
        LdtkTileset tileset = projectFile.Defs.Tilesets.Single(tileset => tileset.Uid == tilesetUid);
        TileSize = tileset.TileGridSize;
        
        List<(tileType tileType, int[] tileIds)> enumTags = [];
        foreach (LdtkEnumTag enumTag in tileset.EnumTags)
        {
            tileType enumValue = Enum.Parse<tileType>(enumTag.EnumValueId);
            enumTags.Add((enumValue, enumTag.TileIds));
        }

        Dictionary<int, FrameData> tileIdToFrameData = [];
        foreach (LdtkCustomData customData in tileset.CustomData)
        {
            FrameData frameData = JsonSerializer.Deserialize<FrameData>(customData.Data, LdtkImporter.JsonSerializerOptions);
            tileIdToFrameData.Add(customData.TileId, frameData);
        }

        LdtkGridTile[] ldtkGridTiles = layerInstance.GridTiles;
        Tiles = new GridTileList<tileType>(Columns);
        foreach(LdtkGridTile ldtkGridTile in ldtkGridTiles)
        {
            if (tileIdToFrameData.TryGetValue(ldtkGridTile.TileId, out FrameData frameData))
            {
                Tiles.Add(CreateAnimatedGridTile(ldtkGridTile, mapAtlas, layerInstance, enumTags, frameData));
            }
            else
            {
                Tiles.Add(CreateGridTile(ldtkGridTile, mapAtlas, layerInstance, enumTags));
            }
        }
    }

    private AnimatedGridTile<tileType> CreateAnimatedGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D mapAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        List<(tileType tileType, int[] tileIds)> enumTags,
        FrameData frameData)
    {
        tileType tileType = enumTags.Single(e => e.tileIds.Contains(ldtkGridTile.TileId)).tileType;
        int gridSize = ldtkLayerInstance.GridSize;
        Animation animation = Animation.FromFrameData(mapAtlas, frameData, gridSize, gridSize);
        return _animatedGridTileFactory(ldtkGridTile.Position, animation, tileType);
    }

    private GridTile<tileType> CreateGridTile(
        LdtkGridTile ldtkGridTile,
        Texture2D mapAtlas,
        LdtkLayerInstance ldtkLayerInstance,
        List<(tileType tileType, int[] tileIds)> enumTags)
    {
        tileType tileType = enumTags.Single(e => e.tileIds.Contains(ldtkGridTile.TileId)).tileType;
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
        return _gridTileFactory(ldtkGridTile.Position, texture, tileType);
    }

    public void Update(GameTime gameTime)
    {
        Cursor.Update(gameTime);
        foreach (GridTile<tileType> gridTile in Tiles)
            gridTile.Update(gameTime);
    }

    public void MoveCursorUp(Camera camera)
    {
        if (_cursorPosition.Y > 0)
        {
            _cursorPosition = _cursorPosition.Up();
            Cursor.MoveUp(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorDown(Camera camera)
    {
        if (_cursorPosition.Y < (Rows - 1))
        {
            _cursorPosition = _cursorPosition.Down();
            Cursor.MoveDown(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorRight(Camera camera)
    {
        if (_cursorPosition.X < (Columns - 1))
        {
            _cursorPosition = _cursorPosition.Right();
            Cursor.MoveRight(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void MoveCursorLeft(Camera camera)
    {
        if (_cursorPosition.X > 0)
        {
            _cursorPosition = _cursorPosition.Left();
            Cursor.MoveLeft(camera);
            MovementArrow.Update(_cursorPosition);
        }
    }

    public void StartPath()
    {
        // I might just need to bite the bullet and add the generic typing.
        // I can't figure out a way to ditch it and still keep the constructor logic.
        MovementArrow.Start(Columns, Rows, 20, _cursorPosition, Tiles);
    }

    public void CancelPath()
    {
        MovementArrow.Cancel();
    }
}
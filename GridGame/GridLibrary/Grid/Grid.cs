using System;
using GridLibrary.Graphics;
using GridLibrary.Ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Grid;

public class Grid
{
    public GridTile[] Tiles { get; }
    public LdtkLevel Map { get; }
    public Texture2D MapAtlas { get; }
    public TextureRegion GridOverlay { get; }
    public int Scalar { get; }
    public Cursor Cursor { get; }
    public int Columns => Map.GetDefaultLayer().Columns;
    public int Rows => Map.GetDefaultLayer().Rows;

    private Point _cursorPosition = Point.Zero;
    public GridTile ActiveTile => this[_cursorPosition];
    
    public Grid(
        LdtkLevel map,
        Texture2D mapAtlas,
        TextureRegion gridOverlayTexture,
        int scalar,
        AnimatedSprite cursorSprite)
    {
        Map = map;
        MapAtlas = mapAtlas;
        GridOverlay = gridOverlayTexture;
        Scalar = scalar;
        Cursor = new Cursor
        {
            CursorSprite = cursorSprite
        };

        LdtkLayerInstance ldtkLayerInstance = map.GetDefaultLayer();
        LdtkGridTile[] ldtkGridTiles = ldtkLayerInstance.GridTiles;
        Tiles = new GridTile[ldtkGridTiles.Length];
        for (int i = 0; i < ldtkGridTiles.Length; i++)
        {
            Tiles[i] = CreateGridTile(ldtkGridTiles[i], mapAtlas, ldtkLayerInstance);
        }
    }

    private static GridTile CreateGridTile(LdtkGridTile ldtkGridTile, Texture2D mapAtlas, LdtkLayerInstance ldtkLayerInstance)
    {
        return new GridTile
        {
            TileType = ldtkGridTile.TileType,
            Position = ldtkGridTile.Position,
            Texture = new TextureRegion
            {
                Texture = mapAtlas,
                SourceRectangle = new Rectangle
                {
                    X = ldtkGridTile.TextureOriginX,
                    Y = ldtkGridTile.TextureOriginY,
                    Width = ldtkLayerInstance.GridSize,
                    Height = ldtkLayerInstance.GridSize
                }
            }
        };
    }

    /// <summary>
    /// Gets the grid tile
    /// </summary>
    /// <param name="index">The index of the grid tile</param>
    public GridTile this[int index] => this.Tiles[index];

    /// <summary>
    /// Gets the grid tile at column and row.
    /// </summary>
    public GridTile this[int column, int row] => this.Tiles[GetIndex(column, row)];

    public GridTile this[Point point] => this.Tiles[GetIndex(point.X, point.Y)];

    private int GetIndex(int column, int row) => (row * Columns) + column;

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
        }
    }

    public void MoveCursorDown(Camera camera)
    {
        if (_cursorPosition.Y < Rows)
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X,
                Y = _cursorPosition.Y + 1
            };
            Cursor.MoveDown(camera);
        }
    }

    public void MoveCursorRight(Camera camera)
    {
        if (_cursorPosition.X < Columns)
        {
            _cursorPosition = new Point
            {
                X = _cursorPosition.X + 1,
                Y = _cursorPosition.Y
            };
            Cursor.MoveRight(camera);
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
        }
    }

    private void MoveCursor()
    {
        throw new NotImplementedException();
    }
}
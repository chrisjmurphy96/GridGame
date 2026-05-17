using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using GridLibrary.Entities;
using GridLibrary.Graphics;
using GridLibrary.Input;
using GridLibrary.Ldtk;
using GridLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridLibrary.Grid;

/// <summary>
/// I am heavily considering making the grid a UI element.
/// Only problem is, a lot of the default UI element stuff
/// doesn't really make sense. The only real benefit I'd get
/// is some code boiler plate reduction and proper IsFocused logic.
/// </summary>
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
    public MoveOverlay<tileType> MoveOverlay { get; }
    public TextureRegion EnemyMoveOverlayTexture { get; }
    public ContextMenu ContextMenu { get; }
    public Dictionary<Point, IEntity> Entities { get; }

    private readonly UIRoot _uiRoot;

    public int Columns => Map.GetTileLayer().Columns;
    public int Rows => Map.GetTileLayer().Rows;

    private Point _cursorPosition = Point.Zero;
    private (Point position, IEntity entity) _activeEntity;
    private bool ChoosingWhereToMove => MoveOverlay.IsVisible;
    private readonly Func<Point, TextureRegion, tileType, GridTile<tileType>> _gridTileFactory;
    private readonly Func<Point, Animation, tileType, AnimatedGridTile<tileType>> _animatedGridTileFactory;
    private readonly static TimeSpan MOVE_DELAY = TimeSpan.FromMilliseconds(100);
    public List<MoveOverlay<tileType>> EnemyMoveOverlays { get; } = [];
    public HashSet<Point> EnemyAttackPoints { get; } = [];

    public GridTile<tileType> ActiveTile => Tiles[_cursorPosition];
    
    public Grid(
        LdtkProjectFile projectFile,
        string levelName,
        Texture2D mapAtlas,
        TextureRegion gridOverlayTexture,
        int scalar,
        Cursor cursor,
        MovementArrow<tileType> movementArrow,
        MoveOverlay<tileType> moveOverlay,
        TextureRegion enemyMoveOverlayTexture,
        ContextMenu contextMenu,
        Dictionary<Point, IEntity> entities,
        UIRoot uiRoot,
        Func<Point, TextureRegion, tileType, GridTile<tileType>> gridTileFactory,
        Func<Point, Animation, tileType, AnimatedGridTile<tileType>> animatedGridTileFactory)
    {
        Map = projectFile.GetLevelByName(levelName);
        MapAtlas = mapAtlas;
        GridOverlay = gridOverlayTexture;
        Scalar = scalar;
        Cursor = cursor;
        MovementArrow = movementArrow;
        MoveOverlay = moveOverlay;
        EnemyMoveOverlayTexture = enemyMoveOverlayTexture;
        ContextMenu = contextMenu;
        Entities = entities;
        _uiRoot = uiRoot;
        _gridTileFactory = gridTileFactory;
        _animatedGridTileFactory = animatedGridTileFactory;
        LdtkLayerInstance layerInstance = Map.GetTileLayer();
        int tilesetUid = layerInstance.TilesetDefUid ?? throw new ArgumentException($"No {layerInstance.TilesetDefUid} found");
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

    public void Update(GameTime gameTime, KeyboardInfo keyboardInfo, Camera camera)
    {
        Cursor.Update(gameTime);
        foreach (GridTile<tileType> gridTile in Tiles)
            gridTile.Update(gameTime);
        UpdateEnemyOverlay();

        // ContextMenu.Update(Entities, _activeEntity.position, Tiles);
        if (ContextMenu.IsFocused || ContextMenu.MovePreview.IsFocused)
        {
            // if (keyboardInfo.WasKeyJustPressed(Keys.X))
            //     ContextMenu.Close();
            // else // I'm pretty sure this isn't what I want, but let's just see the menu for now
            return;
        }

        if (keyboardInfo.WasKeyJustPressed(Keys.Down) ||
            keyboardInfo.IsKeyHeldDown(Keys.Down, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Down);
            MoveCursorDown(camera);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Up) ||
            keyboardInfo.IsKeyHeldDown(Keys.Up, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Up);
            MoveCursorUp(camera);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Right) ||
            keyboardInfo.IsKeyHeldDown(Keys.Right, MOVE_DELAY))
        {   
            keyboardInfo.ResetKeyHold(Keys.Right);
            MoveCursorRight(camera);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Left) ||
            keyboardInfo.IsKeyHeldDown(Keys.Left, MOVE_DELAY))
        {
            keyboardInfo.ResetKeyHold(Keys.Left);
            MoveCursorLeft(camera);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.Z))
        {
            CursorClick(gameTime);
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.X))
        {
            CancelCursorClick();
        }
        if (keyboardInfo.WasKeyJustPressed(Keys.O))
        {
            ToggleEnemyOverlay();
        }
    }

    public bool ShowEnemyOverlay { get; private set; } = false;

    private void ToggleEnemyOverlay()
    {
        ShowEnemyOverlay = !ShowEnemyOverlay;
    }

    /// <summary>
    /// This could probably be its own class, but for now just letting it live here.
    /// The available attack points can change as friendly units move, so this needs to
    /// be recalculated each update cycle.
    /// </summary>
    private void UpdateEnemyOverlay()
    {
        EnemyAttackPoints.Clear();
        if (!ShowEnemyOverlay)
        {
            return;
        }

        foreach((Point entityPosition, IEntity entity) in Entities)
        {
            if (entity.IsFriendly)
                continue;

            // TODO: Could add clicking a singular enemy entity shows just their range.
            HashSet<Point> walkable = Dijkstra.GetWalkable<tileType>(entityPosition, entity.MovementRange, Tiles, Entities, forEnemy: true);
            HashSet<Point> attackable = Dijkstra.GetAttackable<tileType>(entity.DefaultAttack.Range, walkable, Tiles);
            EnemyAttackPoints.UnionWith(walkable);
            EnemyAttackPoints.UnionWith(attackable);
        }
    }

    public void MoveCursorUp(Camera camera)
    {
        if (_cursorPosition.Y > 0 && !ContextMenu.IsFocused)
        {
            _cursorPosition = _cursorPosition.Up();
            Cursor.MoveUp(camera);
            MovementArrow.Update(_cursorPosition, MoveOverlay.MovementPoints);
        }
    }

    public void MoveCursorDown(Camera camera)
    {
        if (_cursorPosition.Y < (Rows - 1) && !ContextMenu.IsFocused)
        {
            _cursorPosition = _cursorPosition.Down();
            Cursor.MoveDown(camera);
            MovementArrow.Update(_cursorPosition, MoveOverlay.MovementPoints);
        }
    }

    public void MoveCursorRight(Camera camera)
    {
        if (_cursorPosition.X < (Columns - 1) && !ContextMenu.IsFocused)
        {
            _cursorPosition = _cursorPosition.Right();
            Cursor.MoveRight(camera);
            MovementArrow.Update(_cursorPosition, MoveOverlay.MovementPoints);
        }
    }

    public void MoveCursorLeft(Camera camera)
    {
        if (_cursorPosition.X > 0 && !ContextMenu.IsFocused)
        {
            _cursorPosition = _cursorPosition.Left();
            Cursor.MoveLeft(camera);
            MovementArrow.Update(_cursorPosition, MoveOverlay.MovementPoints);
        }
    }

    private void HandleMoveSelection()
    {
        IMove move = ContextMenu.Moves[ContextMenu.FocusIndex];
        Point endPosition = MovementArrow.Path.Last();
        move.PerformMove(Entities, endPosition, _cursorPosition);
        Entities.Remove(_activeEntity.position);
        Entities.Add(endPosition, _activeEntity.entity);
        ContextMenu.Close();
        CancelCursorClick();
    }

    /// <summary>
    /// - If context menu is visible, do nothing. They need to cancel it or choose a move.
    /// - If the player has chosen where to move a character, show the context menu.
    /// - If the player has selected a controllable character, start the movement action.
    /// </summary>
    public void CursorClick(GameTime gameTime)
    {
        if (ContextMenu.IsVisible)
            return;

        Entities.TryGetValue(_cursorPosition, out IEntity? entity);
        if (ChoosingWhereToMove)
        {
            if (!MoveOverlay.MovementPoints.Contains(_cursorPosition))
                return;
            if (!ContextMenu.IsVisible)
            {
                ContextMenu.Open(
                    _activeEntity.entity,
                    _cursorPosition,
                    gameTime,
                    HandleMoveSelection,
                    Tiles);
                UIRoot.Focus(ContextMenu);
            }
            return;
        }

        if (entity is null ||
            !entity.IsPlayerControllable)
        {
            return;
        }

        _activeEntity = (_cursorPosition, entity);
        int movementRange = entity.MovementRange;
        int attackRange = entity.DefaultAttack.Range;
        MoveOverlay.Show(movementRange, attackRange, _cursorPosition, Tiles, Entities);
        MovementArrow.Start(movementRange, _cursorPosition, Tiles);
    }

    public void CancelCursorClick()
    {
        MoveOverlay.Hide();
        MovementArrow.Cancel();
    }
}
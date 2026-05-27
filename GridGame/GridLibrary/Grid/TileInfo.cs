using Microsoft.Xna.Framework;

namespace GridLibrary.Grid;

public class TileInfo
{
    public int DodgeModifier { get; init; } = 0;
    public int ArmorModifier { get; init; } = 0;
    public bool CanWalk { get; init; } = true;
    public string TileType { get; init; } = string.Empty;

    public TileInfo(string tileType)
    {
        TileType = tileType;
    }

    public override string ToString()
    {
        return 
$@"
Dodge Modifier: {DodgeModifier}
Armor Modifier: {ArmorModifier}
Can walk:       {CanWalk}
";
    }

    public string ToString(Vector2 cursorPosition)
    {
        return 
$@"
Dodge Modifier:  {DodgeModifier}
Armor Modifier:  {ArmorModifier}
Can walk:        {CanWalk}
Cursor position: x: {cursorPosition.X / 64} y: {cursorPosition / 64}
";
    }
}

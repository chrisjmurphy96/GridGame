namespace GridLibrary.Grid;

public class TileInfo
{
    public int DodgeModifier { get; init; } = 0;
    public int ArmorModifier { get; init; } = 0;
    public bool CanWalk { get; init; } = true;

    public override string ToString()
    {
        return 
$@"
Dodge Modifier: {DodgeModifier}
Armor Modifier: {ArmorModifier}
Can walk:       {CanWalk}
";
    }
}

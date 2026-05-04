namespace GridGame.Core.Tiles;

public class TileInfo
{
    public int DodgeModifier { get; set; } = 0;
    public int ArmorModifier { get; set; } = 0;
    public bool CanWalk { get; set; } = true;

    // public TileInfo() { }

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

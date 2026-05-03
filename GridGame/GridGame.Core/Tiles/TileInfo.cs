namespace GridGame.Core.Tiles;

public struct TileInfo
{
    public int DodgeChanceModifier = 0;
    public int ArmorModifier = 0;
    public bool CanWalk = true;

    public TileInfo() { }

    public override string ToString()
    {
        return 
$@"
Dodge Modifier: {DodgeChanceModifier}
Armor Modifier: {ArmorModifier}
Can walk:       {CanWalk}
";
    }
}

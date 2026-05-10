namespace GridLibrary.Ldtk;

public struct LdtkTileset
{
    public int Uid { get; set; }
    public int TileGridSize { get; set; }
    public LdtkEnumTag[] EnumTags { get; set; }
    public LdtkCustomData[] CustomData { get; set; }
}

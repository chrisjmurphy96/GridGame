namespace GridLibrary.Ldtk;

public struct LdtkEnumTag
{
    /// <summary>
    /// The name of the enum value, e.g. "Tree"
    /// </summary>
    public string EnumValueId { get; set; }

    /// <summary>
    /// Tiles associated with a particular enum value
    /// </summary>
    public int[] TileIds { get; set; }
}
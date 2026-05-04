using System.Linq;

namespace GridLibrary.Ldtk;

public struct LdtkDefs
{
    public LdtkTileset[] Tilesets { get; set; }
    public LdtkEnums[] Enums { get; set; }

    public readonly LdtkTileset DefaultTileset => Tilesets.First();
}

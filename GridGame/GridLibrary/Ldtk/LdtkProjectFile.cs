using System;
using System.Linq;

namespace GridLibrary.Ldtk;

public struct LdtkProjectFile
{
    public Guid IID { get; set; }
    public LdtkDefs Defs { get; set; }
    public LdtkLevel[] Levels { get; set; }

    public readonly LdtkLevel GetLevelByName(string name)
    {
        return Levels.Single(l => l.Identifier == name);
    }
}

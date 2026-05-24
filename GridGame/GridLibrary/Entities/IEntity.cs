using System.Collections.Generic;
using GridLibrary.Graphics;

namespace GridLibrary.Entities;

public interface IEntity
{
    static string LdtkIdentifier { get; } = string.Empty;
    public string DisplayName { get; }
    public TextureRegion Texture { get; }
    public EntityHealth Health { get; }
    public int MovementRange { get; }
    public bool IsFriendly { get; }
    public bool IsPlayerControllable { get; }
    public IMove SelectedMove { get; set; }
    public List<IMove> Moves { get; }
}
namespace GridLibrary.Entities;

public interface IEntity
{
    static string LdtkIdentifier { get; } = string.Empty;
    StandardEntityProperties Properties { get; }
}
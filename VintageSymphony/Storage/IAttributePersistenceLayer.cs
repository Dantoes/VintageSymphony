namespace VintageSymphony.Storage;

public interface IAttributePersistenceLayer : IDisposable
{
    Attribute<T> LoadAttribute<T>(string key, T defaultValue);
    void SaveAttribute<T>(Attribute<T> attribute);
}
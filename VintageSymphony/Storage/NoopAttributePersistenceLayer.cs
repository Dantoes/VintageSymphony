namespace VintageSymphony.Storage;

public class NoopAttributePersistenceLayer : IAttributePersistenceLayer
{
    public Attribute<T> LoadAttribute<T>(string key, T defaultValue)
    {
        return new Attribute<T>(key, defaultValue);
    }

    public void SaveAttribute<T>(Attribute<T> attribute)
    {
    }

    public void Dispose()
    {
    }
}
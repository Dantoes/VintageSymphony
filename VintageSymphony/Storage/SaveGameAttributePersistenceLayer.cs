using Vintagestory.API.Server;

namespace VintageSymphony.Storage;

public class SaveGameAttributePersistenceLayer : IAttributePersistenceLayer
{
    private readonly ISaveGame saveGame;

    public SaveGameAttributePersistenceLayer(ISaveGame saveGame)
    {
        this.saveGame = saveGame;
    }

    public void Dispose()
    {
    }

    public  Attribute<T> LoadAttribute<T>(string key, T defaultValue)
    {
        var value = saveGame.GetData(key, defaultValue)!;
        return new Attribute<T>(key, value);
    }

    public void SaveAttribute<T>(Attribute<T> attribute)
    {
        saveGame.StoreData(attribute.Key, attribute.Value);    
    }
}
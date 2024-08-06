using Vintagestory.API.MathTools;

namespace VintageSymphony.Storage;

public class AttributeStorage : IDisposable
{
    private const string PlayerHomesKey = "playerhomes";

    private readonly IAttributePersistenceLayer persistenceLayer;
    
    private readonly Attribute<List<Vec3i>> playerHomeLocations;
    public List<Vec3i> PlayerHomeLocations
    {
        get => playerHomeLocations.Value;
        set => playerHomeLocations.Value = value;
    }

    
    public AttributeStorage(string keyPrefix, IAttributePersistenceLayer persistenceLayer)
    {
        this.persistenceLayer = persistenceLayer;

        playerHomeLocations = persistenceLayer.LoadAttribute(keyPrefix + PlayerHomesKey, new List<Vec3i>());

    }

    public void Dispose()
    {
        persistenceLayer.SaveAttribute(playerHomeLocations);
        persistenceLayer.Dispose();
    }

}
using Newtonsoft.Json;

namespace VintageSymphony.Storage;

# nullable disable
public class LocalAttributePersistenceLayer : IAttributePersistenceLayer
{
    private readonly string filePath;
    private readonly Dictionary<string, object> attributes;

    public LocalAttributePersistenceLayer(string filePath)
    {
        this.filePath = filePath;
        attributes = new Dictionary<string, object>();

        if (File.Exists(this.filePath))
        {
            var json = File.ReadAllText(this.filePath);
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (jsonData != null)
            {
                foreach (var kvp in jsonData)
                {
                    attributes[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    public Attribute<T> LoadAttribute<T>(string key, T defaultValue)
    {
        if (attributes.TryGetValue(key, out var value))
        {
            var serializedData = value.ToString() ?? "";
            return new Attribute<T>(key, JsonConvert.DeserializeObject<T>(serializedData));
        }
        return new Attribute<T>(key, defaultValue);
    }

    public void SaveAttribute<T>(Attribute<T> attribute)
    {
        attributes[attribute.Key] = attribute.Value;
    }

    public void Dispose()
    {
        var json = JsonConvert.SerializeObject(attributes, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}

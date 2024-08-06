namespace VintageSymphony.Storage;

public class Attribute<T> 
{
    public string Key { get; set; }
    public T Value { get; set; }

    public Attribute(string key, T value)
    {
        Key = key;
        Value = value;
    }
}
using System.Runtime.Serialization;

namespace VintageSymphony.Config;

public class ConfigurationException : Exception
{
    public ConfigurationException()
    {
    }

    protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ConfigurationException(string? message) : base(message)
    {
    }

    public ConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
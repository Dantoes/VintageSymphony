namespace VintageSymphony;

public class ModException : Exception
{
    public ModException()
    {
    }

    public ModException(string? message) : base(message)
    {
    }

    public ModException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
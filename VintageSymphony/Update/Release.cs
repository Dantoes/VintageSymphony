namespace VintageSymphony.Update;

public class Release
{
    public Version Version { get; }
    public string DownloadUrl { get; }
    public string FileName { get; }

    public Release(Version version, string downloadUrl, string fileName)
    {
        Version = version;
        DownloadUrl = downloadUrl;
        FileName = fileName;
    }
}
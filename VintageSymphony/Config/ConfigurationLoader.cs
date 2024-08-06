using Vintagestory.API.Client;

namespace VintageSymphony.Config;

public class ConfigurationLoader
{
    private readonly ICoreClientAPI clientApi;
    private readonly Configuration defaultConfiguration = new()
    {
        GlobalVolume = 0.5f
    };
    private readonly string fileName;

    public ConfigurationLoader(ICoreClientAPI clientApi, string modId)
    {
        this.clientApi = clientApi;
        fileName = $"{modId}.json";
    }

    public Configuration LoadConfiguration()
    {
        try
        {
            var config = clientApi.LoadModConfig<Configuration>(fileName);
            if (config == null)
            {
                CreateDefaultConfigFile();
                return defaultConfiguration;
            }

            return config;
        }
        catch (Exception e)
        {
            throw new ConfigurationException(e.Message, e);
        }
    }

    public void SaveConfiguration(Configuration configuration)
    {
        clientApi.StoreModConfig(configuration, fileName);
    }

    private void CreateDefaultConfigFile()
    {
        SaveConfiguration(defaultConfiguration);
    }
    

}
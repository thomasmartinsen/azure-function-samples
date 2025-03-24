using Microsoft.Extensions.Configuration;

namespace common;

public static class ConfigurationUtilities
{
    public static IConfigurationRoot GetConfigurationRoot<T>() where T : class
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<T>()
            .Build();
    }
}

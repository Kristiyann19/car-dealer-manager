
using Microsoft.Extensions.Configuration;

namespace CarDealerManager.Common.AppSettings
{
    public static class AppSettingsProvider
    {
        public static string MainDbConnectionString { get; private set; }

        public static void AddAppSettings(IConfiguration configuration)
        {
            if (configuration.GetSection("mainDbConnectionString").Exists())
            {
                MainDbConnectionString = configuration.GetSection("mainDbConnectionString").Get<string>();
            }
        }
    }
}

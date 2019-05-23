using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static string GetDataHubPath(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return configuration["Hubs:Data"];
        }

        public static string GetMachineHubPath(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return configuration["Hubs:Machine"];
        }

        #endregion
    }
}

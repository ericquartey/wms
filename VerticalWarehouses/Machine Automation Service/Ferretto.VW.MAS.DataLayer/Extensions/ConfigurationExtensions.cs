using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    internal static class ConfigurationExtensions
    {
        #region Fields

        private const string CacheExpirationTimespanKey = "Vertimag:DataLayer:CacheExpirationTimespan";

        private const string DataLayerConfigurationFileKey = "Vertimag:DataLayer:ConfigurationFile";

        private const string OverrideSetupStatusKey = "Vertimag:DataLayer:OverrideSetupStatus";

        private const string PrimaryConnectionStringName = "AutomationServicePrimary";

        private const string SecondaryConnectionStringName = "AutomationServiceSecondary";

        #endregion

        #region Methods

        public static string GetDataLayerConfigurationFile(this IConfiguration configuration)
        {
            return configuration.GetValue<string>(DataLayerConfigurationFileKey);
        }

        public static string GetDataLayerPrimaryConnectionString(this IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(PrimaryConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception($"Connection string '{PrimaryConnectionStringName}' cannot be null.");
            }

            return connectionString;
        }

        public static string GetDataLayerSecondaryConnectionString(this IConfiguration configuration)
        {
            return configuration.GetConnectionString(SecondaryConnectionStringName);
        }

        public static MemoryCacheEntryOptions GetMemoryCacheOptions(this IConfiguration configuration)
        {
            var timeSpan = configuration.GetValue(CacheExpirationTimespanKey, TimeSpan.FromMinutes(1));

            return new MemoryCacheEntryOptions().SetSlidingExpiration(timeSpan);
        }

        #endregion
    }
}

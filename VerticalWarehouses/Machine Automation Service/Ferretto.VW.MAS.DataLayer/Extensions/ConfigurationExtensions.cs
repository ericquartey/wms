using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    internal static class ConfigurationExtensions
    {
        #region Fields

        private const string CacheExpirationTimespanKey = "Vertimag:DataLayer:CacheExpirationTimespan";

        private const string PrimaryConnectionStringName = "AutomationServicePrimary";

        private const string SecondaryConnectionStringName = "AutomationServiceSecondary";

        #endregion

        #region Methods

        public static string GetCellsConfigurationFile(this IConfiguration configuration)
        {
            return configuration.GetValue("Vertimag:DataLayer:CellsFile", "cells.json");
        }

        public static string GetDataLayerConfigurationFile(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Vertimag:DataLayer:ConfigurationFile");
        }

        public static string GetDataLayerPrimaryConnectionString(this IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(PrimaryConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.Exception($"Connection string '{PrimaryConnectionStringName}' cannot be null.");
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

        public static bool IsSetupStatusOverridden(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Vertimag:DataLayer:OverrideSetupStatus");
        }

        #endregion
    }
}

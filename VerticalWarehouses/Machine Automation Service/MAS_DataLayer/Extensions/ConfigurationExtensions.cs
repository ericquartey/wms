using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer.Extensions
{
    internal static class ConfigurationExtensions
    {
        #region Fields

        private const string PrimaryConnectionStringName = "AutomationServicePrimary";

        private const string SecondaryConnectionStringName = "AutomationServiceSecondary";

        #endregion

        #region Methods

        public static string GetDataLayerConfigurationFile(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Vertimag:DataLayer:ConfigurationFile");
        }

        public static string GetDataLayerPrimaryConnectionString(this IConfiguration configuration)
        {
            return configuration.GetConnectionString(PrimaryConnectionStringName);
        }

        public static string GetDataLayerSecondaryConnectionString(this IConfiguration configuration)
        {
            return configuration.GetConnectionString(SecondaryConnectionStringName);
        }

        #endregion
    }
}

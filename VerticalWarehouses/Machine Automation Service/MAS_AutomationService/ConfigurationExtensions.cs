using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string PrimaryConnectionStringName = "AutomationServicePrimary";

        private const string SecondaryConnectionStringName = "AutomationServiceSecondary";

        private const string WMSServiceAddress = "WMSServiceAddress";

        private const string WMSServiceAddressHubsEndpoint = "WMSServiceAddressHubsEndpoint";

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

        public static string GetDataServiceHubUrl(this IConfiguration configuration)
        {
            return configuration.GetConnectionString(WMSServiceAddressHubsEndpoint);
        }

        public static string GetDataServiceUrl(this IConfiguration configuration)
        {
            return configuration.GetConnectionString(WMSServiceAddress);
        }

        public static bool UseInverterDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Vertimag:InverterDriver:UseMock");
        }

        public static bool UseRemoteIODriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Vertimag:RemoteIODriver:UseMock");
        }

        #endregion
    }
}

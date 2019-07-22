using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string WMSServiceAddress = "WMSServiceAddress";

        private const string WMSServiceAddressHubsEndpoint = "WMSServiceAddressHubsEndpoint";

        #endregion

        #region Methods

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

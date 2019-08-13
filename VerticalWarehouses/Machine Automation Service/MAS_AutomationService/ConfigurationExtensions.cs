using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static System.Uri GetDataServiceHubUrl(this IConfiguration configuration)
        {
            return new System.Uri(configuration.GetConnectionString("WMSServiceAddressHubsEndpoint"));
        }

        public static System.Uri GetDataServiceUrl(this IConfiguration configuration)
        {
            return new System.Uri(configuration.GetConnectionString("WMSServiceAddress"));
        }

        public static bool UseInverterDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Vertimag:InverterDriver:UseMock");
        }

        public static bool UseRemoteIoDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Vertimag:RemoteIODriver:UseMock");
        }

        #endregion
    }
}

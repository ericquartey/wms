using System;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string VertimagInverterDriverUseMockKey = "Vertimag:InverterDriver:UseMock";

        private const string VertimagRemoteIoDriverUseMockKey = "Vertimag:RemoteIODriver:UseMock";

        private const string WmsIsEnabledKey = "WMS:IsEnabled";

        private const string WmsServiceHubUrlKey = "WMS:ServiceHubUrl";

        private const string WmsServiceUrlKey = "WMS:ServiceUrl";

        #endregion

        #region Methods

        public static Uri GetWmsServiceHubUrl(this IConfiguration configuration)
        {
            try
            {
                var valueString = configuration.GetValue<string>(WmsServiceHubUrlKey);

                return new Uri(valueString);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{WmsServiceHubUrlKey}' is not specified or invalid.", ex);
            }
        }

        public static Uri GetWmsServiceUrl(this IConfiguration configuration)
        {
            try
            {
                var valueString = configuration.GetValue<string>(WmsServiceUrlKey);

                return new Uri(valueString);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{WmsServiceHubUrlKey}' is not specified or invalid.", ex);
            }
        }

        public static bool IsWmsEnabled(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>(WmsIsEnabledKey);
        }

        public static bool UseRemoteIoDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>(VertimagRemoteIoDriverUseMockKey);
        }

        #endregion
    }
}

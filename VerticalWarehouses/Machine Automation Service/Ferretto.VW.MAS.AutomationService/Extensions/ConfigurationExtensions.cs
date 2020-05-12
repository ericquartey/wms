using System;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string VertimagRemoteIoDriverUseMockKey = "Vertimag:Drivers:RemoteIO:UseMock";

        #endregion

        #region Methods

        public static bool UseRemoteIoDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>(VertimagRemoteIoDriverUseMockKey);
        }

        #endregion
    }
}

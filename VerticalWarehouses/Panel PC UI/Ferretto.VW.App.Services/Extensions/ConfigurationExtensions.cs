using System;
using System.Collections.Specialized;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static BayNumber GetBayNumber(this NameValueCollection appSettings)
        {
            var bayNumberStringEnv = Environment.GetEnvironmentVariable("BAY_NUMBER");
            if (!string.IsNullOrWhiteSpace(bayNumberStringEnv))
            {
                return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberStringEnv);
            }

            var bayNumberString = appSettings.Get("BayNumber");
            return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberString);
        }

        public static bool LogoutWhenUnhealthy(this NameValueCollection appSettings)
        {
            var logoutWhenUnhealthyStringEnv = Environment.GetEnvironmentVariable("LOGOUT_WHEN_UNHEALTHY");
            if (!string.IsNullOrWhiteSpace(logoutWhenUnhealthyStringEnv))
            {
                return bool.Parse(logoutWhenUnhealthyStringEnv);
            }

            var valueString = appSettings.Get("AutomationService:HealthChecks:LogoutWhenUnhealthy");
            return bool.Parse(valueString);
        }

        #endregion
    }
}

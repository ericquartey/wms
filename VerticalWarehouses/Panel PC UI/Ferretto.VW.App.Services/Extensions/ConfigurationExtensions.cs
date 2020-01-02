using System;
using System.Collections.Specialized;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string BayNumberEnvKey = "BAY_NUMBER";

        private const string BayNumberKey = "BayNumber";

        private const string LogoutWhenUnhealthyEnvKey = "LOGOUT_WHEN_UNHEALTHY";

        private const string LogoutWhenUnhealthyKey = "AutomationService:HealthChecks:LogoutWhenUnhealthy";

        private const string WmsServiceEnabledKey = "WMS:DataService:Enabled";

        #endregion

        #region Methods

        public static BayNumber GetBayNumber(this NameValueCollection appSettings)
        {
            var bayNumberStringEnv = Environment.GetEnvironmentVariable(BayNumberEnvKey);
            if (!string.IsNullOrWhiteSpace(bayNumberStringEnv))
            {
                return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberStringEnv);
            }

            var bayNumberString = appSettings.Get(BayNumberKey);
            return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberString);
        }

        public static string GetLabelPrinterName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("Devices:LabelPrinter");
        }

        public static bool GetWmsDataServiceEnabled(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                var enabledString = appSettings.Get(WmsServiceEnabledKey);
                return bool.Parse(enabledString);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{WmsServiceEnabledKey}' is not specified or invalid.", ex);
            }
        }

        public static bool LogoutWhenUnhealthy(this NameValueCollection appSettings)
        {
            var logoutWhenUnhealthyStringEnv = Environment.GetEnvironmentVariable(LogoutWhenUnhealthyEnvKey);
            if (!string.IsNullOrWhiteSpace(logoutWhenUnhealthyStringEnv))
            {
                return bool.Parse(logoutWhenUnhealthyStringEnv);
            }

            var valueString = appSettings.Get(LogoutWhenUnhealthyKey);
            return bool.Parse(valueString);
        }

        #endregion
    }
}

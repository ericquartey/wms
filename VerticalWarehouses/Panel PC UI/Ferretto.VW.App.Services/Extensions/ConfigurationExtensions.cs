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

        private const string OverrideSetupStatusKey = "OverrideSetupStatus";

        private const string UseOldTrueTableForShutterKey = "Workaround:UseOldTrueTableForShutter";

        private const string WmsServiceEnabledDefaultValue = "False";

        private const string WmsServiceEnabledEnvKey = "WMS_ENABLED";

        #endregion

        #region Methods

        public static string GetAutomationServiceName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("MAS:Service:Name");
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static BayNumber GetBayNumber(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var bayNumberStringEnv = Environment.GetEnvironmentVariable(BayNumberEnvKey);
            if (!string.IsNullOrWhiteSpace(bayNumberStringEnv))
            {
                return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberStringEnv);
            }

            var bayNumberString = appSettings.Get(BayNumberKey);
            return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberString);
        }

        public static bool GetOverrideSetupStatus(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var sessionUser = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();
            var enableOverrideSetupStatusString = appSettings.Get(OverrideSetupStatusKey);
            return (bool.Parse(enableOverrideSetupStatusString) || (sessionUser.UserAccessLevel == UserAccessLevel.Admin));
        }

        public static bool LogoutWhenUnhealthy(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var logoutWhenUnhealthyStringEnv = Environment.GetEnvironmentVariable(LogoutWhenUnhealthyEnvKey);
            if (!string.IsNullOrWhiteSpace(logoutWhenUnhealthyStringEnv))
            {
                return bool.Parse(logoutWhenUnhealthyStringEnv);
            }

            var valueString = appSettings.Get(LogoutWhenUnhealthyKey);
            return bool.Parse(valueString);
        }

        public static bool UseOldTrueTableForShutter(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var useOldTrueTableForShutterString = appSettings.Get(UseOldTrueTableForShutterKey);
            return bool.Parse(useOldTrueTableForShutterString);
        }

        #endregion
    }
}

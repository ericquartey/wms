using System;
using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string WmsServiceHubUrlEnvName = "WMS_DATASERVICE_HUBS_DATA_PATH";

        private const string WmsServiceHubUrlKey = "WMS:DataService:Hubs:Data:Path";

        private const string WmsServiceUrlEnvName = "WMS_DATASERVICE_URL";

        private const string WmsServiceUrlKey = "WMS:DataService:Url";

        #endregion

        #region Methods

        public static string GetAutomationServiceInstallationHubPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("AutomationService:Hubs:Installation:Path");
        }

        public static string GetAutomationServiceLiveHealthPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("AutomationService:HealthChecks:Live:Path");
        }

        public static string GetAutomationServiceOperatorHubPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("AutomationService:Hubs:Operator:Path");
        }

        public static string GetAutomationServiceReadyHealthPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("AutomationService:HealthChecks:Ready:Path");
        }

        public static Uri GetAutomationServiceUrl(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return new Uri(appSettings.Get("AutomationService:Url"));
        }

        public static Uri GetWMSDataServiceHubDataPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                var environmentVariable = Environment.GetEnvironmentVariable(WmsServiceHubUrlEnvName);
                if (!string.IsNullOrWhiteSpace(environmentVariable))
                {
                    return new Uri(environmentVariable);
                }

                return new Uri(appSettings.Get(WmsServiceHubUrlKey));
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{WmsServiceHubUrlKey}' is not specified or invalid.", ex);
            }
        }

        public static Uri GetWMSDataServiceUrl(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                var environmentVariable = Environment.GetEnvironmentVariable(WmsServiceUrlEnvName);
                if (!string.IsNullOrWhiteSpace(environmentVariable))
                {
                    return new Uri(environmentVariable);
                }

                return new Uri(appSettings.Get(WmsServiceUrlKey));
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{WmsServiceUrlKey}' is not specified or invalid.", ex);
            }
        }

        #endregion
    }
}

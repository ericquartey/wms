using System;
using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
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

        public static string GetAutomationServiceName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get("MAS:Service:Name");
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

        public static Uri GetTelemetryHubPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return new Uri(appSettings.Get("TelemetryService:Hubs:Path"));
        }

        #endregion
    }
}

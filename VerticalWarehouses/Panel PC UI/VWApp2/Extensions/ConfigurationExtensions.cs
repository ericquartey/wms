using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static string GetAutomationServiceInstallationHubPath(this NameValueCollection appSettings)
        {
            return appSettings.Get("AutomationService:Hubs:Installation:Path");
        }

        public static string GetAutomationServiceLiveHealthPath(this NameValueCollection appSettings)
        {
            return appSettings.Get("AutomationService:HealthChecks:Live:Path");
        }

        public static string GetAutomationServiceOperatorHubPath(this NameValueCollection appSettings)
        {
            return appSettings.Get("AutomationService:Hubs:Operator:Path");
        }

        public static string GetAutomationServiceReadyHealthPath(this NameValueCollection appSettings)
        {
            return appSettings.Get("AutomationService:HealthChecks:Ready:Path");
        }

        public static System.Uri GetAutomationServiceUrl(this NameValueCollection appSettings)
        {
            return new System.Uri(appSettings.Get("AutomationService:Url"));
        }

        #endregion
    }
}

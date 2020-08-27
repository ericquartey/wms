using System;
using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static string GetBayNumber(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var bayNumberStringEnv = Environment.GetEnvironmentVariable("BAY_NUMBER");
            if (!string.IsNullOrWhiteSpace(bayNumberStringEnv))
            {
                return bayNumberStringEnv;
            }

            return appSettings.Get("BayNumber");
        }

        #endregion
    }
}

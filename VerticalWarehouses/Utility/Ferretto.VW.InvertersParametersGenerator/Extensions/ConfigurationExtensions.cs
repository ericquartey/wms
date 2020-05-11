using System;
using System.Collections.Specialized;

namespace Ferretto.VW.InvertersParametersGenerator
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string InvertersParametersRootPath = "InvertersParameters:Root:Path";

        private const string VertimagConfigurationRootPath = "VertimagConfiguration:Root:Pat";

        #endregion

        #region Methods

        public static string GetInvertersParametersRootPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InvertersParametersRootPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InvertersParametersRootPath}' is not specified or invalid.", ex);
            }
        }

        public static string GetVertimagConfigurationRootPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(VertimagConfigurationRootPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{VertimagConfigurationRootPath}' is not specified or invalid.", ex);
            }
        }

        #endregion
    }
}

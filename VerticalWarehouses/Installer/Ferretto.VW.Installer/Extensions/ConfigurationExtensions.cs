using System;
using System.Collections.Specialized;

namespace Ferretto.VW.Installer
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string InstallDefaultMasIpaddressName = "Install:Default:MasIpaddress";

        private const string InstallDefaultMasIpportName = "Install:Default:MasIpport";

        #endregion

        #region Methods

        public static string GetInstallDefaultMasIpaddress(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallDefaultMasIpaddressName);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallDefaultMasIpaddressName}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallDefaultMasIpport(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallDefaultMasIpportName);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallDefaultMasIpportName}' is not specified or invalid.", ex);
            }
        }

        #endregion
    }
}

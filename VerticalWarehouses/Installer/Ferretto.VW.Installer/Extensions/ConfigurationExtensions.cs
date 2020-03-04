using System;
using System.Collections.Specialized;

namespace Ferretto.VW.Installer
{
    public static class ConfigurationExtensions
    {
        #region Fields
        
        private const string InstallRootPath = "Install:Root:Path";

        private const string UpdateTempPath = "Update:Temp:Path";

        private const string InstallBay1Ipaddress = "Install:Bay1:Ipaddress";

        private const string InstallBay2Ipaddress = "Install:Bay2:Ipaddress";

        private const string InstallBay3Ipaddress = "Install:Bay3:Ipaddress";

        private const string InstallDefaultMasIpaddressName = "Install:Default:MasIpaddress";

        private const string InstallDefaultMasIpportName = "Install:Default:MasIpport";

        private const string InstallPpcPath = "Install:PPC:Path";

        private const string InstallPpcFilePath = "Install:PPC:FilePath";


        private const string InstallMasPath = "Install:MAS:Path";

        #endregion

        #region Methods
        
        public static string GetIGetInstallPpcFilePath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallPpcFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallPpcFilePath}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallPpcPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallPpcPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallPpcPath}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallRootPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallRootPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallRootPath}' is not specified or invalid.", ex);
            }
        }

        public static string GetUpdateTempPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(UpdateTempPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{UpdateTempPath}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallBay1Ipaddress(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallBay1Ipaddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallBay1Ipaddress}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallBay2Ipaddress(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallBay2Ipaddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallBay2Ipaddress}' is not specified or invalid.", ex);
            }
        }

        public static string GetInstallBay3Ipaddress(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallBay3Ipaddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallBay3Ipaddress}' is not specified or invalid.", ex);
            }
        }

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

        public static string GetInstallMasPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallMasPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallMasPath}' is not specified or invalid.", ex);
            }
        }

        #endregion
    }
}

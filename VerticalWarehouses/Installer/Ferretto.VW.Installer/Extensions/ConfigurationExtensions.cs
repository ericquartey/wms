using System;
using System.Collections.Specialized;
using System.Net;

namespace Ferretto.VW.Installer
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string InstallBay1Ipaddress = "Install:Bay1:Ipaddress";

        private const string InstallBay2Ipaddress = "Install:Bay2:Ipaddress";

        private const string InstallBay3Ipaddress = "Install:Bay3:Ipaddress";

        private const string InstallDefaultMasUrl = "Install:Default:MasUrl";

        private const string InstallerDirNameKey = "Installer:DirName";

        private const string InstallRootPath = "Install:Root:Path";

        private const string MasDirNameKey = "MAS:DirName";

        private const string PpcDirNameKey = "PPC:DirName";

        private const string PpcFileNameKey = "PPC:FileName";

        private const string UpdateTempPath = "Update:Temp:Path";

        #endregion

        #region Methods

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

        public static Uri GetInstallDefaultMasUrl(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var uriString = appSettings.Get(InstallDefaultMasUrl);
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                return uri;
            }
            else
            {
                throw new Exception($"The configuration key '{InstallDefaultMasUrl}' is not specified or invalid.");
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static string GetInstallerDirName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(InstallerDirNameKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{InstallerDirNameKey}' is not specified or invalid.", ex);
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

        public static string GetMasDirName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(MasDirNameKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{MasDirNameKey}' is not specified or invalid.", ex);
            }
        }

        public static string GetPpcDirName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(PpcDirNameKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{PpcDirNameKey}' is not specified or invalid.", ex);
            }
        }

        public static string GetPpcFileName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            try
            {
                return appSettings.Get(PpcFileNameKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"The configuration key '{PpcFileNameKey}' is not specified or invalid.", ex);
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

        #endregion
    }
}

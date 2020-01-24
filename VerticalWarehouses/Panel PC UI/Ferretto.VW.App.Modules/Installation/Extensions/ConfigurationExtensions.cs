﻿using System;
using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string UpdateExchangeInstallerName = "Update:Exchange:Installer:Name";

        private const string UpdateExchangeInstallerPath = "Update:Exchange:Installer:Path";

        private const string UpdateExchangeTemp = "Update:Exchange:Temp";

        private const string UpdateRepositoryPath = "Update:Repository:Path";

        private const string UpdateZipChecksumFileName = "Update:Zip:Checksum:FileName";

        #endregion

        #region Methods

        public static string GetUpdateExchangeInstallerName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdateExchangeInstallerName);
        }

        public static string GetUpdateExchangeInstallerPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdateExchangeInstallerPath);
        }

        public static string GetUpdateExchangeTemp(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdateExchangeTemp);
        }

        public static string GetUpdateRepositoryPath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdateRepositoryPath);
        }

        public static string GetUpdateZipChecksumFileName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdateZipChecksumFileName);
        }

        #endregion
    }
}

using System;
using System.Collections.Specialized;

namespace Ferretto.VW.App
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string UpdatePath = "Update:Path";

        #endregion

        #region Methods

        public static string GetUpdatePath(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return appSettings.Get(UpdatePath);
        }

        #endregion
    }
}

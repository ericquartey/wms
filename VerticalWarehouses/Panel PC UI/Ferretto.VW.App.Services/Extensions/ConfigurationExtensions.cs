using System.Collections.Specialized;

namespace Ferretto.VW.App.Services
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static int GetBayNumber(this NameValueCollection appSettings)
        {
            var bayNumberString = appSettings.Get("BayNumber");
            return int.Parse(bayNumberString);
        }

        #endregion
    }
}

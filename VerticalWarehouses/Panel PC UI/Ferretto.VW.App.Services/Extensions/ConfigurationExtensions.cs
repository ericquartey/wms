using System;
using System.Collections.Specialized;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.App.Services
{
    public static class ConfigurationExtensions
    {


        #region Methods

        public static BayNumber GetBayNumber(this NameValueCollection appSettings)
        {
            var bayNumberString = appSettings.Get("BayNumber");
            return (BayNumber)Enum.Parse(typeof(BayNumber), bayNumberString);
        }

        #endregion
    }
}

using System;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.TimeManagement
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string WmsIsEnabledKey = "WMS:IsEnabled";

        #endregion

        #region Methods

        public static bool IsWmsEnabled(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>(WmsIsEnabledKey);
        }

        #endregion
    }
}

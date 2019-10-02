using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.MissionsManager
{
    public static class ConfigurationExtensions
    {
        #region Methods

        public static bool IsWmsEnabled(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("WMS:IsEnabled");
        }

        #endregion
    }
}

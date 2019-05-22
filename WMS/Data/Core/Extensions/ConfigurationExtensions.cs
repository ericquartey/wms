using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string DefaultImagesPath = "Images\\";

        private const int DefaultMaxImageDimension = 1024;

        private const int DefaultMaxPageSize = 200;

        #endregion

        #region Methods

        public static string GetDataHubPath(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return configuration["Hubs:Data"];
        }

        public static string GetImagesPath(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return configuration["Image:Path"] ?? DefaultImagesPath;
        }

        public static string GetMachineHubPath(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return configuration["Hubs:Machine:Url"];
        }

        public static int GetMaxImageDimension(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return int.TryParse(configuration["Image:DefaultPixelMax"], out var maxImageDimension)
                ? maxImageDimension
                : DefaultMaxImageDimension;
        }

        public static int GetMaxMachineReconnectTimeoutMilliseconds(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            if (int.TryParse(configuration["Hubs:Machine:MaxReconnectTimeoutMilliseconds"], out var timeout))
            {
                return timeout;
            }

            return 10000;
        }

        public static int GetMaxPageSize(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            return int.TryParse(configuration["MaxTake"], out var maxPageSize)
               ? maxPageSize
               : DefaultMaxPageSize;
        }

        #endregion
    }
}

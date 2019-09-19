using System;
using Ferretto.VW.MAS.IODriver.Interface.Services;
using Ferretto.VW.MAS.IODriver.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.DataLayer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddIODriver(
                    this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IIoDeviceService, IoDeviceService>();

            return services;
        }

        #endregion
    }
}

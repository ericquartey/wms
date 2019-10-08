using System;
using Ferretto.VW.MAS.IODriver.Interface.Services;
using Ferretto.VW.MAS.IODriver.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.IODriver
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddIODriver(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<HostedIoDriver>();

            services.AddSingleton<IIoDevicesProvider, IoDevicesProvider>();

            return services;
        }

        #endregion
    }
}

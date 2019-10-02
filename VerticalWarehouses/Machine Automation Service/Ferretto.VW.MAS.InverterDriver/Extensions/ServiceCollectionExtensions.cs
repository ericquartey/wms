using System;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.InverterDriver.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddInverterDriver(
                    this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IInvertersProvider, InvertersProvider>();

            return services;
        }

        #endregion
    }
}

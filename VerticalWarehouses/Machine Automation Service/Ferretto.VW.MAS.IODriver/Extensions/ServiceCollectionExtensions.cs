using System;
using Ferretto.VW.MAS.DataModels;
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

            services.AddHostedService<IoDriverService>();

            services.AddSingleton<IIoDevicesProvider, IoDevicesProvider>();
            services.AddScoped<ISecurityIsClearedConditionEvaluator, SecurityIsClearedConditionEvaluator>();

            return services;
        }

        #endregion
    }
}

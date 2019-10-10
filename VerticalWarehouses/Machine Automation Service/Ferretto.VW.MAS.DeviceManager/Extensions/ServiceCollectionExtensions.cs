using System;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.DeviceManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddFiniteStateMachines(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<DeviceManager>();

            services
                .AddSingleton<IMachineResourcesProvider, MachineResourcesProvider>()
                .AddSingleton(s => s.GetRequiredService<IMachineResourcesProvider>() as ISensorsProvider)
                .AddSingleton<IElevatorProvider, ElevatorProvider>()
                .AddSingleton<IMachineControlProvider, MachineControlProvider>()
                .AddSingleton<IBayChainProvider, BayChainProvider>();

            return services;
        }

        #endregion
    }
}

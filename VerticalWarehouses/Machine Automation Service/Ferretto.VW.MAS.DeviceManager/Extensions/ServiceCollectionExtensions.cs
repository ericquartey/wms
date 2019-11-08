using System;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.DeviceManager
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

            services.AddHostedService<DeviceManagerService>();

            services
                .AddTransient<IElevatorHorizontalChainZeroConditionEvaluator, ElevatorHorizontalChainZeroConditionEvaluator>();

            services
                .AddSingleton<IMachineResourcesProvider, MachineResourcesProvider>()
                .AddSingleton(s => s.GetRequiredService<IMachineResourcesProvider>() as ISensorsProvider)
                .AddTransient<IElevatorProvider, ElevatorProvider>()
                .AddTransient<IShutterProvider, ShutterProvider>()
                .AddTransient<IMachineControlProvider, MachineControlProvider>()
                .AddTransient<ILoadingUnitMovementProvider, LoadingUnitMovementProvider>()
                .AddSingleton<ICarouselProvider, CarouselProvider>();

            return services;
        }

        #endregion
    }
}

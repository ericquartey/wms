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
                .AddScoped<IBayTelescopicZeroConditionEvaluator, BayTelescopicZeroConditionEvaluator>()
                .AddScoped<IElevatorHorizontalChainZeroConditionEvaluator, ElevatorHorizontalChainZeroConditionEvaluator>()
                .AddScoped<IElevatorOverrunConditionEvaluator, ElevatorOverrunConditionEvaluator>();

            services
                .AddSingleton<IMachineResourcesProvider, MachineResourcesProvider>()
                .AddScoped(s => s.GetRequiredService<IMachineResourcesProvider>() as ISensorsProvider)
                .AddScoped<IElevatorProvider, ElevatorProvider>()
                .AddScoped<IShutterProvider, ShutterProvider>()
                .AddScoped<IMachineControlProvider, MachineControlProvider>()
                .AddScoped<ILoadingUnitMovementProvider, LoadingUnitMovementProvider>()
                .AddScoped<ICarouselProvider, CarouselProvider>()
                .AddScoped<IInverterProgrammingProvider, InverterProgrammingProvider>()
                .AddScoped<IExternalBayProvider, ExternalBayProvider>();

            return services;
        }

        #endregion
    }
}

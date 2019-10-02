using System;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.FiniteStateMachines
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

            services.AddHostedService<FiniteStateMachines>();

            services
                .AddSingleton<IMachineResourcesProvider, MachineResourcesProvider>()
                .AddSingleton(s => s.GetRequiredService<IMachineResourcesProvider>() as ISensorsProvider)
                .AddSingleton<IElevatorProvider, ElevatorProvider>();

            return services;
        }

        #endregion
    }
}

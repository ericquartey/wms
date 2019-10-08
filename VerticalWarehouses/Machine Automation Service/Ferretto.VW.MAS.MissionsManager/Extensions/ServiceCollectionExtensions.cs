using System;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MissionsManager.Providers;
using Ferretto.VW.MAS.MissionsManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MissionsManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddMissionsManager(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<BackgroundServices.MissionsManagerService>();

            services
                .AddTransient<IRunningStateProvider, RunningStateProvider>();

            services
                .AddTransient<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>()
                .AddTransient<IChangeRunningStateStartState, ChangeRunningStateStartState>()
                .AddTransient<IChangeRunningStateResetFaultState, ChangeRunningStateResetFaultState>()
                .AddTransient<IChangeRunningStateResetSecurity, ChangeRunningStateResetSecurity>()
                .AddTransient<IChangeRunningStateInverterPowerSwitch, ChangeRunningStateInverterPowerSwitch>()
                .AddTransient<IChangeRunningStateEndState, ChangeRunningStateEndState>();

            return services;
        }

        #endregion
    }
}

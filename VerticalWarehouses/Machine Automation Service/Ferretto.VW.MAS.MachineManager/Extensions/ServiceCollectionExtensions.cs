using System;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MachineManager
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddMachineManager(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<MachineManagerService>();

            services
                .AddScoped<IRunningStateProvider, RunningStateProvider>()
                .AddScoped<IMoveLoadingUnitProvider, MoveLoadingUnitProvider>()
                .AddScoped<IMissionMoveProvider, MissionMoveProvider>()
                .AddScoped<IMachineModeProvider, MachineModeProvider>();

            services
                .AddScoped<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>();

            services
                .AddScoped<IChangeRunningStateStartState, ChangeRunningStateStartState>()
                .AddScoped<IChangeRunningStateResetFaultState, ChangeRunningStateResetFaultState>()
                .AddScoped<IChangeRunningStateResetSecurity, ChangeRunningStateResetSecurity>()
                .AddScoped<IChangeRunningStateInverterPowerSwitch, ChangeRunningStateInverterPowerSwitch>()
                .AddScoped<IChangeRunningStateEndState, ChangeRunningStateEndState>();

            return services;
        }

        #endregion
    }
}

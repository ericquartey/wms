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
                .AddTransient<IRunningStateProvider, RunningStateProvider>()
                .AddTransient<IMoveLoadingUnitProvider, MoveLoadingUnitProvider>()
                .AddTransient<IMissionMoveProvider, MissionMoveProvider>()
                .AddTransient<IMachineModeProvider, MachineModeProvider>();

            services
                .AddTransient<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>();

            services
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

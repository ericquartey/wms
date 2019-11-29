using System;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
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
                .AddTransient<IMachineModeProvider, MachineModeProvider>();

            services
                .AddTransient<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>()
                .AddTransient<IMoveLoadingUnitStateMachine, MoveLoadingUnitStateMachine>();

            services
                .AddTransient<IChangeRunningStateStartState, ChangeRunningStateStartState>()
                .AddTransient<IChangeRunningStateResetFaultState, ChangeRunningStateResetFaultState>()
                .AddTransient<IChangeRunningStateResetSecurity, ChangeRunningStateResetSecurity>()
                .AddTransient<IChangeRunningStateInverterPowerSwitch, ChangeRunningStateInverterPowerSwitch>()
                .AddTransient<IChangeRunningStateEndState, ChangeRunningStateEndState>();

            services
                .AddTransient<IMoveLoadingUnitStartState, MoveLoadingUnitStartState>()
                .AddTransient<IMoveLoadingUnitLoadElevatorState, MoveLoadingUnitLoadElevatorState>()
                .AddTransient<IMoveLoadingUnitCloseShutterState, MoveLoadingUnitCloseShutterState>()
                .AddTransient<IMoveLoadingUnitMoveToTargetState, MoveLoadingUnitMoveToTargetState>()
                .AddTransient<IMoveLoadingUnitDepositUnitState, MoveLoadingUnitDepositUnitState>()
                .AddTransient<IMoveLoadingUnitWaitEjectConfirm, MoveLoadingUnitWaitEjectConfirm>()
                .AddTransient<IMoveLoadingUnitWaitEjectConfirm, MoveLoadingUnitWaitEjectConfirm>()
                .AddTransient<IMoveLoadingUnitWaitPickConfirm, MoveLoadingUnitWaitPickConfirm>();

            return services;
        }

        #endregion
    }
}

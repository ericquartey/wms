using System;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
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
                .AddSingleton<IRunningStateProvider, RunningStateProvider>()
                .AddSingleton<IMoveLoadingUnitProvider, MoveLoadingUnitProvider>();

            services
                .AddTransient<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>()
                .AddTransient<IMoveLoadingUnitStateMachine, MoveLoadingUnitStateMachine>();

            services
                .AddTransient<IChangeRunningStateStartState, ChangeRunningStateStartState>()
                .AddTransient<IChangeRunningStateResetFaultState, ChangeRunningStateResetFaultState>()
                .AddTransient<IChangeRunningStateResetSecurity, ChangeRunningStateResetSecurity>()
                .AddTransient<IChangeRunningStateInverterPowerSwitch, ChangeRunningStateInverterPowerSwitch>()
                .AddTransient<IChangeRunningStateEndState, ChangeRunningStateEndState>()

                .AddTransient<IMoveLoadingUnitStartState, MoveLoadingUnitStartState>()
                .AddTransient<IMoveLoadingUnitOpenShutterState, IMoveLoadingUnitOpenShutterState>()
                .AddTransient<IMoveLoadingUnitLoadUnitState, MoveLoadingUnitLoadUnitState>()
                .AddTransient<IMoveLoadingUnitMoveToTargetState, MoveLoadingUnitMoveToTargetState>()
                .AddTransient<IMoveLoadingUnitDepositUnitState, MoveLoadingUnitDepositUnitState>()
                .AddTransient<IMoveLoadingUnitEndState, MoveLoadingUnitEndState>();

            return services;
        }

        #endregion
    }
}

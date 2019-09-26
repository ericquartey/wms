using System;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.WeightAcquisition;
using Ferretto.VW.MAS.MissionsManager.Providers;
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
                .AddTransient<IWeightAnalysisMissionProvider, WeightAnalysisMissionProvider>()
                .AddTransient<IWeightCheckMissionProvider, WeightCheckMissionProvider>()
                .AddTransient<IRunningStateProvider, RunningStateProvider>();

            services
                .AddTransient<IWeightAcquisitionStateMachine, WeightAcquisitionStateMachine>()
                .AddTransient<IWeightAcquisitionMoveToStartPositionState, WeightAcquisitionMoveToStartPositionState>()
                .AddTransient<IWeightAcquisitionInPlaceSamplingState, WeightAcquisitionInPlaceSamplingState>()
                .AddTransient<IWeightAcquisitionInMotionSamplingState, WeightAcquisitionInMotionSamplingState>()
                .AddTransient<IWeightAcquisitionMoveBackToStartPositionState, WeightAcquisitionMoveBackToStartPositionState>();

            services
                .AddTransient<IChangeRunningStateStateMachine, ChangeRunningStateStateMachine>()
                .AddTransient<IChangeRunningStateStartState, ChangeRunningStateStartState>()
                .AddTransient<IChangeRunningStateResetFaultState, ChangeRunningStateResetFaultState>()
                .AddTransient<IChangeRunningStateResetSecurity, ChangeRunningStateResetSecurity>()
                .AddTransient<IChangeRunningStateEndState, ChangeRunningStateEndState>();

            return services;
        }

        #endregion
    }
}

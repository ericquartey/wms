using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus.States;
using Ferretto.VW.MAS.MissionsManager.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MissionsManager
{
    public static class ServiceCollectionExtensions
    {


        #region Methods

        public static IServiceCollection AddMissionsManager(this IServiceCollection services)
        {
            if (services is null)
            {
                return services;
            }

            services.AddHostedService<MissionsManagerService>();

            services
                .AddTransient<IWeightAnalysisMissionProvider, WeightAnalysisMissionProvider>()
                .AddTransient<IWeightCheckMissionProvider, WeightCheckMissionProvider>();

            services
                .AddTransient<IWeightAcquisitionStateMachine, WeightAcquisitionStateMachine>()
                .AddTransient<IWeightAcquisitionMoveToStartPositionState, WeightAcquisitionMoveToStartPositionState>()
                .AddTransient<IWeightAcquisitionInPlaceSamplingState, WeightAcquisitionInPlaceSamplingState>()
                .AddTransient<IWeightAcquisitionInMotionSamplingState, WeightAcquisitionInMotionSamplingState>()
                .AddTransient<IWeightAcquisitionMoveBackToStartPositionState, WeightAcquisitionMoveBackToStartPositionState>();

            services
                .AddTransient<IChangePowerStatusStateMachine, ChangePowerStatusStateMachine>()
                .AddTransient<IChangePowerStatusStartState, ChangePowerStatusStartState>();
            return services;
        }

        #endregion
    }
}

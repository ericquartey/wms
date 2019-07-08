using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class MissionPolicyExtensions
    {
        #region Methods

        public static Policy ComputeAbortPolicy(this IMissionPolicy mission)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Abort),
                Type = PolicyType.Operation
            };

            if (mission.OperationsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Mission.OnlyLoadingUnitMissionsCanBeAborted);
            }

            if (mission.Status != MissionStatus.Executing
                &&
                mission.Status != MissionStatus.New
                &&
                mission.Status != MissionStatus.Error)
            {
                policy.AddErrorMessage(
                    Resources.Mission.UnableToAbortTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeCompletePolicy(this IMissionPolicy mission)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Complete),
                Type = PolicyType.Operation
            };

            if (mission.OperationsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Mission.OnlyLoadingUnitMissionsCanBeCompleted);
            }

            if (mission.Status != MissionStatus.Executing)
            {
                policy.AddErrorMessage(
                    Resources.Mission.UnableToCompleteTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeExecutePolicy(this IMissionPolicy mission)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Execute),
                Type = PolicyType.Operation
            };

            if (mission.OperationsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Mission.OnlyLoadingUnitMissionsCanBeExecuted);
            }

            if (mission.Status != MissionStatus.New)
            {
                policy.AddErrorMessage(
                    Resources.Mission.UnableToExecuteTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        #endregion
    }
}

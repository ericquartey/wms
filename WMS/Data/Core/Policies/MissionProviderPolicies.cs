using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces.Policies;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class MissionProviderPolicies
    {
        #region Methods

        public static Policy ComputeAbortPolicy(this IMissionPolicy missionToAbort)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Abort),
                Type = PolicyType.Operation
            };

            if (missionToAbort.Status != MissionStatus.Executing
                &&
                missionToAbort.Status != MissionStatus.New
                &&
                missionToAbort.Status != MissionStatus.Error)
            {
                policy.AddErrorMessage(Resources.Mission.UnableToAbortTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeCompletePolicy(this IMissionPolicy missionToComplete)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Complete),
                Type = PolicyType.Operation
            };

            if (missionToComplete.Status != MissionStatus.Executing)
            {
                policy.AddErrorMessage(Resources.Mission.UnableToCompleteTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeExecutePolicy(this IMissionPolicy missionToExecute)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Execute),
                Type = PolicyType.Operation
            };

            if (missionToExecute.Status != MissionStatus.New)
            {
                policy.AddErrorMessage(Resources.Mission.UnableToExecuteTheMissionBecauseOfItsCurrentState);
            }

            return policy;
        }

        #endregion
    }
}

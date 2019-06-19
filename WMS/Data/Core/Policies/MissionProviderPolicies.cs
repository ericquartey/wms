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
            string reason = null;
            if (missionToAbort.Status != MissionStatus.Executing
                &&
                missionToAbort.Status != MissionStatus.New
                &&
                missionToAbort.Status != MissionStatus.Error)
            {
                reason = $"Unable to abort the mission, because it is not in the New or Executing or Error state.";
            }

            return new Policy
            {
                IsAllowed = reason == null,
                Reason = reason,
                Name = nameof(MissionPolicy.Abort),
                Type = PolicyType.Operation,
            };
        }

        public static Policy ComputeCompletePolicy(this IMissionPolicy missionToComplete)
        {
            string reason = null;
            if (missionToComplete.Status != MissionStatus.Executing)
            {
                reason = $"Unable to complete the mission, because it is not in the Executing state.";
            }

            return new Policy
            {
                IsAllowed = reason == null,
                Reason = reason,
                Name = nameof(MissionPolicy.Complete),
                Type = PolicyType.Operation,
            };
        }

        public static Policy ComputeExecutePolicy(this IMissionPolicy missionToExecute)
        {
            string reason = null;
            if (missionToExecute.Status != MissionStatus.New)
            {
                reason = $"Unable to execute the mission, because it is not in the New state.";
            }

            return new Policy
            {
                IsAllowed = reason == null,
                Reason = reason,
                Name = nameof(MissionPolicy.Execute),
                Type = PolicyType.Operation,
            };
        }

        #endregion
    }
}

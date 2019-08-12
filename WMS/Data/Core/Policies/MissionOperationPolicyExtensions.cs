using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class MissionOperationPolicyExtensions
    {
        #region Methods

        public static Policy ComputeAbortPolicy(this IMissionOperationPolicy missionOperation)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Abort),
                Type = PolicyType.Operation,
            };

            if (missionOperation.Status != Enums.MissionOperationStatus.Executing
                &&
                missionOperation.Status != Enums.MissionOperationStatus.New
                &&
                missionOperation.Status != Enums.MissionOperationStatus.Error)
            {
                policy.AddErrorMessage(Resources.MissionOperation.UnableToAbortTheOperationBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeCompletePolicy(this IMissionOperationPolicy missionToComplete)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Complete),
                Type = PolicyType.Operation,
            };

            if (missionToComplete.Status != Enums.MissionOperationStatus.Executing)
            {
                policy.AddErrorMessage(Resources.MissionOperation.UnableToCompleteTheOperationBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeExecutePolicy(this IMissionOperationPolicy missionToExecute)
        {
            var policy = new Policy
            {
                Name = nameof(MissionPolicy.Execute),
                Type = PolicyType.Operation,
            };

            if (missionToExecute.Status != Enums.MissionOperationStatus.New)
            {
                policy.AddErrorMessage(Resources.MissionOperation.UnableToExecuteTheOperationBecauseOfItsCurrentState);
            }

            return policy;
        }

        #endregion
    }
}

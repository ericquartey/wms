using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class LoadingUnitPolicyExtensions
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this ILoadingUnitDeletePolicy loadingUnitToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (loadingUnitToDelete.CompartmentsCount > 0)
            {
                policy.AddErrorMessage(Resources.LoadingUnit.CannotDeleteTheLoadingUnitBecauseItHasAssociatedCompartments);
            }

            if (loadingUnitToDelete.ActiveMissionsCount > 0)
            {
                policy.AddErrorMessage(Resources.LoadingUnit.CannotDeleteTheLoadingUnitBecauseItHasAssociatedActiveMissions);
            }

            if (loadingUnitToDelete.ActiveSchedulerRequestsCount > 0)
            {
                policy.AddErrorMessage(Resources.LoadingUnit.CannotDeleteTheLoadingUnitBecauseItHasAssociatedActiveSchedulerRequests);
            }

            return policy;
        }

        public static Policy ComputeUpdatePolicy(this ILoadingUnitUpdatePolicy model)
        {
            return new Policy
            {
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeWithdrawPolicy(this ILoadingUnitWithdrawPolicy loadingUnitToWithdraw)
        {
            var policy = new Policy
            {
                Name = nameof(LoadingUnitPolicy.Withdraw),
                Type = PolicyType.Operation
            };

            if (loadingUnitToWithdraw.CellId == null)
            {
                policy.AddErrorMessage(Resources.LoadingUnit.CannotWithdrawTheLoadingUnitBecauseItHasNoAssociatedCell);
            }

            return policy;
        }

        #endregion
    }
}

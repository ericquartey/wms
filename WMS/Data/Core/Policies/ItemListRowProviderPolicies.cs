using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemListRowProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this IItemListRowDeletePolicy rowToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (rowToDelete.ActiveSchedulerRequestsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.ItemListRow.CannotDeleteTheItemListRowBecauseItHasAssociatedActiveSchedulerRequests);
            }

            if (rowToDelete.ActiveMissionsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.ItemListRow.CannotDeleteTheItemListRowBecauseItHasAssociatedActiveMissions);
            }

            if (rowToDelete.Status != ItemListRowStatus.New)
            {
                policy.AddErrorMessage(
                    Resources.ItemListRow.CannotDeleteTheItemListRowBecauseItHasAssociatedItIsNotInTheNewState);
            }

            return policy;
        }

        public static Policy ComputeExecutePolicy(this IItemListRowExecutePolicy rowToExecute)
        {
            var policy = new Policy
            {
                Name = nameof(ItemListRowPolicy.Execute),
                Type = PolicyType.Operation
            };

            if (rowToExecute.Status != ItemListRowStatus.New &&
                rowToExecute.Status != ItemListRowStatus.Error &&
                rowToExecute.Status != ItemListRowStatus.Incomplete &&
                rowToExecute.Status != ItemListRowStatus.Suspended &&
                rowToExecute.Status != ItemListRowStatus.Waiting)
            {
                policy.AddErrorMessage(
                    Resources.ItemListRow.CannotExecuteTheItemListRowBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeUpdatePolicy(this IItemListRowUpdatePolicy model)
        {
            return new Policy
            {
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        #endregion
    }
}

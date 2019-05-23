using System.Collections.Generic;
using System.Linq;
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
            var errorMessages = new List<string>();
            if (rowToDelete.ActiveSchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{rowToDelete.ActiveSchedulerRequestsCount}]");
            }

            if (rowToDelete.ActiveMissionsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.Mission} [{rowToDelete.ActiveMissionsCount}]");
            }

            if (rowToDelete.Status != ItemListRowStatus.New)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRowStatus} [{rowToDelete.Status.ToString()}]");
            }

            string reason = null;
            if (errorMessages.Any())
            {
                reason = string.Format(
                    Common.Resources.Errors.NotPossibleExecuteOperation,
                    string.Join(", ", errorMessages.ToArray()));
            }

            return new Policy
            {
                IsAllowed = !errorMessages.Any(),
                Reason = reason,
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeExecutePolicy(this IItemListRowExecutePolicy rowToExecute)
        {
            var errorMessages = new List<string>();

            if (rowToExecute.Status != ItemListRowStatus.New &&
                rowToExecute.Status != ItemListRowStatus.Error &&
                rowToExecute.Status != ItemListRowStatus.Incomplete &&
                rowToExecute.Status != ItemListRowStatus.Suspended)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRowStatus} [{rowToExecute.Status.ToString()}]");
            }

            string reason = null;
            if (errorMessages.Any())
            {
                reason = string.Format(
                    Common.Resources.Errors.NotPossibleExecuteOperation,
                    string.Join(", ", errorMessages.ToArray()));
            }

            return new Policy
            {
                IsAllowed = !errorMessages.Any(),
                Reason = reason,
                Name = nameof(ItemListRowPolicy.Execute),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeUpdatePolicy(this IItemListRowUpdatePolicy model)
        {
            return new Policy
            {
                IsAllowed = true,
                Reason = null,
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        #endregion
    }
}

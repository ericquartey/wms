using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class LoadingUnitProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this ILoadingUnitDeletePolicy loadingUnitToDelete)
        {
            var errorMessages = new List<string>();
            if (loadingUnitToDelete.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{loadingUnitToDelete.CompartmentsCount}]");
            }

            if (loadingUnitToDelete.ActiveMissionsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{loadingUnitToDelete.ActiveMissionsCount}]");
            }

            if (loadingUnitToDelete.ActiveSchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{loadingUnitToDelete.ActiveSchedulerRequestsCount}]");
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

        public static Policy ComputeUpdatePolicy(this ILoadingUnitUpdatePolicy model)
        {
            return new Policy
            {
                IsAllowed = true,
                Reason = null,
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeWithdrawPolicy(this ILoadingUnitWithdrawPolicy loadingUnitToWithdraw)
        {
            var errorMessages = new List<string>();
            if (loadingUnitToWithdraw.CellId == null)
            {
                errorMessages.Add($"{Common.Resources.Errors.LoadingUnitWithoutAssociatedCell}");
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
                Name = nameof(LoadingUnitPolicy.Withdraw),
                Type = PolicyType.Operation
            };
        }

        #endregion
    }
}

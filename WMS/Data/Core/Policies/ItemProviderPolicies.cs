using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this IItemDeletePolicy itemToDelete)
        {
            var errorMessages = new List<string>();
            if (itemToDelete == null)
            {
                return null;
            }

            if (itemToDelete.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{itemToDelete.CompartmentsCount}]");
            }

            if (itemToDelete.ItemListRowsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRow} [{itemToDelete.ItemListRowsCount}]");
            }

            if (itemToDelete.MissionsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{itemToDelete.MissionsCount}]");
            }

            if (itemToDelete.SchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{itemToDelete.SchedulerRequestsCount}]");
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

        public static Policy ComputeItemCompartmentTypeDeletePolicy(this IItemCompartmentTypeDeletePolicy itemToDelete)
        {
            var errorMessages = new List<string>();
            if (itemToDelete == null)
            {
                return null;
            }

            if (itemToDelete.TotalStock > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{itemToDelete.TotalStock}]");
            }

            if (itemToDelete.TotalReservedForPick > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRow} [{itemToDelete.TotalReservedForPick}]");
            }

            if (itemToDelete.TotalReservedToPut > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{itemToDelete.TotalReservedToPut}]");
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

        public static Policy ComputePickPolicy(this IItemPickPolicy itemToWithdraw)
        {
            var errorMessages = new List<string>();
            if (itemToWithdraw == null)
            {
                return null;
            }

            if (itemToWithdraw.TotalAvailable.CompareTo(0) == 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemAvailable} [{itemToWithdraw.TotalAvailable}]");
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
                Name = nameof(ItemPolicy.Pick),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputePutPolicy(this IItemPutPolicy itemToPut)
        {
            var errorMessages = new List<string>();
            if (itemToPut == null)
            {
                return null;
            }

            if (!itemToPut.HasCompartmentTypes)
            {
                errorMessages.Add(Common.Resources.Errors.PutItemNoCompartmentType);
            }

            if (!itemToPut.HasAssociatedAreas)
            {
                errorMessages.Add(Common.Resources.Errors.PutItemNoAssociatedAreas);
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
                Name = nameof(ItemPolicy.Put),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeUpdatePolicy(this IItemUpdatePolicy model)
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

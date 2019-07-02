using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemPolicyExtensions
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this IItemDeletePolicy itemToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (itemToDelete.CompartmentsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Item.CannotDeleteTheItemBecauseItHasAssociatedCompartments);
            }

            if (itemToDelete.ItemListRowsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Item.CannotDeleteTheItemBecauseItHasAssociatedItemListRows);
            }

            if (itemToDelete.MissionOperationsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Item.CannotDeleteTheItemBecauseItHasAssociatedMissions);
            }

            if (itemToDelete.SchedulerRequestsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.Item.CannotDeleteTheItemBecauseItHasAssociatedSchedulerRequests);
            }

            return policy;
        }

        public static Policy ComputeItemCompartmentTypeDeletePolicy(this IItemCompartmentTypeDeletePolicy itemToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (itemToDelete.TotalStock > 0)
            {
                policy.AddErrorMessage(Resources.Item.CannotDeleteTheItemCompartmentTypeBecauseStockIsNotZero);
            }

            if (itemToDelete.TotalReservedForPick > 0)
            {
                policy.AddErrorMessage(Resources.Item.CannotDeleteTheItemCompartmentTypeBecauseTotalReservedForPickIsNotZero);
            }

            if (itemToDelete.TotalReservedToPut > 0)
            {
                policy.AddErrorMessage(Resources.Item.CannotDeleteTheItemCompartmentTypeBecauseTotalReservedToPutIsNotZero);
            }

            return policy;
        }

        public static Policy ComputePickPolicy(this IItemPickPolicy itemToWithdraw)
        {
            var policy = new Policy
            {
                Name = nameof(ItemPolicy.Pick),
                Type = PolicyType.Operation
            };

            if (itemToWithdraw.TotalAvailable.CompareTo(0) == 0)
            {
                policy.AddErrorMessage(Resources.Item.CannotPickTheItemBecauseItHasNoAvailableQuantity);
            }

            return policy;
        }

        public static Policy ComputePutPolicy(this IItemPutPolicy itemToPut)
        {
            var policy = new Policy
            {
                Name = nameof(ItemPolicy.Put),
                Type = PolicyType.Operation
            };

            if (!itemToPut.HasCompartmentTypes)
            {
                policy.AddErrorMessage(Resources.Item.CannotPutTheItemBecauseItHasNoAssociatedCompartmentTypes);
            }

            if (!itemToPut.HasAssociatedAreas)
            {
                policy.AddErrorMessage(Resources.Item.CannotPutTheItemBecauseItHasNoAssociatedAreas);
            }

            return policy;
        }

        public static Policy ComputeUpdatePolicy(this IItemUpdatePolicy model)
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

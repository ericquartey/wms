using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemListProviderPolicies
    {
        #region Methods

        public static Policy ComputeAddRowPolicy(this IPolicyItemList statusItemListModel)
        {
            var policy = new Policy
            {
                Name = nameof(ItemListPolicy.AddRow),
                Type = PolicyType.Operation
            };

            if (statusItemListModel.Status != ItemListStatus.New)
            {
                policy.AddErrorMessage(Resources.ItemList.CannotAddRowsToTheListBecauseItIsNotInTheNewState);
            }

            return policy;
        }

        public static Policy ComputeDeletePolicy(this IItemListDeletePolicy listToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (listToDelete.Status != ItemListStatus.New)
            {
                policy.AddErrorMessage(
                    Resources.ItemList.CannotDeleteTheListBecauseItIsNotInTheNewState);
            }

            if (listToDelete.HasActiveRows)
            {
                policy.AddErrorMessage(
                    Resources.ItemList.CannotDeleteTheListBecauseItHasActiveRows);
            }

            return policy;
        }

        public static Policy ComputeExecutePolicy(this IPolicyItemList listToExecute)
        {
            var policy = new Policy
            {
                Name = nameof(ItemListPolicy.Execute),
                Type = PolicyType.Operation
            };

            if (listToExecute.Status != ItemListStatus.New &&
                listToExecute.Status != ItemListStatus.Error &&
                listToExecute.Status != ItemListStatus.Incomplete &&
                listToExecute.Status != ItemListStatus.Suspended &&
                listToExecute.Status != ItemListStatus.Waiting)
            {
                policy.AddErrorMessage(
                    Resources.ItemList.CannotExecuteTheListBecauseOfItsCurrentState);
            }

            return policy;
        }

        public static Policy ComputeUpdatePolicy(this IPolicyItemList model)
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

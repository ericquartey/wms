using System.Collections.Generic;
using System.Linq;
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
            var errorMessages = new List<string>();
            if (statusItemListModel.Status != ItemListStatus.New)
            {
                errorMessages.Add(
                    Resources.ItemList.CannotAddRowsToTheListBecauseItIsNotInTheNewState);
            }

            string reason = null;
            if (errorMessages.Any())
            {
                reason = string.Join(System.Environment.NewLine, errorMessages);
            }

            return new Policy
            {
                IsAllowed = !errorMessages.Any(),
                Reason = reason,
                Name = nameof(ItemListPolicy.AddRow),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeDeletePolicy(this IItemListDeletePolicy listToDelete)
        {
            var errorMessages = new List<string>();
            if (listToDelete.Status != ItemListStatus.New)
            {
                errorMessages.Add(
                    Resources.ItemList.CannotDeleteTheListBecauseItIsNotInTheNewState);
            }

            if (listToDelete.HasActiveRows)
            {
                errorMessages.Add(
                    Resources.ItemList.CannotDeleteTheListBecauseItHasActiveRows);
            }

            string reason = null;
            if (errorMessages.Any())
            {
                reason = string.Join(System.Environment.NewLine, errorMessages);
            }

            return new Policy
            {
                IsAllowed = !errorMessages.Any(),
                Reason = reason,
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeExecutePolicy(this IPolicyItemList listToExecute)
        {
            var errorMessages = new List<string>();
            if (listToExecute.Status != ItemListStatus.New &&
                listToExecute.Status != ItemListStatus.Error &&
                listToExecute.Status != ItemListStatus.Incomplete &&
                listToExecute.Status != ItemListStatus.Suspended &&
                listToExecute.Status != ItemListStatus.Waiting)
            {
                errorMessages.Add(
                    Resources.ItemList.CannotExecuteTheListBecauseOfItsCurrentState);
            }

            string reason = null;
            if (errorMessages.Any())
            {
                reason = string.Join(System.Environment.NewLine, errorMessages);
            }

            return new Policy
            {
                IsAllowed = !errorMessages.Any(),
                Reason = reason,
                Name = nameof(ItemListPolicy.Execute),
                Type = PolicyType.Operation
            };
        }

        public static Policy ComputeUpdatePolicy(this IPolicyItemList model)
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

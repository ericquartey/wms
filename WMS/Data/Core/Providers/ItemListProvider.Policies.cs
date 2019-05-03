using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListProvider
    {
        #region Methods

        private Policy ComputeAddRowPolicy(BaseModel<int> model)
        {
            if (!(model is IPolicyItemList statusItemListModel))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();
            if (statusItemListModel.Status == ItemListStatus.Completed
                || statusItemListModel.Status == ItemListStatus.Executing)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListStatus}");
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
                Name = "AddRow",
                Type = PolicyType.Operation
            };
        }

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is IItemListDeletePolicy listToDelete))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();
            if (listToDelete.Status != ItemListStatus.New)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListStatus} [{listToDelete.Status.ToString()}]");
            }

            if (listToDelete.HasActiveRows)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRow}");
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
                Name = CommonPolicies.Delete.ToString(),
                Type = PolicyType.Operation
            };
        }

        private Policy ComputeExecutePolicy(BaseModel<int> model)
        {
            if (!(model is IPolicyItemList listToExecute))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();
            if (listToExecute.Status == ItemListStatus.Completed
                || listToExecute.Status == ItemListStatus.Waiting
                || listToExecute.Status == ItemListStatus.Executing)
            {
                errorMessages.Add($"Cannot execute the list because its current status is '{listToExecute.Status.ToString()}'.");
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
                Name = "Execute",
                Type = PolicyType.Operation
            };
        }

        private Policy ComputeUpdatePolicy()
        {
            return new Policy
            {
                IsAllowed = true,
                Reason = null,
                Name = CommonPolicies.Update.ToString(),
                Type = PolicyType.Operation
            };
        }

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeUpdatePolicy());
            model.AddPolicy(this.ComputeDeletePolicy(model));
            model.AddPolicy(this.ComputeExecutePolicy(model));
            model.AddPolicy(this.ComputeAddRowPolicy(model));
        }

        #endregion
    }
}

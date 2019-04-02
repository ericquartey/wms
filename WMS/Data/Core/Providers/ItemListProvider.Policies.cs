using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListProvider
    {
        #region Methods

        private Policy ComputeAddRowPolicy(BaseModel<int> model)
        {
            if (!(model is IStatusItemList statusItemListModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (statusItemListModel.Status != ItemListStatus.Completed)
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
            if (!(model is IStatusItemList statusItemListModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (statusItemListModel.Status == ItemListStatus.Incomplete
                || statusItemListModel.Status == ItemListStatus.Suspended
                || statusItemListModel.Status == ItemListStatus.Waiting)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListStatus} [{statusItemListModel.Status.ToString()}]");
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
            if (!(model is IStatusItemList statusItemListModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (statusItemListModel.Status == ItemListStatus.Incomplete
                || statusItemListModel.Status == ItemListStatus.Suspended
                || statusItemListModel.Status == ItemListStatus.Waiting)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListStatus} [{statusItemListModel.Status.ToString()}]");
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

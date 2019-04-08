using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is IItemDeletePolicy itemToDelete))
            {
                return null;
            }

            var errorMessages = new List<string>();
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
                Name = CommonPolicies.Delete.ToString(),
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

        private Policy ComputeWithdrawPolicy(BaseModel<int> model)
        {
            if (!(model is IItemWithdrawPolicy itemToWithdraw))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (itemToWithdraw.TotalAvailable == 0)
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
                Name = "Withdraw",
                Type = PolicyType.Operation
            };
        }

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeUpdatePolicy());
            model.AddPolicy(this.ComputeDeletePolicy(model));
            model.AddPolicy(this.ComputeWithdrawPolicy(model));
        }

        #endregion
    }
}

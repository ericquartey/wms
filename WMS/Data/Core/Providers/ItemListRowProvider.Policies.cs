using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListRowProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is IItemListRowDeletePolicy rowToDelete))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

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
                Name = CommonPolicies.Delete.ToString(),
                Type = PolicyType.Operation
            };
        }

        private Policy ComputeExecutePolicy(BaseModel<int> model)
        {
            if (!(model is IItemListRowExecutePolicy rowToExecute))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();

            if (rowToExecute.Status == ItemListRowStatus.Incomplete
                || rowToExecute.Status == ItemListRowStatus.Suspended
                || rowToExecute.Status == ItemListRowStatus.Executing
                || rowToExecute.Status == ItemListRowStatus.Waiting)
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
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemProvider
    {
        #region Methods

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
            if (!(model is IAvailabilityItem availabilityItemModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (availabilityItemModel.TotalAvailable == 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemAvailable} [{availabilityItemModel.TotalAvailable}]");
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

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is ICountersItem countersItemModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (countersItemModel.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{countersItemModel.CompartmentsCount}]");
            }

            if (countersItemModel.ItemListRowsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRow} [{countersItemModel.ItemListRowsCount}]");
            }

            if (countersItemModel.MissionsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{countersItemModel.MissionsCount}]");
            }

            if (countersItemModel.SchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{countersItemModel.SchedulerRequestsCount}]");
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

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeUpdatePolicy());
            model.AddPolicy(this.ComputeDeletePolicy(model));
            model.AddPolicy(this.ComputeWithdrawPolicy(model));
        }

        #endregion
    }
}

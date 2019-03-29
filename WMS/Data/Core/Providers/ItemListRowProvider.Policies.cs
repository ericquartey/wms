using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListRowProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is IStatusItemListRow statusItemListRowModel))
            {
                return null;
            }

            if (!(model is ICountersItemListRow countersItemListRowModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (countersItemListRowModel.SchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{countersItemListRowModel.SchedulerRequestsCount}]");
            }

            if (statusItemListRowModel.Status != ItemListRowStatus.Waiting)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRowStatus} [{statusItemListRowModel.Status.ToString()}]");
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
            if (!(model is IStatusItemListRow statusItemListRowModel))
            {
                return null;
            }

            var errorMessages = new List<string>();

            if (statusItemListRowModel.Status == ItemListRowStatus.Incomplete
                || statusItemListRowModel.Status == ItemListRowStatus.Suspended
                || statusItemListRowModel.Status == ItemListRowStatus.Waiting)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemListRowStatus} [{statusItemListRowModel.Status.ToString()}]");
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

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeUpdatePolicy());
            model.AddPolicy(this.ComputeDeletePolicy(model));
            model.AddPolicy(this.ComputeExecutePolicy(model));
        }

        #endregion
    }
}

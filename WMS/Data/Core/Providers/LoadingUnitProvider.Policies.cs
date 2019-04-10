using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class LoadingUnitProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is ILoadingUnitDeletePolicy loadingUnitToDelete))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (loadingUnitToDelete.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{loadingUnitToDelete.CompartmentsCount}]");
            }

            if (loadingUnitToDelete.ActiveMissionsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{loadingUnitToDelete.ActiveMissionsCount}]");
            }

            if (loadingUnitToDelete.ActiveSchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{loadingUnitToDelete.ActiveSchedulerRequestsCount}]");
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
            if (model == null)
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();
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

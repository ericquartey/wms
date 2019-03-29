using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class LoadingUnitProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is ICountersLoadingUnit countersLoadingUnitModel))
            {
                return null;
            }

            var errorMessages = new List<string>();
            if (countersLoadingUnitModel.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{countersLoadingUnitModel.CompartmentsCount}]");
            }

            if (countersLoadingUnitModel.MissionsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Mission} [{countersLoadingUnitModel.MissionsCount}]");
            }

            if (countersLoadingUnitModel.SchedulerRequestsCount > 0)
            {
                errorMessages.Add(
                    $"{Common.Resources.BusinessObjects.SchedulerRequest} [{countersLoadingUnitModel.SchedulerRequestsCount}]");
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
        }

        #endregion
    }
}

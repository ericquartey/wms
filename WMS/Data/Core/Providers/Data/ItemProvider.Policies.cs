using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemProvider
    {
        #region Fields

        private string errorArgument = "Method was called with incompatible type argument.";

        #endregion

        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is IItemDeletePolicy itemToDelete))
            {
                throw new System.InvalidOperationException(this.errorArgument);
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
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };
        }

        private Policy ComputePickPolicy(BaseModel<int> model)
        {
            if (!(model is IItemPickPolicy itemToWithdraw))
            {
                throw new InvalidOperationException(this.errorArgument);
            }

            var errorMessages = new List<string>();
            if (itemToWithdraw.TotalAvailable.CompareTo(0) == 0)
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
                Name = nameof(ItemPolicy.Pick),
                Type = PolicyType.Operation
            };
        }

        private Policy ComputePutPolicy(BaseModel<int> model)
        {
            var errorMessages = new List<string>();

            if (!(model is IItemPutPolicy itemToPut))
            {
                throw new System.InvalidOperationException(this.errorArgument);
            }

            if (!itemToPut.HasCompartmentTypes)
            {
                errorMessages.Add(Common.Resources.Errors.PutItemNoCompartmentType);
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
                Name = nameof(ItemPolicy.Put),
                Type = PolicyType.Operation
            };
        }

        private Policy ComputeUpdatePolicy()
        {
            return new Policy
            {
                IsAllowed = true,
                Reason = null,
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeUpdatePolicy());
            model.AddPolicy(this.ComputeDeletePolicy(model));
            model.AddPolicy(this.ComputePickPolicy(model));
            model.AddPolicy(this.ComputePutPolicy(model));
        }

        #endregion
    }
}

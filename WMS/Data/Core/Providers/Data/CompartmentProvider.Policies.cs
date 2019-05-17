using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class CompartmentProvider
    {
        #region Methods

        private Policy ComputeDeletePolicy(BaseModel<int> model)
        {
            if (!(model is ICompartmentDeletePolicy compartmentToDelete))
            {
                throw new System.InvalidOperationException("Method was called with incompatible type argument.");
            }

            var errorMessages = new List<string>();
            if (compartmentToDelete.Stock > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemStock} [{compartmentToDelete.Stock}]");
            }

            if (compartmentToDelete.IsItemPairingFixed)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.PairingFixed}");
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
        }

        #endregion
    }
}

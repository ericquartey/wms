using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class CompartmentTypeProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this ICompartmentTypeDeletePolicy compartmentTypeToDelete)
        {
            var errorMessages = new List<string>();
            if (compartmentTypeToDelete == null)
            {
                return null;
            }

            if (compartmentTypeToDelete.CompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.Compartment} [{compartmentTypeToDelete.CompartmentsCount}]");
            }

            if (compartmentTypeToDelete.ItemCompartmentsCount > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.ItemCompartment} [{compartmentTypeToDelete.ItemCompartmentsCount}]");
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

        #endregion
    }
}

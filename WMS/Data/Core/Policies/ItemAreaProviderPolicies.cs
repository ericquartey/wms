using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemAreaProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this IItemAreaDeletePolicy itemAreaToDelete)
        {
            var errorMessages = new List<string>();
            if (itemAreaToDelete.IsItemInArea)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.CompartmentsInAreaStillAssociatedToItem}]");
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

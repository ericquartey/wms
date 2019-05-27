using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class AreaProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeleteItemsAreaPolicy(this IAreaDeleteItemArea itemAreaToDelete)
        {
            var errorMessages = new List<string>();
            if (itemAreaToDelete.TotalStock > 0)
            {
                errorMessages.Add($"{Common.Resources.BusinessObjects.AllowedItemAreaTotalStock} [{itemAreaToDelete.TotalStock}]");
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
                Name = nameof(AreaPolicy.DeleteItemArea),
                Type = PolicyType.Operation
            };
        }

        #endregion
    }
}

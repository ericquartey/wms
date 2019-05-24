using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class AreaProvider
    {
        #region Fields

        private string errorArgument = "Method was called with incompatible type argument.";

        #endregion

        #region Methods

        private Policy ComputeDeleteItemsAreaPolicy(BaseModel<int> model)
        {
            if (!(model is IAreaDeleteItemArea itemAreaToDelete))
            {
                throw new System.InvalidOperationException(this.errorArgument);
            }

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

        private void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy(this.ComputeDeleteItemsAreaPolicy(model));
        }

        #endregion
    }
}

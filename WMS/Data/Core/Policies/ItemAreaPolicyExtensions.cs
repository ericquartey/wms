using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class ItemAreaPolicyExtensions
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this IItemAreaDeletePolicy itemAreaToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (itemAreaToDelete.IsItemInArea)
            {
                policy.AddErrorMessage(
                    Resources.ItemArea.CannotDeleteItemAreaBecauseCompartmentsInAreaStillAssociatedToItem);
            }

            return policy;
        }

        #endregion
    }
}

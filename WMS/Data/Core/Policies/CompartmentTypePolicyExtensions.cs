using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class CompartmentTypePolicyExtensions
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this ICompartmentTypeDeletePolicy compartmentTypeToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation,
            };

            if (compartmentTypeToDelete.CompartmentsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.CompartmentType.CannotDeleteTheCompartmentTypeBecauseItHasAssociatedCompartments);
            }

            if (compartmentTypeToDelete.ItemCompartmentsCount > 0)
            {
                policy.AddErrorMessage(
                    Resources.CompartmentType.CannotDeleteTheCompartmentTypeBecauseItHasAssociatedItems);
            }

            return policy;
        }

        #endregion
    }
}

using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class CompartmentProviderPolicies
    {
        #region Methods

        public static Policy ComputeDeletePolicy(this ICompartmentDeletePolicy compartmentToDelete)
        {
            var policy = new Policy
            {
                Name = nameof(CrudPolicies.Delete),
                Type = PolicyType.Operation
            };

            if (compartmentToDelete.Stock > 0)
            {
                policy.AddErrorMessage(Resources.Compartment.CannotDeleteTheCompartmentBecauseStockIsNotZero);
            }

            if (compartmentToDelete.IsItemPairingFixed)
            {
                policy.AddErrorMessage(Resources.Compartment.CannotDeleteTheCompartmentBecausePairingIsFixed);
            }

            return policy;
        }

        public static Policy ComputeUpdatePolicy(this ICompartmentUpdatePolicy model)
        {
            return new Policy
            {
                Name = nameof(CrudPolicies.Update),
                Type = PolicyType.Operation
            };
        }

        #endregion
    }
}

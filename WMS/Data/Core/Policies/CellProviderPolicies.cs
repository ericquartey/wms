using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Policies
{
    internal static class CellProviderPolicies
    {
        #region Methods

        public static Policy ComputeUpdatePolicy(this ICellUpdatePolicy model)
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

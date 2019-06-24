using System.Collections.Generic;
using System.Linq;
using Ferretto.WMS.App.Resources;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Policy = Ferretto.WMS.App.Core.Models.Policy;
using PolicyType = Ferretto.Common.BLL.Interfaces.Models.PolicyType;

namespace Ferretto.WMS.App.Core.Extensions
{
    public static class DtoExtensions
    {
        #region Methods

        public static IEnumerable<Policy> GetPolicies(this BaseModelOfInt32 dto)
        {
            return dto?.Policies.Select(p =>
                new Policy
                {
                    IsAllowed = p.IsAllowed,
                    Name = p.Name,
                    Reason = string.IsNullOrEmpty(p.Reason) && !p.IsAllowed
                        ? DesktopApp.GenericDenyPolicy
                        : p.Reason,
                    Type = (PolicyType)p.Type
                });
        }

        #endregion
    }
}

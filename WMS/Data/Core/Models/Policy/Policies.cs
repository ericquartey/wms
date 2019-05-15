using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Flags]
    public enum Policies
    {
        None = 0,

        AddRow = 1,

        Execute = 2,

        Withdraw = 3,

        Create = CommonPolicies.Create,

        Update = CommonPolicies.Update,

        Delete = CommonPolicies.Delete,
    }
}

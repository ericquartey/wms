using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Flags]
    public enum Policies
    {
        None = 0,

        AddRow = 4,

        Execute = 5,

        Withdraw = 6,

        Create = CommonPolicies.Create,

        Update = CommonPolicies.Update,

        Delete = CommonPolicies.Delete,
    }
}

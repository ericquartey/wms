using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IPolicyDescriptor<out TPolicy>
        where TPolicy : IPolicy
    {
        #region Properties

        IEnumerable<TPolicy> Policies { get; }

        #endregion
    }
}

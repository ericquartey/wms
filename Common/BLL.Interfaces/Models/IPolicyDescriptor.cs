using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IPolicyDescriptor
    {
        #region Properties

        IEnumerable<IPolicy> Policies { get; }

        #endregion
    }
}

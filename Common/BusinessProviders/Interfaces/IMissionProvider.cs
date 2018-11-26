using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMissionProvider : IBusinessProvider<Mission, Mission>
    {
        #region Methods

        Task<int> AddRange(IEnumerable<Mission> missions);

        #endregion Methods
    }
}

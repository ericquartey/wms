using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBayProvider : IReadAllAsyncProvider<Bay, int>,
        IReadSingleAsyncProvider<Bay, int>
    {
        #region Methods

        Task<IEnumerable<Bay>> GetByAreaIdAsync(int id);

        #endregion
    }
}

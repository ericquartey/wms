using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IBayProvider :
        IReadAllAsyncProvider<Bay, int>,
        IReadSingleAsyncProvider<Bay, int>
    {
        #region Methods

        Task<IEnumerable<Bay>> GetByAreaIdAsync(int id);

        #endregion
    }
}

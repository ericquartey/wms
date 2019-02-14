using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IBayProvider :
        IReadAllAsyncProvider<Bay, int>,
        IReadSingleAsyncProvider<Bay, int>
    {
        #region Methods

        Task<OperationResult<Bay>> ActivateAsync(int id);

        Task<OperationResult<Bay>> DeactivateAsync(int id);

        Task<IEnumerable<Bay>> GetByAreaIdAsync(int id);

        #endregion
    }
}

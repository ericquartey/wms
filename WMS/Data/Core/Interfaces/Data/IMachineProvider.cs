using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMachineProvider :
        IReadAllPagedAsyncProvider<Machine, int>,
        IReadSingleAsyncProvider<MachineDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IOperationResult<IEnumerable<MachineServiceInfo>>> GetAllMachinesServiceInfoAsync();

        Task<Machine> GetByBayIdAsync(int bayId);

        #endregion
    }
}

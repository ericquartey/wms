using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellProvider :
        IPagedBusinessProvider<Cell>,
        IReadSingleAsyncProvider<CellDetails, int>,
        IUpdateAsyncProvider<CellDetails>
    {
        #region Methods

        Task<IEnumerable<Enumeration>> GetByAisleIdAsync(int aisleId);

        Task<IEnumerable<Enumeration>> GetByAreaIdAsync(int areaId);

        #endregion
    }
}

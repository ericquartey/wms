using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IAisleProvider :
        IReadAllAsyncProvider<Aisle>,
        IReadSingleAsyncProvider<Aisle, int>
    {
        #region Methods

        Task<IEnumerable<Aisle>> GetAislesByAreaIdAsync(int areaId);

        #endregion
    }
}

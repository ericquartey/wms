using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitProvider :
        IPagedBusinessProvider<LoadingUnit>,
        IReadSingleAsyncProvider<LoadingUnitDetails, int>,
        ICreateAsyncProvider<LoadingUnitDetails>,
        IUpdateAsyncProvider<LoadingUnitDetails>
    {
        #region Methods

        Task<IEnumerable<LoadingUnitDetails>> GetByCellIdAsync(int id);

        #endregion
    }
}

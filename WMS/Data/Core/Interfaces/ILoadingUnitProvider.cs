using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ILoadingUnitProvider :
        ICreateAsyncProvider<LoadingUnitDetails, int>,
        IReadAllPagedAsyncProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnitDetails, int>,
        IUpdateAsyncProvider<LoadingUnitDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IEnumerable<LoadingUnitDetails>> GetByCellIdAsync(int id);

        #endregion
    }
}

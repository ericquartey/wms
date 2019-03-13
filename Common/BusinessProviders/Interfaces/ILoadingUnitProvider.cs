using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitProvider :
        IPagedBusinessProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnitDetails, int>,
        ICreateAsyncProvider<LoadingUnitDetails, int>,
        IUpdateAsyncProvider<LoadingUnitDetails, int>
    {
        #region Methods

        Task<IEnumerable<LoadingUnitDetails>> GetByCellIdAsync(int id);

        Task<LoadingUnitDetails> GetNewAsync();

        #endregion
    }
}

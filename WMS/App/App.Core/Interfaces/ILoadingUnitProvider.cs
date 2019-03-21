using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
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

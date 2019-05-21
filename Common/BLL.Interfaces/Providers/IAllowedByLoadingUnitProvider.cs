using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IAllowedByLoadingUnitProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAllowedByLoadingUnitIdAsync(
                int loadingUnitId,
                int skip,
                int take,
                IEnumerable<SortOption> orderBySortOptions = null);

        Task<int> GetAllAllowedByLoadingUnitIdCountAsync(int loadingUnitId);

        #endregion
    }
}

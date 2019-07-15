using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemProvider :
        ICreateAsyncProvider<ItemDetails, int>,
        IReadAllPagedAsyncProvider<Item, int>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemAvailable, int>,
        IGetUniqueValuesAsyncProvider,
        IDeleteAsyncProvider<ItemDetails, int>
    {
        #region Methods

        Task<IEnumerable<Item>> GetAllAllowedByCompartmentTypeIdAsync(
            int compartmentTypeId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null);

        Task<IEnumerable<Item>> GetAllAllowedByLoadingUnitIdAsync(
                int loadingUnitId,
                int skip,
                int take,
                IEnumerable<SortOption> orderBySortOptions = null);

        Task<int> GetAllAllowedByLoadingUnitIdCountAsync(int loadingUnitId);

        Task<IEnumerable<ItemWithCompartmentTypeInfo>> GetAllAssociatedByCompartmentTypeIdAsync(int compartmentTypeId);

        Task<IEnumerable<Item>> GetAllByAreaIdAsync(
            int areaId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null);

        Task<ItemAvailable> GetByIdForExecutionAsync(int id);

        #endregion
    }
}

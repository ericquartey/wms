using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IItemProvider :
        IPagedBusinessProvider<Item, int>,
        ICreateAsyncProvider<ItemDetails, int>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails, int>,
        IDeleteAsyncProvider<ItemDetails, int>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<Item>>> GetAllAllowedByLoadingUnitIdAsync(
        int loadingUnitId,
        int skip,
        int take,
        IEnumerable<SortOption> orderBySortOptions = null);

        Task<IOperationResult<int>> GetAllAllowedByLoadingUnitIdCountAsync(int loadingUnitId);

        Task<IOperationResult<IEnumerable<AssociateItemWithCompartmentType>>> GetAllAssociatedByCompartmentTypeIdAsync(int compartmentTypeId);

        Task<IOperationResult<IEnumerable<AllowedItemInCompartment>>> GetAllowedByCompartmentIdAsync(int compartmentId);

        Task<IOperationResult<ItemDetails>> GetNewAsync();

        Task<IOperationResult<double>> GetPickAvailabilityAsync(ItemPick itemPick, CancellationToken cancellationToken = default(CancellationToken));

        Task<IOperationResult<double>> GetPutCapacityAsync(ItemPut itemPut, CancellationToken cancellationToken = default(CancellationToken));

        Task<IOperationResult<SchedulerRequest>> PickAsync(ItemPick itemPick);

        Task<IOperationResult<SchedulerRequest>> PutAsync(ItemPut itemPut);

        #endregion
    }
}

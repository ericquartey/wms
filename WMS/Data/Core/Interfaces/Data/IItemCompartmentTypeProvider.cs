using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemCompartmentTypeProvider :
        ICreateAsyncProvider<ItemCompartmentType, (int, int)>,
        IUpdateAsyncProvider<ItemCompartmentType, (int, int)>,
        IDeleteAsyncProvider<ItemCompartmentType, (int ItemId, int CompartmentTypeId)>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<ItemCompartmentType>>> CreateItemCompartmentTypesRangeByItemIdAsync(IEnumerable<ItemCompartmentType> itemCompartmentTypes);

        Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllByItemIdAsync(int id);

        Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllUnassociatedByItemIdAsync(int id);

        #endregion
    }
}

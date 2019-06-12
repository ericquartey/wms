using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemCompartmentTypeProvider :
        ICreateAsyncProvider<ItemCompartmentType, int>,
        IUpdateAsyncProvider<ItemCompartmentType, int>
    {
        #region Methods

        Task<IOperationResult<ItemCompartmentType>> DeleteAsync(int itemId, int compartmentTypeId);

        Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllByItemIdAsync(int id);

        #endregion
    }
}

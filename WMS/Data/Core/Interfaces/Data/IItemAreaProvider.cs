using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemAreaProvider
    {
        #region Methods

        Task<IOperationResult<ItemArea>> CreateAsync(ItemArea model);

        Task<IOperationResult<ItemArea>> DeleteAsync(int id, int itemId);

        Task<IEnumerable<AllowedItemArea>> GetByItemIdAsync(int id);

        #endregion
    }
}

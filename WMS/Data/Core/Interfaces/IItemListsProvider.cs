using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListsProvider
    {
        #region Methods

        Task<IEnumerable<ItemList>> GetAllAsync();

        Task<ItemList> GetByIdAsync(int id);

        #endregion Methods
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public interface IWarehouse
    {
        #region Properties

        IEnumerable<Item> Items { get; }

        IEnumerable<ItemList> Lists { get; }

        IEnumerable<Mission> Missions { get; }

        #endregion

        #region Methods

        Task<Item> UpdateAsync(Item item);

        #endregion
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public interface IWarehouse
    {
        #region Properties

        IEnumerable<AbcClass> AbcClasses { get; }

        IEnumerable<Area> Areas { get; }

        IEnumerable<ItemCategory> ItemCategories { get; }

        IEnumerable<Item> Items { get; }

        IEnumerable<ItemList> Lists { get; }

        IEnumerable<Mission> Missions { get; }

        #endregion Properties

        #region Methods

        Task<Item> UpdateAsync(Item item);

        #endregion Methods
    }
}

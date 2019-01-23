using System.Collections.Generic;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public interface IWarehouse
    {
        #region Properties

        IEnumerable<Item> Items { get; }

        IEnumerable<ItemList> Lists { get; }

        IEnumerable<Mission> Missions { get; }

        #endregion Properties
    }
}

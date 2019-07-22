using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.WmsCommunication.Interfaces
{
    public interface IWmsDataProvider
    {
        #region Methods

        Task<string> GetItemImageCodeAsync(int itemId);

        Task<ObservableCollection<ItemList>> GetItemLists();

        Task<ObservableCollection<Item>> GetItemsAsync(string searchCode, int skip, int quantity);

        Task<ObservableCollection<ItemListRow>> GetListRowsAsync(string listCode);

        Task<ObservableCollection<TrayControlCompartment>> GetTrayControlCompartmentsAsync(MissionInfo mission);

        Task<bool> PickAsync(int itemId, int areaId, int bayId, int requestedQuantity);

        #endregion
    }
}

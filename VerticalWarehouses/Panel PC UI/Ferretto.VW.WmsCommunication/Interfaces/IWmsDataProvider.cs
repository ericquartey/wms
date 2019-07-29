using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.WmsCommunication.Interfaces
{
    public interface IWmsDataProvider
    {
        #region Methods

        Task<string> GetCompartmentPosition(Mission mission);

        Task<DrawerActivityItemDetail> GetDrawerActivityItemDetailAsync(Mission mission);

        Task<string> GetItemImageCodeAsync(int itemId);

        Task<IList<ItemList>> GetItemLists(int areaId);

        Task<ObservableCollection<Item>> GetItemsAsync(int areaid, string searchCode, int skip, int take, CancellationToken cancellationToken);

        Task<IList<ItemListRow>> GetListRowsAsync(int listId);

        Task<ObservableCollection<TrayControlCompartment>> GetTrayControlCompartmentsAsync(Mission mission);

        TrayControlCompartment GetTrayControlSelectedCompartment(IEnumerable<TrayControlCompartment> viewCompartments, Mission mission);

        Task ItemListExecute(int listId, int areaId);

        Task<bool> PickAsync(int itemId, int areaId, int bayId, int requestedQuantity);

        #endregion
    }
}

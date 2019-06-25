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

        Task<string> GetCompartmentPosition(Mission mission);

        Task<DrawerActivityItemDetail> GetDrawerActivityItemDetailAsync(Mission mission);

        Task<string> GetItemImageCodeAsync(int itemId);

        Task<ObservableCollection<Item>> GetItemsAsync(string searchCode, int skip, int quantity);

        Task<ObservableCollection<TrayControlCompartment>> GetTrayControlCompartmentsAsync(Mission mission);

        Task<TrayControlCompartment> GetTrayControlSelectedCompartment(ObservableCollection<TrayControlCompartment> viewCompartments, Mission mission);

        Task<bool> PickAsync(int itemId, int areaId, int bayId, int requestedQuantity);

        #endregion
    }
}

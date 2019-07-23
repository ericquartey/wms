using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IItemListsDataService itemListsDataService;

        private readonly IItemsDataService itemsDataService;

        private readonly ILoadingUnitsDataService loadingUnitsDataService;

        private readonly IStatusMessageService statusMessageService;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IStatusMessageService statusMessageService,
            ILoadingUnitsDataService loadingUnitsDataService,
            IItemsDataService itemsDataService,
            IItemListsDataService itemListsDataService)
        {
            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (loadingUnitsDataService == null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsDataService));
            }

            if (itemsDataService == null)
            {
                throw new ArgumentNullException(nameof(itemsDataService));
            }

            if (itemListsDataService == null)
            {
                throw new ArgumentNullException(nameof(itemListsDataService));
            }

            this.statusMessageService = statusMessageService;
            this.loadingUnitsDataService = loadingUnitsDataService;
            this.itemsDataService = itemsDataService;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<string> GetItemImageCodeAsync(int itemId)
        {
            ItemDetails item = null;
            try
            {
                item = await this.itemsDataService.GetByIdAsync(itemId);
            }
            catch (SwaggerException ex)
            {
                this.statusMessageService.Notify(ex);
            }
            return item.Image;
        }

        public async Task<IEnumerable<ItemList>> GetItemListsAsync()
        {
            try
            {
                return await this.itemListsDataService.GetAllAsync(take: 10);
            }
            catch (SwaggerException ex)
            {
                this.statusMessageService.Notify(ex);
                return new List<ItemList>();
            }
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(string searchCode, int skip, int quantity)
        {
            try
            {
                return await this.itemsDataService.GetAllAsync(search: searchCode, skip: skip, take: quantity);
            }
            catch (SwaggerException ex)
            {
                this.statusMessageService.Notify(ex);

                return new List<Item>();
            }
        }

        public async Task<IEnumerable<ItemListRow>> GetListRowsAsync(int listId)
        {
            try
            {
                return await this.itemListsDataService.GetRowsAsync(listId);
            }
            catch (Exception ex)
            {
                this.statusMessageService.Notify(ex);
                return new List<ItemListRow>();
            }
        }

        public async Task<IEnumerable<TrayControlCompartment>> GetTrayControlCompartmentsAsync(MissionInfo mission)
        {
            var returnValue = new ObservableCollection<TrayControlCompartment>();
            var loadingUnitId = mission.LoadingUnitId;
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync(loadingUnitId);
            if (compartments != null && compartments.Count > 0)
            {
                returnValue = new ObservableCollection<TrayControlCompartment>(compartments.Select(x => new TrayControlCompartment
                {
                    Depth = x.Depth,
                    Id = x.Id,
                    LoadingUnitId = x.LoadingUnitId,
                    Width = x.Width,
                    XPosition = x.XPosition,
                    YPosition = x.YPosition
                }));
            }
            return returnValue;
        }

        public async Task<bool> PickAsync(int itemId, int areaId, int bayId, int requestedQuantity)
        {
            // HACK BUG 3381 WORKAROUND - DEVELOPMENT ONLY
            if (bayId == 0 || areaId == 0)
            {
                areaId = 2;
                bayId = 2;
            }
            // END HACK
            try
            {
                await this.itemsDataService.PickAsync(itemId, new ItemOptions
                {
                    AreaId = areaId,
                    BayId = bayId,
                    RequestedQuantity = requestedQuantity,
                    RunImmediately = true
                });

                return true;
            }
            catch (Exception ex)
            {
                this.statusMessageService.Notify(ex);
                return false;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.WmsCommunication
{
    public class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IItemListRowsDataService itemListRowsDataService;

        private readonly IItemListsDataService itemListsDataService;

        private readonly IItemsDataService itemsDataService;

        private readonly ILoadingUnitsDataService loadingUnitsDataService;

        private readonly IMaterialStatusesDataService materialStatusesDataService;

        private readonly IPackageTypesDataService packageTypesDataService;

        #endregion

        #region Constructors

        public WmsDataProvider(Uri wmsConnectionString)
        {
            this.itemsDataService = DataServiceFactory.GetService<IItemsDataService>(wmsConnectionString);
            this.loadingUnitsDataService = DataServiceFactory.GetService<ILoadingUnitsDataService>(wmsConnectionString);
            this.materialStatusesDataService = DataServiceFactory.GetService<IMaterialStatusesDataService>(wmsConnectionString);
            this.packageTypesDataService = DataServiceFactory.GetService<IPackageTypesDataService>(wmsConnectionString);
            this.itemListsDataService = DataServiceFactory.GetService<IItemListsDataService>(wmsConnectionString);
            this.itemListRowsDataService = DataServiceFactory.GetService<IItemListRowsDataService>(wmsConnectionString);
        }

        #endregion

        #region Methods

        public async Task<string> GetCompartmentPosition(Mission mission)
        {
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync((int)mission.LoadingUnitId);
            var compartment = compartments.First(x => x.Id == mission.CompartmentId);
            var compartmentXpos = compartment.XPosition;
            var compartmentYpos = compartment.YPosition;
            return $"{compartmentXpos}, {compartmentYpos}";
        }

        public async Task<DrawerActivityItemDetail> GetDrawerActivityItemDetailAsync(Mission mission)
        {
            var item = await this.itemsDataService.GetByIdAsync((int)mission.ItemId);
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync((int)mission.LoadingUnitId);
            var compartment = compartments.First(x => x.Id == mission.CompartmentId);
            var materialStatus = await this.materialStatusesDataService.GetByIdAsync((int)compartment.MaterialStatusId);
            var packageType = await this.packageTypesDataService.GetByIdAsync((int)compartment.PackageTypeId);
            var returnValue = new DrawerActivityItemDetail
            {
                Batch = compartment.Lot,
                ItemCode = item.Code,
                ItemDescription = item.Description,
                ListCode = mission.ItemListRowCode,
                ListDescription = mission.ItemListDescription,
                ListRow = mission.ItemListRowId.ToString(),
                MaterialStatus = materialStatus.Description,
                PackageType = packageType.Description,
                Position = $"{compartment.XPosition}, {compartment.YPosition}",
                ProductionDate = item.CreationDate.ToShortDateString(),
                RequestedQuantity = mission.RequestedQuantity.ToString(),
                Image = item.Image
            };
            return returnValue;
        }

        public async Task<string> GetItemImageCodeAsync(int itemId)
        {
            ItemDetails item = null;
            try
            {
                item = await this.itemsDataService.GetByIdAsync(itemId);
            }
            catch (SwaggerException ex)
            {
                throw new NotImplementedException(ex.Message);
            }
            return item.Image;
        }

        public async Task<ObservableCollection<ItemList>> GetItemLists()
        {
            try
            {
                return await this.itemListsDataService.GetAllAsync(take: 10);
            }
            catch (SwaggerException ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }

        public async Task<ObservableCollection<Item>> GetItemsAsync(string searchCode, int skip, int quantity)
        {
            var items = await this.itemsDataService.GetAllAsync(search: searchCode, skip: skip, take: quantity);
            return items;
        }

        public async Task<ObservableCollection<ItemListRow>> GetListRowsAsync(string listCode)
        {
            var lists = new ObservableCollection<ItemListRow>();
            try
            {
                var searchCode = listCode.Trim('-');
                lists = await this.itemListRowsDataService.GetAllAsync(search: searchCode);
            }
            catch (Exception ex)
            {
                throw new Exception("WMS Data Provider - " + ex.Message);
            }
            return lists;
        }

        public async Task<ObservableCollection<TrayControlCompartment>> GetTrayControlCompartmentsAsync(Mission mission)
        {
            var returnValue = new ObservableCollection<TrayControlCompartment>();
            var loadingUnitId = (int)mission.LoadingUnitId;
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

        public TrayControlCompartment GetTrayControlSelectedCompartment(
            IEnumerable<TrayControlCompartment> viewCompartments,
            Mission mission)
        {
            var compartmentId = (int)mission.CompartmentId;
            return viewCompartments.First(x => x.Id == compartmentId);
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
            catch (SwaggerException)
            {
                return false;
            }
        }

        #endregion
    }
}

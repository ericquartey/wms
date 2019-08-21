using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IItemsDataService itemsDataService;

        private readonly ILoadingUnitsDataService loadingUnitsDataService;

        private readonly IStatusMessageService statusMessageService;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IStatusMessageService statusMessageService,
            ILoadingUnitsDataService loadingUnitsDataService,
            IItemsDataService itemsDataService)
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

            this.statusMessageService = statusMessageService;

            this.loadingUnitsDataService = loadingUnitsDataService;
            this.itemsDataService = itemsDataService;
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

        public async Task<IEnumerable<TrayControlCompartment>> GetTrayControlCompartmentsAsync(MissionInfo mission)
        {
            var returnValue = new ObservableCollection<TrayControlCompartment>();
            var loadingUnitId = mission.LoadingUnitId;
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync(loadingUnitId);
            if (compartments != null && compartments.Count > 0)
            {
                returnValue = new ObservableCollection<TrayControlCompartment>(compartments.Select(x =>
                    new TrayControlCompartment
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

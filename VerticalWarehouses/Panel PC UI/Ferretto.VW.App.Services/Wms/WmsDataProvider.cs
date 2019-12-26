using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IItemsWmsWebService itemsWmsWebService;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IBayManager bayManager,
            IItemsWmsWebService itemsWmsWebService)
        {
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.itemsWmsWebService = itemsWmsWebService ?? throw new System.ArgumentNullException(nameof(itemsWmsWebService));
        }

        #endregion

        #region Methods

        public async Task<string> GetItemImagePathAsync(int itemId)
        {
            try
            {
                var item = await this.itemsWmsWebService.GetByIdAsync(itemId);
                return item.Image;
            }
            catch
            {
                return null;
            }
        }

        public async Task PickAsync(int itemId, int requestedQuantity)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                return;
            }

            var bay = await this.bayManager.GetBayAsync();

            await this.itemsWmsWebService.PickAsync(itemId, new ItemOptions
            {
                AreaId = this.bayManager.Identity.AreaId.Value,
                BayId = bay.Id,
                MachineId = this.bayManager.Identity.Id,
                RequestedQuantity = requestedQuantity,
                RunImmediately = true
            });
        }

        #endregion
    }
}

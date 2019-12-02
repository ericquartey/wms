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

        private readonly IItemsDataService itemsDataService;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IBayManager bayManager,
            IItemsDataService itemsDataService)
        {
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.itemsDataService = itemsDataService ?? throw new System.ArgumentNullException(nameof(itemsDataService));
        }

        #endregion

        #region Methods

        public async Task<string> GetItemImagePathAsync(int itemId)
        {
            try
            {
                var item = await this.itemsDataService.GetByIdAsync(itemId);
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

            await this.itemsDataService.PickAsync(itemId, new ItemOptions
            {
                AreaId = this.bayManager.Identity.AreaId.Value,
                BayId = (int)ConfigurationManager.AppSettings.GetBayNumber(),
                RequestedQuantity = requestedQuantity,
                RunImmediately = true
            });
        }

        #endregion
    }
}

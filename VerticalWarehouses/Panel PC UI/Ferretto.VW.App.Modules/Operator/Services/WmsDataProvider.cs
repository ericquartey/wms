using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator
{
    internal sealed class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineItemsWebService itemWebService;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IBayManager bayManager,
            IMachineItemsWebService itemWebService)
        {
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.itemWebService = itemWebService ?? throw new System.ArgumentNullException(nameof(itemWebService));
        }

        #endregion

        #region Methods

        public async Task<string> GetItemImagePathAsync(int itemId)
        {
            try
            {
                var item = await this.itemWebService.GetByIdAsync(itemId);
                return item.Image;
            }
            catch
            {
                return null;
            }
        }

        public async Task PickAsync(int itemId, double requestedQuantity)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                return;
            }

            var bay = await this.bayManager.GetBayAsync();

            await this.itemWebService.PickAsync(itemId, new ItemOptions
            {
                AreaId = this.bayManager.Identity.AreaId.Value,
                BayId = bay.Id,
                MachineId = this.bayManager.Identity.Id,
                RequestedQuantity = requestedQuantity,
                RunImmediately = true
            });
        }

        public async Task PutAsync(int itemId, double requestedQuantity)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                return;
            }

            var bay = await this.bayManager.GetBayAsync();

            await this.itemWebService.PutAsync(itemId, new ItemOptions
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

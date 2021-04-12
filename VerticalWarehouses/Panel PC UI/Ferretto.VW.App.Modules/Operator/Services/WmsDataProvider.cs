using System;
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

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private bool isEnabled;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IBayManager bayManager,
            IMachineItemsWebService itemWebService,
            IMachineWmsStatusWebService wmsStatusWebService)
        {
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.itemWebService = itemWebService ?? throw new System.ArgumentNullException(nameof(itemWebService));
            this.wmsStatusWebService = wmsStatusWebService ?? throw new System.ArgumentNullException(nameof(wmsStatusWebService));
        }

        #endregion

        #region Properties

        public bool IsEnabled => this.isEnabled;

        #endregion

        #region Methods

        public async Task CheckAsync(int itemId, int compartmentId, string lot = null, string serialNumber = null, string userName = null)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                throw new InvalidOperationException(Resources.Localized.Get("General.AreaMachineUnknow"));
            }

            try
            {
                var bay = await this.bayManager.GetBayAsync();

                await this.itemWebService.CheckAsync(itemId, new ItemOptions
                {
                    AreaId = this.bayManager.Identity.AreaId.Value,
                    BayId = bay.Id,
                    MachineId = this.bayManager.Identity.Id,
                    RunImmediately = true,
                    Lot = lot,
                    CompartmentId = compartmentId,
                    SerialNumber = serialNumber,
                    UserName = userName,
                });
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // do nothing
            }
        }

        public async Task<string> GetItemImagePathAsync(int itemId)
        {
            try
            {
                var item = await this.itemWebService.GetByIdAsync(itemId);
                return item.Image;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                return null;
            }
        }

        public async Task PickAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, int? compartmentId = null, string lot = null, string serialNumber = null, string userName = null)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                throw new InvalidOperationException(Resources.Localized.Get("General.AreaMachineUnknow"));
            }

            try
            {
                var bay = await this.bayManager.GetBayAsync();

                await this.itemWebService.PickAsync(itemId, new ItemOptions
                {
                    AreaId = this.bayManager.Identity.AreaId.Value,
                    BayId = bay.Id,
                    MachineId = this.bayManager.Identity.Id,
                    RequestedQuantity = requestedQuantity,
                    RunImmediately = true,
                    ReasonId = reasonId,
                    ReasonNotes = reasonNotes,
                    CompartmentId = compartmentId,
                    Lot = lot,
                    SerialNumber = serialNumber,
                    UserName = userName,
                });
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // do nothing
            }
        }

        public async Task PutAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, int? compartmentId = null, string lot = null, string serialNumber = null, string userName = null)
        {
            if (!this.bayManager.Identity.AreaId.HasValue)
            {
                return;
            }

            try
            {
                var bay = await this.bayManager.GetBayAsync();

                await this.itemWebService.PutAsync(itemId, new ItemOptions
                {
                    AreaId = this.bayManager.Identity.AreaId.Value,
                    BayId = bay.Id,
                    MachineId = this.bayManager.Identity.Id,
                    RequestedQuantity = requestedQuantity,
                    RunImmediately = true,
                    ReasonId = reasonId,
                    ReasonNotes = reasonNotes,
                    CompartmentId = compartmentId,
                    Lot = lot,
                    SerialNumber = serialNumber,
                    UserName = userName,
                });
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // do nothing
            }
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                do
                {
                    try
                    {
                        this.isEnabled = await this.wmsStatusWebService.IsEnabledAsync();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        await Task.Delay(5000);
                    }
                } while (true);
            });
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AspNetCore.Http;

namespace Ferretto.VW.App.Modules.Operator
{
    internal sealed class WmsDataProvider : IWmsDataProvider
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly IMachineItemsWebService itemWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private bool isEnabled;

        private bool isSocketLinkEnabled;

        #endregion

        #region Constructors

        public WmsDataProvider(
            IBayManager bayManager,
            IMachineItemsWebService itemWebService,
            IMachineWmsStatusWebService wmsStatusWebService,
            IMachineCompartmentsWebService compartmentsWebService
            )
        {
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.itemWebService = itemWebService ?? throw new System.ArgumentNullException(nameof(itemWebService));
            this.wmsStatusWebService = wmsStatusWebService ?? throw new System.ArgumentNullException(nameof(wmsStatusWebService));
            this.compartmentsWebService = compartmentsWebService ?? throw new ArgumentNullException(nameof(compartmentsWebService));
        }

        #endregion

        #region Properties

        public bool IsEnabled => this.isEnabled;

        public bool IsSocketLinkEnabled => this.isSocketLinkEnabled;

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
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    throw new InvalidOperationException(Resources.Localized.Get("General.ForbiddenOperation"));
                }
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
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    throw new InvalidOperationException(Resources.Localized.Get("General.ForbiddenOperation"));
                }
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
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    throw new InvalidOperationException(Resources.Localized.Get("General.ForbiddenOperation"));
                }
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
                        this.isSocketLinkEnabled = await this.wmsStatusWebService.SocketLinkIsEnabledAsync();
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

        public async Task UpdateItemStockAsync(int compartmentId, int itemId, double stock, int? reasonId = null, string reasonNotes = null, string lot = null, string serialNumber = null, string userName = null)
        {
            try
            {
                await this.compartmentsWebService.UpdateItemStockAsync(
                    compartmentId,
                    itemId,
                    stock,
                    new ItemOptions
                    {
                        ReasonId = reasonId,
                        ReasonNotes = reasonNotes,
                        Lot = lot,
                        SerialNumber = serialNumber,
                        UserName = userName,
                    });
                this.logger.Debug($"User requested to update compartment {compartmentId}, item {itemId} with quantity {stock}.");
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    throw new InvalidOperationException(Resources.Localized.Get("General.ForbiddenOperation"));
                }
            }
        }

        #endregion
    }
}

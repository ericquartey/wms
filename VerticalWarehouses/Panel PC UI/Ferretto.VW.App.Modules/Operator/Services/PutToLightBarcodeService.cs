using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
{
    internal sealed class PutToLightBarcodeService : IPutToLightBarcodeService
    {
        #region Fields

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachinePutToLightWebService putToLightWebService;

        private string selectedBasketCode;

        private string selectedShelfCode;

        private UserAction selectedUserAction;

        #endregion

        #region Constructors

        public PutToLightBarcodeService(
            IMachinePutToLightWebService putToLightWebService,
            IMachineIdentityWebService identityService,
            IEventAggregator eventAggregator)
        {
            this.putToLightWebService = putToLightWebService;
            this.identityService = identityService;
            this.eventAggregator = eventAggregator;

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            if (e.IsReset)
            {
                this.ResetUserSelection();

                return false;
            }

            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                case UserAction.CompleteBasket:
                case UserAction.RemoveFullBasket:

                    this.InitiateUserAction(e);
                    break;

                case UserAction.SelectBasket:

                    await this.SelectBasketAsync(e);
                    break;

                case UserAction.SelectShelf:

                    await this.SelectShelfAsync(e);
                    break;

                default:
                    return false;
            }

            return true;
        }

        private async Task AssociateBasketToShelfAsync()
        {
            try
            {
                var machineIdentity = await this.identityService.GetAsync();
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception($"Identificativo macchina non definito"));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.AssociateBasketToShelfAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess($"Il collo '{this.selectedBasketCode}' è stato associato allo scaffale '{this.selectedShelfCode}'.");

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private async Task CompleteBasketAsync()
        {
            try
            {
                var machineIdentity = await this.identityService.GetAsync();
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception($"Identificativo macchina non definito"));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.CompleteBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess($"Il collo {this.selectedBasketCode} è stato chiuso.");

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private void InitiateUserAction(UserActionEventArgs e)
        {
            this.selectedUserAction = e.UserAction;
            this.selectedBasketCode = null;
            this.selectedShelfCode = null;

            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                    this.NotifyInfo($"Inizio associazione di un collo ad uno scaffale. Scansionare lo scaffale.");
                    break;

                case UserAction.RemoveFullBasket:
                    this.NotifyInfo($"Inizio marcatura di collo pieno. Scansionare lo scaffale.");
                    break;

                case UserAction.CompleteBasket:
                    this.NotifyInfo($"Inizio chiusura collo. Scansionare lo scaffale.");
                    break;
            }
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private void NotifyInfo(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Info));
        }

        private void NotifySuccess(string message)
        {
            this.eventAggregator
              .GetEvent<PresentationNotificationPubSubEvent>()
              .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Success));
        }

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Warning));
        }

        private async Task RemoveFullBasketAsync()
        {
            try
            {
                var machineIdentity = await this.identityService.GetAsync();
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception($"Identificativo macchina non definito"));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.RemoveFullBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess($"Il collo {this.selectedBasketCode} è stato marcato come pieno.");

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private void ResetUserSelection()
        {
            this.selectedUserAction = default(UserAction);
            this.selectedBasketCode = null;
            this.selectedShelfCode = null;
        }

        private async Task RunActionAsync()
        {
            if (string.IsNullOrWhiteSpace(this.selectedBasketCode)
                ||
                string.IsNullOrWhiteSpace(this.selectedShelfCode))
            {
                return;
            }

            switch (this.selectedUserAction)
            {
                case UserAction.AssociateBasketToShelf:

                    await this.AssociateBasketToShelfAsync();
                    break;

                case UserAction.CompleteBasket:

                    await this.CompleteBasketAsync();
                    break;

                case UserAction.RemoveFullBasket:

                    await this.RemoveFullBasketAsync();
                    break;
            }
        }

        private async Task SelectBasketAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified)
            {
                this.NotifyWarning("Scansionare prima il codice a barre dell'azione da eseguire.");//TODO localize

                return;
            }

            if (this.selectedShelfCode == null)
            {
                this.NotifyWarning("Scansionare prima il codice a barre dello scaffale.");//TODO localize

                return;
            }

            this.selectedBasketCode = e.GetBasketCode();
            this.NotifyInfo($"Collo '{this.selectedBasketCode}' selezionato.");

            if (this.selectedBasketCode is null)
            {
                this.NotifyWarning("No basket code found in the barcode");//TODO localize
            }
            else
            {
                await this.RunActionAsync();
            }
        }

        private async Task SelectShelfAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified)
            {
                this.NotifyWarning("Scansionare prima il codice a barre dell'azione da eseguire.");//TODO localize

                return;
            }

            this.selectedBasketCode = null;
            this.selectedShelfCode = e.GetShelfCode();
            this.NotifyInfo($"Scaffale '{this.selectedShelfCode}' selezionato. Scansionare il collo.");

            if (this.selectedShelfCode is null)
            {
                this.NotifyWarning("No shelf code found in the barcode");//TODO localize
            }
            else
            {
                await this.RunActionAsync();
            }
        }

        #endregion
    }
}

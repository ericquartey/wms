using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
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
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.AssociateBasketToShelfAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxAssociateShelf"), this.selectedBasketCode, this.selectedShelfCode));

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
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.CompleteBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxClosed"), this.selectedBasketCode));

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
                    this.NotifyInfo(Localized.Get("OperatorApp.StartAssociateBoxToShelf"));
                    break;

                case UserAction.RemoveFullBasket:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartMarkingFullBox"));
                    break;

                case UserAction.CompleteBasket:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartClosingBox"));
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
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.RemoveFullBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxIsFull"), this.selectedBasketCode));

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
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            if (this.selectedShelfCode == null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanShelfFirst"));

                return;
            }

            this.selectedBasketCode = e.GetBasketCode();
            this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedBox"), this.selectedBasketCode));

            if (this.selectedBasketCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoBoxCode"));
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
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            this.selectedBasketCode = null;
            this.selectedShelfCode = e.GetShelfCode();
            this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedShelf"), this.selectedShelfCode));

            if (this.selectedShelfCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoShelfCode"));
            }
            else
            {
                await this.RunActionAsync();
            }
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories.Barcode
{
    internal sealed class PutToLightBarcodeService : IPutToLightBarcodeService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachinePutToLightWebService putToLightWebService;

        private string selectedBasketCode;

        private string selectedShelfCode;

        private UserAction selectedUserAction;

        #endregion

        #region Constructors

        public PutToLightBarcodeService(
            IMachinePutToLightWebService putToLightWebService,
            IEventAggregator eventAggregator)
        {
            this.putToLightWebService = putToLightWebService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            if (e.IsReset)
            {
                this.selectedUserAction = UserAction.NotSpecified;
                this.selectedBasketCode = null;
                this.selectedShelfCode = null;

                return false;
            }

            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                case UserAction.CompleteBasket:
                case UserAction.RemoveFullBasket:

                    this.selectedUserAction = e.UserAction;
                    this.selectedBasketCode = null;
                    this.selectedShelfCode = null;
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
                await this.putToLightWebService.AssociateBasketToShelfAsync(this.selectedBasketCode, this.selectedShelfCode);
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
                await this.putToLightWebService.CompleteBasketAsync(this.selectedBasketCode, this.selectedShelfCode);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
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
                await this.putToLightWebService.RemoveFullBasketAsync(this.selectedBasketCode, this.selectedShelfCode);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
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
            this.selectedBasketCode = e.GetBasketCode();

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
            this.selectedShelfCode = e.GetShelfCode();

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

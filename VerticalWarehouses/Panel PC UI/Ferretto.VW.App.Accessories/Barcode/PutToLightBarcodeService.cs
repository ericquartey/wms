using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Accessories.Barcode
{
    internal sealed class PutToLightBarcodeService : IPutToLightBarcodeService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string selectedBasketCode;

        private string selectedShelfCode;

        #endregion

        #region Constructors

        public PutToLightBarcodeService(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                    await this.AssociateBasketToShelfAsync();
                    break;

                case UserAction.CompleteBasket:
                    await this.CompleteBasketAsync();
                    break;

                case UserAction.SelectBasket:
                    this.SelectBasket(e);
                    break;

                case UserAction.SelectShelf:
                    this.SelectShelf(e);
                    break;

                case UserAction.RemoveFullBasket:
                    await this.RemoveFullBasketAsync();
                    break;

                default:
                    return false;
            }

            return true;
        }

        private async Task AssociateBasketToShelfAsync()
        {
        }

        private async Task CompleteBasketAsync()
        {
        }

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Warning));
        }

        private async Task RemoveFullBasketAsync()
        {
        }

        private void SelectBasket(UserActionEventArgs e)
        {
            this.selectedBasketCode = e.GetBasketCode();

            if (this.selectedBasketCode is null)
            {
                this.NotifyWarning("No basket code found in the barcode");//TODO localize
            }
        }

        private void SelectShelf(UserActionEventArgs e)
        {
            this.selectedShelfCode = e.GetShelfCode();

            if (this.selectedShelfCode is null)
            {
                this.NotifyWarning("No basket code found in the barcode");//TODO localize
            }
        }

        #endregion
    }
}

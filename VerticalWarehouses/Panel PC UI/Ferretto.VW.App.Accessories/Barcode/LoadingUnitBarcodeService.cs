using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.Native;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories.Barcode
{
    public class LoadingUnitBarcodeService : ILoadingUnitBarcodeService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        #endregion

        #region Constructors

        public LoadingUnitBarcodeService(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IBayManager bayManager,
            IEventAggregator eventAggregator)
        {
            this.loadingUnitsWebService = loadingUnitsWebService;
            this.bayManager = bayManager;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            if (e.IsReset)
            {
                return false;
            }

            switch (e.UserAction)
            {
                case UserAction.RecallLoadingUnit:
                    await this.RecallLoadingUnitAsync();
                    break;

                default:
                    return false;
            }

            return true;
        }

        private void NotifyError(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message));
        }

        private async Task RecallLoadingUnitAsync()
        {
            try
            {
                var bay = await this.bayManager.GetBayAsync();
                var bayPosition = bay.Carousel is null
                    ? bay.Positions.OrderByDescending(p => p.Height).FirstOrDefault(p => p.LoadingUnit != null)
                    : bay.Positions.OrderByDescending(p => p.Height).First();

                if (bayPosition?.LoadingUnit != null)
                {
                    await this.loadingUnitsWebService.RemoveFromBayAsync(bayPosition.LoadingUnit.Id);
                }
                else
                {
                    this.NotifyError("Nessun cassetto disponibile per il rientro.");
                }
            }
            catch
            {
                this.NotifyError("Errore durante la richiesta di rientro cassetto.");
            }
            finally
            {
            }
        }

        #endregion
    }
}

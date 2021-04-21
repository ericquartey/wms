using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
{
    public class LoadingUnitBarcodeService : ILoadingUnitBarcodeService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMissionOperationsService missionOperationsService;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        #endregion

        #region Constructors

        public LoadingUnitBarcodeService(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IBayManager bayManager,
            IWmsDataProvider wmsDataProvider,
            IMissionOperationsService missionOperationsService,
            INavigationService navigationService,
            IEventAggregator eventAggregator)
        {
            this.loadingUnitsWebService = loadingUnitsWebService;
            this.bayManager = bayManager;
            this.wmsDataProvider = wmsDataProvider;
            this.missionOperationsService = missionOperationsService;
            this.navigationService = navigationService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return false;
            }

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

        private void NavigateAwayFromActiveView()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.LOADING_UNIT)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.INVENTORY)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.PUT)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.PICK))
                {
                    this.navigationService.GoBackTo(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.WAIT, "NavigateAwayFromActiveView 1");
                }
                else if (
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.PICK_DETAILS)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.PUT_DETAILS)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS))
                {
                    this.navigationService.GoBackTo(nameof(Utils.Modules.Operator), Utils.Modules.Operator.OPERATOR_MENU, "NavigateAwayFromActiveView 2");
                }
            });
        }

        private void NotifyError(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message));
        }

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Warning));
        }

        private async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.logger.Debug($"User requested recall of loading unit.");

                var bay = await this.bayManager.GetBayAsync();
                var bayPosition = bay.Carousel is null
                    ? bay.Positions.OrderByDescending(p => p.Height).FirstOrDefault(p => p.LoadingUnit != null)
                    : bay.Positions.OrderByDescending(p => p.Height).First();

                var loadingUnit = bayPosition?.LoadingUnit;

                if (loadingUnit != null)
                {
                    var activeOperation = this.missionOperationsService.ActiveWmsOperation;

                    if (this.wmsDataProvider.IsEnabled && activeOperation != null)
                    {
                        var canComplete = await this.missionOperationsService.CompleteAsync(activeOperation.Id, 1);
                        if (!canComplete)
                        {
                            this.logger.Debug($"Operation '{activeOperation.Id}' cannot be completed, forcing recall of loading unit.");

                            await this.missionOperationsService.RecallLoadingUnitAsync(loadingUnit.Id);
                        }
                    }
                    else
                    {
                        await this.missionOperationsService.RecallLoadingUnitAsync(loadingUnit.Id);
                    }

                    this.NavigateAwayFromActiveView();
                }
                else
                {
                    this.NotifyWarning("Nessun cassetto disponibile per il rientro.");
                }
            }
            catch
            {
                this.NotifyError("Errore durante la richiesta di rientro cassetto.");
            }
        }

        #endregion
    }
}

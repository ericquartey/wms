using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemInventoryViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private bool isCurrentDraperyItemFullyRequested;

        #endregion

        #region Constructors

        public ItemInventoryViewModel(
            ILaserPointerService deviceService,
            IMachineAreasWebService areasWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            INavigationService navigationService,
            IOperatorNavigationService operatorNavigationService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService,
            IWmsDataProvider wmsDataProvider,
            IAuthenticationService authenticationService)
            : base(
                  deviceService,
                  areasWebService,
                  machineIdentityWebService,
                  machineConfigurationWebService,
                  navigationService,
                  operatorNavigationService,
                  loadingUnitsWebService,
                  itemsWebService,
                  compartmentsWebService,
                  missionOperationsWebService,
                  bayManager,
                  eventAggregator,
                  missionOperationsService,
                  dialogService,
                  wmsDataProvider,
                  authenticationService)
        {
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemInventory.ToString();

        public bool IsCurrentDraperyItemFullyRequested
        {
            get => this.isCurrentDraperyItemFullyRequested;
            set => this.SetProperty(ref this.isCurrentDraperyItemFullyRequested, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override bool CanConfirmOperation()
        {
            return
               !this.IsWaitingForResponse
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsBusyConfirmingOperation
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value >= 0;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            // do nothing
        }

        public override async Task OnAppearedAsync()
        {
            // Setup only reserved for Tendaggi Paradiso
            this.IsCurrentDraperyItemFullyRequested = this.IsCurrentDraperyItem && this.MissionOperation.FullyRequested.HasValue && this.MissionOperation.FullyRequested.Value;

            await base.OnAppearedAsync();

            this.MeasureUnitDescription = string.Format(Resources.Localized.Get("OperatorApp.InventoryQuantityDetected"), this.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.MeasureUnitDescription));
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = null;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
              nameof(Utils.Modules.Operator),
              Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS,
              null,
              trackCurrentView: true);
        }

        #endregion
    }
}

using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemInventoryViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private string barcodeString;

        private bool isBarcodeActive;

        private bool isCurrentDraperyItemFullyRequested;

        private bool isVisibleBarcodeReader;

        private DelegateCommand showBarcodeReaderCommand;

        #endregion

        #region Constructors

        public ItemInventoryViewModel(
            IBarcodeReaderService barcodeReaderService,
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
            IAuthenticationService authenticationService,
            IMachineAccessoriesWebService accessoriesWebService)
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
                  authenticationService,
                  accessoriesWebService)
        {
            this.barcodeReaderService = barcodeReaderService;
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemInventory.ToString();

        public ICommand BarcodeReaderCancelCommand =>
                                    this.barcodeReaderCancelCommand
                    ??
                    (this.barcodeReaderCancelCommand = new DelegateCommand(
                        async () => this.BarcodeReaderCancel()));

        public ICommand BarcodeReaderConfirmCommand =>
                                            this.barcodeReaderConfirmCommand
                    ??
                    (this.barcodeReaderConfirmCommand = new DelegateCommand(
                        async () => this.BarcodeReaderConfirm()));

        public string BarcodeString
        {
            get => this.barcodeString;
            set => this.SetProperty(ref this.barcodeString, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBarcodeActive
        {
            get => this.isBarcodeActive;
            set => this.SetProperty(ref this.isBarcodeActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItemFullyRequested
        {
            get => this.isCurrentDraperyItemFullyRequested;
            set => this.SetProperty(ref this.isCurrentDraperyItemFullyRequested, value, this.RaiseCanExecuteChanged);
        }

        public bool IsVisibleBarcodeReader
        {
            get => this.isVisibleBarcodeReader;
            set => this.SetProperty(ref this.isVisibleBarcodeReader, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ShowBarcodeReaderCommand =>
            this.showBarcodeReaderCommand
            ??
            (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader, this.CanBarcodeReader));

        #endregion

        #region Methods

        public void BarcodeReaderCancel()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;
        }

        public void BarcodeReaderConfirm()
        {
            if (!string.IsNullOrEmpty(this.BarcodeString))
            {
                this.barcodeReaderService.SimulateRead(this.BarcodeString.EndsWith("\r") ? this.BarcodeString : this.BarcodeString + "\r");

                this.BarcodeString = string.Empty;
                this.IsVisibleBarcodeReader = false;
            }
        }

        public bool CanBarcodeReader()
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

        public override void Disappear()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            //this.IsBarcodeActive = this.barcodeReaderService.

            this.IsBarcodeActive = this.barcodeReaderService.IsActive;
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.BarcodeString));
            this.barcodeReaderConfirmCommand?.RaiseCanExecuteChanged();
            this.showBarcodeReaderCommand?.RaiseCanExecuteChanged();
        }

        protected void ShowBarcodeReader()
        {
            this.IsVisibleBarcodeReader = true;
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

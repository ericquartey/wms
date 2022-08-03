using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemAddViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private readonly INavigationService navigationService;

        private bool chargeItemTextViewVisibility;

        private DelegateCommand confirmAddItemOperationCommand;

        private int confirmButtonColumnIndexPosition;

        private string itemSearchKeyTitleName;

        private int loadingUnitId;

        private bool productsDataGridViewVisibility;

        #endregion

        #region Constructors

        public ItemAddViewModel(
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
            IMachineAccessoriesWebService accessoriesWebService,
            IMachineBaysWebService machineBaysWebService)
            : base(
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
                  accessoriesWebService,
                  machineBaysWebService)
        {
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemInventory.ToString();

        public bool ChargeItemTextViewVisibility
        {
            get => this.chargeItemTextViewVisibility;
            set => this.SetProperty(ref this.chargeItemTextViewVisibility, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ConfirmAddItemOperationCommand =>
            this.confirmAddItemOperationCommand
            ??
            (this.confirmAddItemOperationCommand = new DelegateCommand(
                async () => await this.ConfirmAddItemOperationAsync(), this.CanConfirmAddItemOperation));

        public int ConfirmButtonColumnIndexPosition
        {
            get => this.confirmButtonColumnIndexPosition;
            set => this.SetProperty(ref this.confirmButtonColumnIndexPosition, value, this.RaiseCanExecuteChanged);
        }

        public string ItemSearchKeyTitleName
        {
            get => this.itemSearchKeyTitleName;
            set => this.SetProperty(ref this.itemSearchKeyTitleName, value, this.RaiseCanExecuteChanged);
        }

        public bool ProductsDataGridViewVisibility
        {
            get => this.productsDataGridViewVisibility;
            set => this.SetProperty(ref this.productsDataGridViewVisibility, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            if (this.Data is int loadingUnitId)
            {
                this.loadingUnitId = loadingUnitId;
            }
            this.ItemSearchKeyTitleName = Localized.Get(OperatorApp.ItemSearchKeySearch);
            this.ProductsDataGridViewVisibility = true;
            this.ChargeItemTextViewVisibility = true;
            this.ConfirmButtonColumnIndexPosition = 1;
            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.confirmAddItemOperationCommand?.RaiseCanExecuteChanged();
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
              nameof(Utils.Modules.Operator),
              Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS,
              null,
              trackCurrentView: false);
        }

        private bool CanConfirmAddItemOperation()
        {
            var retValue = !this.IsWaitingForResponse &&
                this.IsWmsHealthy
                && this.SelectedProduct != null;

            return retValue;
        }

        private async Task ConfirmAddItemOperationAsync()
        {
            // Note:
            // The add item operation to loading unit (standard case) is based on the product selected by the user.
            //

            if (this.SelectedProduct == null)
            {
                this.Logger.Debug($"Invalid item selected");

                this.ShowNotification(Localized.Get("OperatorApp.InvalidArgument"), Services.Models.NotificationSeverity.Error);
                return;
            }

            this.IsWaitingForResponse = true;

            try
            {
                var selectedItemId = this.SelectedProduct?.Id;
                //var compartmentId = this.SelectedItemCompartment.Id;
                var compartmentId = this.MissionOperation.CompartmentId;
                var item = await this.itemsWebService.GetByIdAsync(selectedItemId ?? 0);
                this.QuantityTolerance = item.PickTolerance ?? 0;

                var itemAddedToLoadingUnitInfo = new ItemAddedToLoadingUnitDetail
                {
                    ItemId = selectedItemId ?? 0,
                    LoadingUnitId = this.loadingUnitId,
                    CompartmentId = compartmentId,
                    ItemDescription = $"{item.Code}\n{item.Description}",
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = string.Empty,
                };

                // Show the view to adding item into current loading unit
                this.navigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.ADDITEMINTOLOADINGUNIT,
                    itemAddedToLoadingUnitInfo,
                    trackCurrentView: false);
            }
            catch
            {
                this.Logger.Error($"Invalid operation performed.");
                this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), " "), Services.Models.NotificationSeverity.Error);
            }

            this.IsWaitingForResponse = false;
        }

        #endregion
    }
}

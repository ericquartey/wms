using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IAlphaNumericBarService alphaNumericBarService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly List<MissionOperation> putLists = new List<MissionOperation>();

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private string barcodeString;

        private bool canPutBox;

        private bool chargeListTextViewVisibility;

        private DelegateCommand confirmListOperationCommand;

        private bool confirmOperation;

        private DelegateCommand confirmOperationCommand;

        private bool confirmPartialOperation;

        private DelegateCommand confirmPartialOperationCommand;

        private DelegateCommand fullOperationCommand;

        private bool isAddItemVisible;

        private bool isAddListItemVisible;

        private bool isAdjustmentVisible;

        private bool isBarcodeActive;

        private bool isBoxOperationVisible;

        private bool isCarrefour;

        private bool isCarrefourOrDraperyItem;

        private bool isCurrentDraperyItemFullyRequested;

        private bool isOperationVisible;

        private bool isPickVisible;

        private bool isPutVisible;

        private bool isVisibleBarcodeReader;

        private string itemSearchKeyTitleName = OperatorApp.ItemSearchKeySearch;

        private bool productsDataGridViewVisibility;

        private DelegateCommand putBoxCommand;

        private bool putListDataGridViewVisibility;

        private string searchItem;

        private MissionOperation selectedList;

        private DelegateCommand showBarcodeReaderCommand;

        private DelegateCommand showPutListsCommand;

        private DelegateCommand suspendCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public ItemPutViewModel(
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
            IAlphaNumericBarService alphaNumericBarService,
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
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

            this.compartmentsWebService = compartmentsWebService ?? throw new ArgumentNullException(nameof(compartmentsWebService));

            this.identityService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));

            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));

            this.barcodeReaderService = barcodeReaderService;

            this.missionOperationsService = missionOperationsService;

            this.alphaNumericBarService = alphaNumericBarService;
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPut.ToString();

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

        public bool CanPutBox
        {
            get => this.canPutBox;
            set => this.SetProperty(ref this.canPutBox, value && this.IsBoxEnabled, this.RaiseCanExecuteChanged);
        }

        public bool ChargeListTextViewVisibility
        {
            get => this.chargeListTextViewVisibility;
            set => this.SetProperty(ref this.chargeListTextViewVisibility, value);
        }

        public ICommand ConfirmListOperationCommand =>
                                   this.confirmListOperationCommand
           ??
           (this.confirmListOperationCommand = new DelegateCommand(
               async () => await this.ConfirmListOperationAsync(), this.CanConfirmListOperation));

        public bool ConfirmOperation
        {
            get => this.confirmOperation;
            set => this.SetProperty(ref this.confirmOperation, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(this.barcodeOk),
                this.CanConfirmOperationPut));

        public bool ConfirmPartialOperation
        {
            get => this.confirmPartialOperation;
            set => this.SetProperty(ref this.confirmPartialOperation, value);
        }

        public ICommand ConfirmPartialOperationCommand =>
            this.confirmPartialOperationCommand
            ??
            (this.confirmPartialOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPartialOperationAsync(),
                this.CanConfirmPartialOperationPut));

        public ICommand FullOperationCommand =>
                    this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPartialOperationAsync(),
                this.CanPartiallyCompleteOnFullCompartment));

        public bool IsAddItemVisible
        {
            get => this.isAddItemVisible;
            set => this.SetProperty(ref this.isAddItemVisible, value);
        }

        public bool IsAddListItemVisible
        {
            get => this.isAddListItemVisible;
            set
            {
                if (this.SetProperty(ref this.isAddListItemVisible, value && this.IsAddEnabled) && value)
                {
                    this.IsPickVisible = false;
                    this.IsPutVisible = false;
                    this.IsBoxOperationVisible = false;
                    this.IsAdjustmentVisible = false;
                }
            }
        }

        public bool IsAdjustmentVisible
        {
            get => this.isAdjustmentVisible;
            set => this.SetProperty(ref this.isAdjustmentVisible, value);
        }

        public bool IsBarcodeActive
        {
            get => this.isBarcodeActive;
            set => this.SetProperty(ref this.isBarcodeActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBoxOperationVisible
        {
            get => this.isBoxOperationVisible;
            set => this.SetProperty(ref this.isBoxOperationVisible, value);
        }

        public bool IsCarrefour
        {
            get => this.isCarrefour;
            set => this.SetProperty(ref this.isCarrefour, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCarrefourOrDraperyItem
        {
            get => this.isCarrefourOrDraperyItem;
            set => this.SetProperty(ref this.isCarrefourOrDraperyItem, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItemFullyRequested
        {
            get => this.isCurrentDraperyItemFullyRequested;
            set => this.SetProperty(ref this.isCurrentDraperyItemFullyRequested, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOperationVisible
        {
            get => this.isOperationVisible;
            set => this.SetProperty(ref this.isOperationVisible, value);
        }

        public bool IsPickVisible
        {
            get => this.isPickVisible;
            set => this.SetProperty(ref this.isPickVisible, value);
        }

        public bool IsPutVisible
        {
            get => this.isPutVisible;
            set => this.SetProperty(ref this.isPutVisible, value);
        }

        public bool IsVisibleBarcodeReader
        {
            get => this.isVisibleBarcodeReader;
            set => this.SetProperty(ref this.isVisibleBarcodeReader, value, this.RaiseCanExecuteChanged);
        }

        public string ItemSearchKeyTitleName
        {
            get => this.itemSearchKeyTitleName;
            set => this.SetProperty(ref this.itemSearchKeyTitleName, value);
        }

        public bool ProductsDataGridViewVisibility
        {
            get => this.productsDataGridViewVisibility;
            set => this.SetProperty(ref this.productsDataGridViewVisibility, value);
        }

        public ICommand PutBoxCommand =>
                            this.putBoxCommand
            ??
            (this.putBoxCommand = new DelegateCommand(
                async () => await this.PutBoxAsync("0"),
                this.CanPutBoxes));

        public bool PutListDataGridViewVisibility
        {
            get => this.putListDataGridViewVisibility;
            set => this.SetProperty(ref this.putListDataGridViewVisibility, value);
        }

        public IList<MissionOperation> PutLists => new List<MissionOperation>(this.putLists);

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value))
                {
                    this.IsSearching = true;
                    this.SelectedList = null;

                    this.TriggerSearchAsync().GetAwaiter();
                }
            }
        }

        public MissionOperation SelectedList
        {
            get => this.selectedList;
            set => this.SetProperty(ref this.selectedList, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ShowBarcodeReaderCommand =>
            this.showBarcodeReaderCommand
            ??
            (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader));

        public ICommand ShowPutListsCommand =>
                                                                                                                                                                                                                    this.showPutListsCommand
            ??
            (this.showPutListsCommand = new DelegateCommand(
                () => this.ShowPutLists(),
                this.CanShowPutLists));

        public ICommand SuspendCommand =>
            this.suspendCommand
            ??
            (this.suspendCommand = new DelegateCommand(
                async () => await this.SuspendOperationAsync(),
                this.CanSuspendButton));

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
            return true;
        }

        public bool CanConfirmOperationPut()
        {
            try
            {
                this.confirmOperation = this.MissionOperation != null &&
                    this.InputQuantity.Value == this.MissionRequestedQuantity &&
                    !this.IsOperationCanceled;

                if (this.confirmOperation && this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
                {
                    this.confirmOperation = false;
                }

                this.confirmPartialOperation = this.MissionOperation != null &&
                    this.InputQuantity.Value >= 0 &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity &&
                    (!this.IsQuantityLimited || this.InputQuantity.Value <= this.MissionRequestedQuantity) &&
                    !this.IsOperationCanceled;

                if (this.confirmPartialOperation && this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
                {
                    this.confirmPartialOperation = false;
                }

                this.RaisePropertyChanged(nameof(this.ConfirmOperation));

                this.RaisePropertyChanged(nameof(this.ConfirmPartialOperation));
            }
            catch (Exception)
            {
            }

            return
               !this.IsWaitingForResponse
               &&
               this.MissionOperation != null
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsOperationConfirmed
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value == this.MissionRequestedQuantity
               &&
               !this.CanPutBox;
        }

        public bool CanConfirmPartialOperationPut()
        {
            if (this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
            {
                return false;
            }

            return
               !this.IsWaitingForResponse
               &&
               this.MissionOperation != null
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsOperationConfirmed
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value != this.MissionRequestedQuantity
               &&
               (!this.IsQuantityLimited || this.InputQuantity.Value <= this.MissionRequestedQuantity)
               &&
               !this.canPutBox;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (this.CanPutBoxes() && userAction.UserAction == UserAction.VerifyItem)
            {
                await this.PutBoxAsync(userAction.Code);
                return;
            }

            if (userAction.UserAction == UserAction.VerifyItem && this.IsDoubleConfirmBarcodePut)
            {
                userAction.SetDoubleConfirm(true);
                await base.CommandUserActionAsync(userAction);
                this.CanConfirmOperationPut();
                this.CanPartiallyCompleteOnFullCompartment();
            }
            else
            {
                this.CanConfirmOperationPut();
                if (this.confirmOperation || this.confirmPartialOperation)
                {
                    if (userAction.UserAction == UserAction.VerifyItem)
                    {
                        userAction.SetDoubleConfirm(false);
                        var last = this.Mission.Operations.Last().Id == this.MissionOperation.Id;

                        if (last &&
                            this.BarcodeLenght > 0 &&
                            this.MissionOperation.SerialNumber == "1" &&
                            this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity == 1.0)
                        {
                            var messageBoxResult = this.DialogService.ShowMessage(Localized.Get("OperatorApp.LastOperationMessage"), Localized.Get("OperatorApp.Warning"), DialogType.Exclamation, DialogButtons.OK);
                            if (messageBoxResult == DialogResult.OK)
                            {
                                await base.CommandUserActionAsync(userAction);
                            }
                        }
                        else if (!this.IsDoubleConfirmBarcodePut)
                        {
                            await base.CommandUserActionAsync(userAction);
                        }
                    }
                    else if (userAction.UserAction == UserAction.ConfirmKey)
                    {
                        await this.ConfirmOperationAsync(this.barcodeOk);
                    }
                }
            }
        }

        public async Task ConfirmOperationAsync(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                bool canComplete = false;
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var itemId = this.MissionOperation.Id;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

                var compartmentId = this.MissionOperation.CompartmentId;
                var isLastMissionOnCurrentLoadingUnit = false;
                var isRequestConfirm = await this.MachineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                if (isRequestConfirm)
                {
                    isLastMissionOnCurrentLoadingUnit = await this.MissionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionOperation.Id);
                    if (isLastMissionOnCurrentLoadingUnit)
                    {
                        this.Logger.Debug($"Deactivate Bay");
                        await this.machineBaysWebService.DeactivateAsync();
                    }
                }

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght || this.MissionOperation.MaximumQuantity == decimal.One)//16 => lunghezza matrice
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
                    quantity = 1;
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
                }

                if (this.FullCompartment)
                {
                    await this.compartmentsWebService.SetFillPercentageAsync(compartmentId, 100);
                }

                if (canComplete)
                {
                    await this.UpdateWeight(loadingUnitId, quantity, item.AverageWeight, type);

                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "ConfirmOperationAsync");
                }

                if (isLastMissionOnCurrentLoadingUnit)
                {
                    var messageBoxResult = this.DialogService.ShowMessage(
                        Localized.Get("InstallationApp.ConfirmationOperation"),
                        Localized.Get("OperatorApp.DrawerBackToStorage"),
                        DialogType.Question,
                        DialogButtons.OK);
                    if (messageBoxResult is DialogResult.OK)
                    {
                        // go away...
                    }
                    this.Logger.Debug($"Activate Bay");
                    await this.machineBaysWebService.ActivateAsync();
                }

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx)
                {
                    if (webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        var error = $"{Localized.Get("General.BadRequestTitle")}: ({webEx.StatusCode})";
                        this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                    }
                }
                else if (ex is System.Net.Http.HttpRequestException hEx)
                {
                    var error = $"{Localized.Get("General.BadRequestTitle")}: ({hEx.Message})";
                    this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }

                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
                this.Logger.Debug($"Activate Bay");
                await this.machineBaysWebService.ActivateAsync();

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        public override void Disappear()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            this.BarcodeImageExist = false;

            this.alphaNumericBarService.ClearMessage();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsOperationVisible = true;
            this.IsAddItemVisible = false;
            this.IsBoxOperationVisible = false;
            this.IsAdjustmentVisible = false;

            var configuration = await this.machineConfigurationWebService.GetAsync();
            this.IsCarrefour = configuration.Machine.IsCarrefour;
            this.IsCarrefourOrDraperyItem = this.IsCarrefour || this.IsCurrentDraperyItem;
            this.IsQuantityLimited = configuration.Machine.IsQuantityLimited;

            this.IsAddListItemVisible = !this.IsAddEnabled;

            this.IsAddItem = false;
            this.IsAddItemLists = configuration.Machine.IsAddItemByList;

            this.CloseLine = false;
            this.FullCompartment = false;
            this.EmptyCompartment = false;
            this.ProductsDataGridViewVisibility = false;
            this.IsKeyboardButtonVisible = configuration.Machine.TouchHelper;

            await base.OnAppearedAsync();

            // Setup only reserved for Tendaggi Paradiso
            this.IsCurrentDraperyItemFullyRequested = this.IsCurrentDraperyItem && this.MissionOperation?.FullyRequested != null && this.MissionOperation?.FullyRequested.Value == true;

            if (this.IsQuantityLimited && this.MissionOperation != null)
            {
                this.MaxInputQuantity = this.MissionOperation.RequestedQuantity;
            }
            this.BarcodeImageExist = false;
            this.BarcodeImageSource = this.GenerateBarcodeSource(this.MissionOperation?.ItemCode);

            this.MeasureUnitDescription = string.Format(Localized.Get("OperatorApp.DrawerActivityRefillingQtyRefilled"), this.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.MeasureUnitDescription));

            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            this.searchItem = this.MissionOperation?.ItemCode;
            this.RaisePropertyChanged(nameof(this.SearchItem));
            await this.ReloadPutLists();
            this.PutListDataGridViewVisibility = this.PutLists.Any();
            this.SelectedList = this.putLists.Find(l => l.ItemListCode == this.MissionOperation?.ItemListCode);

            this.RaiseCanExecuteChanged();
        }

        public override void OnMisionOperationRetrieved()
        {
            if (this.MissionOperation != null)
            {
                if (this.MissionOperation.ItemCode.Contains("CONTENITORE"))
                {
                    this.CanPutBox = true;
                }
                else
                {
                    this.CanPutBox = false;
                }

                this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
            }
            else
            {
                this.CanPutBox = false;
            }
            this.InputQuantity = this.MissionRequestedQuantity;
            base.InitializeInputQuantity();
            this.BarcodeImageSource = this.GenerateBarcodeSource(this.MissionOperation?.ItemCode);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.BarcodeString));
            this.barcodeReaderConfirmCommand?.RaiseCanExecuteChanged();

            this.fullOperationCommand.RaiseCanExecuteChanged();
            this.confirmOperationCommand.RaiseCanExecuteChanged();
            this.confirmPartialOperationCommand.RaiseCanExecuteChanged();
            this.putBoxCommand.RaiseCanExecuteChanged();
            this.showPutListsCommand?.RaiseCanExecuteChanged();
            this.confirmListOperationCommand?.RaiseCanExecuteChanged();
        }

        protected void ShowBarcodeReader()
        {
            this.IsVisibleBarcodeReader = true;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
            nameof(Utils.Modules.Operator),
            Utils.Modules.Operator.ItemOperations.PUT_DETAILS,
            new DrawerActivityItemDetail
            {
                ItemCode = this.MissionOperation.ItemCode,
                ItemDescription = this.MissionOperation.ItemDescription,
                ListCode = this.MissionOperation.ItemListCode,
                ListDescription = this.MissionOperation.ItemListDescription,
                ListRow = this.MissionOperation.ItemListRowCode,
                Batch = this.MissionOperation.ItemListShipmentUnitCode,
                ProductionDate = this.MissionOperation.ItemProductionDate.ToString(),
                RequestedQuantity = this.MissionOperation.RequestedQuantity.ToString(),
            },
            trackCurrentView: true);
        }

        private bool CanConfirmListOperation()
        {
            var retValue = this.IsWmsEnabledAndHealthy &&
                !this.IsWaitingForResponse &&
                !this.IsBusyConfirmingOperation &&
                this.MissionOperation != null &&
                this.selectedList != null;

            return retValue;
        }

        private bool CanPartiallyCompleteOnFullCompartment()
        {
            if (this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
            {
                this.CanConfirmPartialOperation = false;
            }
            else
            {
                this.CanConfirmPartialOperation =
                    !this.IsWaitingForResponse
                    &&
                    this.MissionOperation != null
                    &&
                    !this.IsBusyAbortingOperation
                    &&
                    !this.IsBusyConfirmingOperation
                    &&
                    this.InputQuantity.HasValue
                    &&
                    this.InputQuantity.Value >= 0
                    &&
                    (!this.IsQuantityLimited || this.InputQuantity.Value <= this.MissionRequestedQuantity)
                    &&
                    !this.CanPutBox;
            }
            this.RaisePropertyChanged(nameof(this.CanConfirmPartialOperation));
            return this.CanConfirmPartialOperation;
        }

        private bool CanPutBoxes()
        {
            try
            {
                return this.MissionOperation != null
                        &&
                        !this.IsWaitingForResponse
                        &&
                        !this.IsBusyAbortingOperation
                        &&
                        !this.IsBusyConfirmingOperation
                        &&
                        this.InputQuantity.HasValue
                        &&
                        this.CanInputQuantity
                        &&
                        this.InputQuantity.Value == this.MissionRequestedQuantity
                        &&
                        this.CanPutBox;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanShowPutLists()
        {
            return this.IsAddEnabled;
        }

        private bool CanSuspendButton()
        {
            return true;
        }

        private async Task ConfirmListOperationAsync()
        {
            if (this.SelectedList == null)
            {
                this.Logger.Debug($"Invalid list selected");

                this.ShowNotification(Localized.Get("OperatorApp.InvalidArgument"), Services.Models.NotificationSeverity.Error);
                return;
            }

            this.IsWaitingForResponse = true;

            try
            {
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var selectedItemId = this.SelectedList.ItemId;

                var compartmentId = this.MissionOperation.CompartmentId;
                var item = await this.itemsWebService.GetByIdAsync(selectedItemId);
                this.QuantityTolerance = item.PickTolerance ?? 0;

                var itemAddedToLoadingUnitInfo = new ItemAddedToLoadingUnitDetail
                {
                    ItemId = selectedItemId,
                    LoadingUnitId = loadingUnitId,
                    CompartmentId = compartmentId,
                    ItemDescription = $"{item.Code}\n{item.Description}",
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = OperatorApp.Quantity,
                    MissionOperation = this.SelectedList,
                };

                // Show the view to adding item into current loading unit
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.ADDITEMINTOLOADINGUNIT,
                    itemAddedToLoadingUnitInfo,
                    trackCurrentView: true);
            }
            catch
            {
                this.Logger.Error($"Invalid operation performed.");
                this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), " "), Services.Models.NotificationSeverity.Error);
            }

            this.IsWaitingForResponse = false;
        }

        private async Task PutBoxAsync(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                bool canComplete = false;

                if (barcode != null)
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
                    this.Logger.Debug("Barcode: " + barcode);
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                }

                if (canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "PutBoxAsync");
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx)
                {
                    if (webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        var error = $"{Localized.Get("General.BadRequestTitle")}: ({webEx.StatusCode})";
                        this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                    }
                }
                else if (ex is System.Net.Http.HttpRequestException hEx)
                {
                    var error = $"{Localized.Get("General.BadRequestTitle")}: ({hEx.Message})";
                    this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }
                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
            }
        }

        private async Task ReloadPutLists()
        {
            this.ChargeListTextViewVisibility = !this.PutLists.Any();

            try
            {
                var machineId = (await this.identityService.GetAsync()).Id;

                this.putLists.Clear();

                var missionLists = await this.missionOperationsWebService.GetPutListsAsync(machineId);

                if (string.IsNullOrEmpty(this.searchItem))
                {
                    this.putLists.AddRange(missionLists);
                }
                else
                {
                    this.putLists.AddRange(missionLists.Where(m => m.ItemCode.Contains(this.searchItem)
                        || m.ItemDescription.Contains(this.searchItem)
                        || m.ItemListCode.Contains(this.searchItem)
                        || m.ItemListRowCode.Contains(this.searchItem)));
                }

                this.RaisePropertyChanged(nameof(this.PutLists));
            }
            catch (TaskCanceledException)
            {
                // normal situation
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.ChargeListTextViewVisibility = false;
            }
        }

        private void ShowPutLists()
        {
            this.IsAddListItemVisible = !this.IsAddListItemVisible;
        }

        private async Task TriggerSearchAsync()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            try
            {
                const int callDelayMilliseconds = 500;

                await Task.Delay(callDelayMilliseconds, this.tokenSource.Token);
                await this.ReloadPutLists();
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IAlphaNumericBarService alphaNumericBarService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private string barcodeItem;

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private string barcodeString;

        private bool canConfirm;

        private bool canConfirmOnEmpty;

        private bool canPickBox;

        private DelegateCommand emptyOperationCommand;

        private bool isAddItemFeatureAvailable;

        private bool isBarcodeActive;

        private bool isCarrefour;

        private bool isCarrefourOrDraperyItem;

        private bool isCurrentDraperyItemFullyRequested;

        private bool isVisibleBarcodeReader;

        private string measureUnitTxt;

        private DelegateCommand pickBoxCommand;

        private DelegateCommand showBarcodeReaderCommand;

        private DelegateCommand suspendCommand;

        private string toteBarcode;

        #endregion

        //private DelegateCommand signallingDefectCommand;

        #region Constructors

        public ItemPickViewModel(
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

            this.barcodeReaderService = barcodeReaderService;
            this.missionOperationsService = missionOperationsService;
            this.alphaNumericBarService = alphaNumericBarService;
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPick.ToString();

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

        public bool CanConfirm
        {
            get => this.canConfirm;
            set => this.SetProperty(ref this.canConfirm, value);
        }

        public bool CanConfirmOnEmpty
        {
            get => this.canConfirmOnEmpty;
            set => this.SetProperty(ref this.canConfirmOnEmpty, value);
        }

        public bool CanPickBox
        {
            get => this.canPickBox;
            set => this.SetProperty(ref this.canPickBox, value && this.IsBoxEnabled, this.RaiseCanExecuteChanged);
        }

        public ICommand EmptyOperationCommand =>
                            this.emptyOperationCommand
                    ??
                    (this.emptyOperationCommand = new DelegateCommand(
                        async () => await this.PartiallyCompleteOnEmptyCompartmentAsync(),
                        this.CanPartiallyCompleteOnEmptyCompartment));

        public override EnableMask EnableMask => EnableMask.Any;

        /// <summary>
        /// Gets or sets a value indicating whether it makes visible the 'Add' button according to a well-defined configuration machine parameter.
        /// </summary>
        public bool IsAddItemFeatureAvailable
        {
            get => this.isAddItemFeatureAvailable;
            set => this.SetProperty(ref this.isAddItemFeatureAvailable, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBarcodeActive
        {
            get => this.isBarcodeActive;
            set => this.SetProperty(ref this.isBarcodeActive, value, this.RaiseCanExecuteChanged);
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

        public bool IsVisibleBarcodeReader
        {
            get => this.isVisibleBarcodeReader;
            set => this.SetProperty(ref this.isVisibleBarcodeReader, value, this.RaiseCanExecuteChanged);
        }

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        public ICommand PickBoxCommand =>
            this.pickBoxCommand
            ??
            (this.pickBoxCommand = new DelegateCommand(
                async () => await this.PickBoxAsync("0"),
                this.CanPickBoxes));

        public ICommand ShowBarcodeReaderCommand =>
            this.showBarcodeReaderCommand
            ??
            (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader));

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

        //public ICommand SignallingDefectCommand =>promag
        //    this.signallingDefectCommand
        //    ??
        //    (this.signallingDefectCommand = new DelegateCommand(
        //        /*async*/ () => /*await*/ this.SignallingDefect(),
        //        this.CanOpenSignallingDefect));
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

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            // Handle the tote devices (use with barcode rules, be careful about the order of blocks)
            var bIsToteManaged = this.ToteBarcodeLength > 0;
            if (bIsToteManaged && userAction.UserAction == UserAction.VerifyItem)
            {
                await this.PickBoxAsync(userAction.Code);
                return;
            }

            if (this.CanPickBoxes() && userAction.UserAction == UserAction.VerifyItem)
            {
                await this.PickBoxAsync(userAction.Code);
                return;
            }

            if (this.IsDoubleConfirmBarcodePick && userAction.UserAction == UserAction.VerifyItem)
            {
                userAction.SetDoubleConfirm(true);
                await base.CommandUserActionAsync(userAction);
                this.CanPartiallyCompleteOnEmptyCompartment();
            }
            else
            {
                this.CanPartiallyCompleteOnEmptyCompartment();
                if (this.CanConfirm || this.CanConfirmPartialOperation)
                {
                    if (userAction.UserAction == UserAction.VerifyItem)
                    {
                        if (this.IsCarrefour)
                        {
                            // test begin
                            //this.MissionOperation.MaximumQuantity = decimal.One;
                            //this.MissionOperation.ItemDetails.BoxId = "box";
                            // test end
                            if (userAction.Code == this.MissionOperation?.ItemCode)
                            {
                                this.barcodeItem = userAction.Code;
                                this.ShowNotification(Localized.Get("OperatorApp.ItemBarcodeAcquired") + this.barcodeItem);
                            }
                            else if (userAction.Code == this.MissionOperation?.ItemDetails?.BoxId)
                            {
                                this.toteBarcode = userAction.Code;
                                this.ShowNotification(Localized.Get("OperatorApp.ToteBarcodeAcquired") + this.toteBarcode);
                            }
                            else
                            {
                                try
                                {
                                    var barcodeItemService = await this.ItemsWebService.GetByBarcodeAsync(userAction.Code);

                                    if (barcodeItemService?.Code == this.MissionOperation?.ItemCode)
                                    {
                                        this.barcodeItem = userAction.Code;
                                        this.ShowNotification(Localized.Get("OperatorApp.ItemBarcodeAcquired") + this.barcodeItem);
                                    }
                                    else
                                    {
                                        this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatch"), userAction.Code), Services.Models.NotificationSeverity.Error);
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatch"), userAction.Code), Services.Models.NotificationSeverity.Error);
                                    return;
                                }
                            }

                            if (!string.IsNullOrEmpty(this.barcodeItem))
                            {
                                if (!string.IsNullOrEmpty(this.toteBarcode)
                                    || string.IsNullOrEmpty(this.MissionOperation?.ItemDetails?.BoxId)
                                    || !await this.MissionOperationsService.MustCheckToteBarcode())
                                {
                                    await this.ConfirmOperationAsync(this.barcodeItem);
                                    this.barcodeItem = string.Empty;
                                    this.toteBarcode = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            await base.CommandUserActionAsync(userAction);
                        }
                    }
                    else if (userAction.UserAction == UserAction.ConfirmKey && this.barcodeOk?.Length > 0)
                    {
                        await this.ConfirmOperationAsync(this.barcodeOk);

                        return;
                    }
                }
            }

            // Handle the tote devices (use without barcode rules)
            bIsToteManaged = this.ToteBarcodeLength > 0;
            if (bIsToteManaged && userAction.UserAction == UserAction.NotSpecified)
            {
                await this.PickBoxAsync(userAction.Code);
                return;
            }

            // Handle the adding drapery item
            var bIsAddItemParameterConfigured = await this.MachineIdentityWebService.IsEnableAddItemDraperyAsync();
            if (bIsAddItemParameterConfigured && userAction.UserAction == UserAction.NotSpecified)
            {
                await base.CommandUserActionAsync(userAction);
                return;
            }
        }

        public override void Disappear()
        {
            //if (this.lastMissionOperation != null && this.MissionOperation != null)
            //{
            //    this.lastMissionOperation.RequestedQuantity = this.InputQuantity.Value;
            //}

            //if (this.lastSelectedCompartmentDetail != null && this.AvailableQuantity.HasValue)
            //{
            //    this.lastSelectedCompartmentDetail.Stock = this.AvailableQuantity.Value;
            //}

            this.alphaNumericBarService.ClearMessage();

            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            this.BarcodeImageExist = false;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            var configuration = await this.machineConfigurationWebService.GetAsync();
            this.IsCarrefour = configuration.Machine.IsCarrefour;
            this.IsQuantityLimited = configuration.Machine.IsQuantityLimited;
            this.IsCarrefourOrDraperyItem = this.IsCarrefour || this.IsCurrentDraperyItem;

            this.IsAddItem = false;
            this.IsAddItemLists = false;

            this.IsBarcodeActive = this.barcodeReaderService.IsActive;
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            //this.CanInputAvailableQuantity = true;
            this.CanInputAvailableQuantity = this.IsEnableAvailableQtyItemEditingPick;
            this.CanInputQuantity = true;
            this.CloseLine = true;
            this.FullCompartment = false;
            this.EmptyCompartment = false;
            this.RaisePropertyChanged(nameof(this.CanInputAvailableQuantity));
            this.RaisePropertyChanged(nameof(this.CanInputQuantity));

            this.Compartments = null;
            this.SelectedCompartment = null;

            this.MeasureUnitTxt = string.Format(Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            await base.OnAppearedAsync();
            if (this.IsQuantityLimited && this.MissionOperation != null)
            {
                this.MaxInputQuantity = (decimal)this.MissionOperation.RequestedQuantity;
            }

            this.BarcodeImageExist = false;
            this.BarcodeImageSource = this.GenerateBarcodeSource(this.MissionOperation?.ItemCode);

            this.IsAddItemFeatureAvailable = configuration.Machine.IsEnableAddItem &&
                configuration.Machine.IsDrapery &&
                this.IsCurrentDraperyItem;

            // Setup only reserved for Tendaggi Paradiso
            this.IsCurrentDraperyItemFullyRequested = this.IsCurrentDraperyItem && this.MissionOperation?.FullyRequested != null && this.MissionOperation.FullyRequested.Value;

            this.barcodeItem = string.Empty;
            this.toteBarcode = string.Empty;

            //this.SetLastQuantity();
        }

        public override void OnMisionOperationRetrieved()
        {
            if (this.MissionOperation != null)
            {
                if (this.MissionOperation != null)
                {
                    if (this.MissionOperation.ItemCode.Contains("CONTENITORE"))
                    {
                        this.CanPickBox = true;
                    }
                    else
                    {
                        this.CanPickBox = false;
                    }

                    this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
                }
                else
                {
                    this.CanPickBox = false;
                }

                this.InputQuantity = this.MissionRequestedQuantity;
                base.InitializeInputQuantity();
                //this.AvailableQuantity = this.MissionRequestedQuantity;
                this.BarcodeImageSource = this.GenerateBarcodeSource(this.MissionOperation?.ItemCode);

                this.RaisePropertyChanged(nameof(this.InputQuantity));
                this.RaisePropertyChanged(nameof(this.AvailableQuantity));
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.BarcodeString));

            this.barcodeReaderConfirmCommand?.RaiseCanExecuteChanged();

            this.pickBoxCommand?.RaiseCanExecuteChanged();

            this.emptyOperationCommand?.RaiseCanExecuteChanged();

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            this.emptyOperationCommand?.RaiseCanExecuteChanged();
            this.emptyOperationCommand.RaiseCanExecuteChanged();

            //this.signallingDefectCommand.RaiseCanExecuteChanged();
        }

        protected void ShowBarcodeReader()
        {
            this.IsVisibleBarcodeReader = true;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        private bool CanPartiallyCompleteOnEmptyCompartment()
        {
            try
            {
                this.CanConfirm =
                    this.MissionOperation != null
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
                    !this.CanPickBox
                    && !(this.IsDoubleConfirmBarcodePick && string.IsNullOrEmpty(this.barcodeOk));

                this.RaisePropertyChanged(nameof(this.CanConfirm));

                this.CanConfirmPartialOperation =
                    this.MissionOperation != null
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
                    this.InputQuantity.Value >= 0
                    &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity
                    &&
                    this.InputQuantity.Value <= this.AvailableQuantity
                    &&
                    (!this.IsQuantityLimited || this.InputQuantity.Value <= this.MissionRequestedQuantity)
                    &&
                    !this.CanPickBox
                    && !(this.IsDoubleConfirmBarcodePick && string.IsNullOrEmpty(this.barcodeOk));

                this.RaisePropertyChanged(nameof(this.CanConfirmPartialOperation));
            }
            catch (Exception)
            {
            }
            //return this.CanConfirm;
            return false;
        }

        //private bool CanOpenSignallingDefect()
        //{
        //    return this.IsCurrentDraperyItem;
        //}
        private bool CanPickBoxes()
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
                        this.CanPickBox;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanSuspendButton()
        {
            return true;
        }

        private async Task PartiallyCompleteOnEmptyCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

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

                var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, 0, null, false, false);
                if (!canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "PartiallyCompleteOnEmptyCompartmentAsync");
                }
                else
                {
                    await this.UpdateWeight(loadingUnitId, quantity, item.AverageWeight, type);
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
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsOperationConfirmed = false;
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
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
                this.Logger.Debug($"Activate Bay");
                await this.machineBaysWebService.ActivateAsync();
            }
        }

        /*
        // To use with drapery item
        private async Task PickBox_New_Async(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            // Show the confirm view for the drapery item
            if (this.IsCurrentDraperyItem)
            {
                this.ShowDraperyItemConfirmView(
                    barcode,
                    isPartiallyConfirmOperation: false);

                return;
            }

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                bool canComplete = false;

                if (barcode != null)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode, Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
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
                        "PickBox_New_Async");
                }

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }
        */

        private async Task PickBoxAsync(string barcode)
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

                var quantity = this.InputQuantity;
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

                if (barcode != null)
                {
                    var isToteBarcodeManaged = this.ToteBarcodeLength > 0;

                    if (isToteBarcodeManaged)
                    {
                        if (barcode.Length == this.ToteBarcodeLength &&
                            !string.IsNullOrEmpty(this.barcodeItem))
                        {
                            // acquire the tote barcode
                            var toteBarcode = barcode;
                            var itemId = this.MissionOperation.Id;

                            this.Logger.Debug($"Confirm operation for: {this.barcodeItem} item, {toteBarcode} tote");
                            this.ShowNotification(Localized.Get("OperatorApp.ToteBarcodeAcquired") + toteBarcode);

                            // The flow of operation requires:
                            // - acquisition of item barcode (first)
                            // - acquisition of tote barcode
                            // - complete the operation
                            canComplete = await this.MissionOperationsService.CompleteAsync(
                                this.MissionOperation.Id,
                                this.InputQuantity.Value,
                                this.barcodeItem,
                                0,
                                toteBarcode);

                            this.barcodeItem = string.Empty;

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
                                    "PickBoxAsync");
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(this.barcodeItem))
                            {
                                // acquire the item barcode
                                this.Logger.Debug($"Cache barcode item: {barcode}");

                                var szMsg = Localized.Get("OperatorApp.ItemBarcodeAcquired") + barcode + ".    " + Localized.Get("OperatorApp.ToteBarcodeToAcquire");
                                this.ShowNotification(szMsg);

                                // Handle a particular barcode item ("1P" prefix)
                                if (barcode.StartsWith("1P", StringComparison.CurrentCulture) &&
                                    barcode.Length >= 3)
                                {
                                    barcode = barcode.Substring(2);
                                }

                                this.barcodeItem = barcode;
                            }
                            else
                            {
                                // Invalid barcode tote
                                this.ShowNotification(Localized.Get("OperatorApp.ItemAndToteInvalidPickOperation"), Services.Models.NotificationSeverity.Error);
                                this.barcodeItem = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        this.barcodeItem = barcode;
                        var itemId = this.MissionOperation.Id;

                        this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                        canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, this.barcodeItem);

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
                                "PickBoxAsync");
                        }
                    }
                }
                else
                {
                    var itemId = this.MissionOperation.Id;
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);

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
                            "PickBoxAsync");
                    }
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

                //if (canComplete)
                //{
                //    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                //}
                //else
                //{
                //    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                //    this.NavigationService.GoBackTo(
                //        nameof(Utils.Modules.Operator),
                //        Utils.Modules.Operator.ItemOperations.WAIT,
                //        "PickBoxAsync");
                //}

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
                if (ex is MasWebApiException webEx)
                {
                    if (webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.barcodeItem = string.Empty;
                        var isToteBarcodeManaged = this.ToteBarcodeLength > 0;
                        if (isToteBarcodeManaged)
                        {
                            this.ShowNotification(Localized.Get("OperatorApp.ItemAndToteInvalidPickOperation"), Services.Models.NotificationSeverity.Error);
                        }
                        else
                        {
                            this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                        }
                    }
                    else
                    {
                        var error = $"{Localized.Get("General.BadRequestTitle")}: ({webEx.StatusCode})";
                        this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                    }

                    //throw new InvalidOperationException(Resources.Localized.Get("General.ForbiddenOperation"));
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

        #endregion

        //private /*async Task*/ void SignallingDefect()
        //{
        //    this.Logger.Debug("Signalling defect call....");

        //    this.NavigationService.Appear(
        //        nameof(Utils.Modules.Operator),
        //        Utils.Modules.Operator.ItemOperations.SIGNALLINGDEFECT,
        //        this.MissionOperation);
        //    //this.MissionOperation,
        //    //trackCurrentView: true);
        //}

        //private void SetLastQuantity()
        //{
        //    try
        //    {
        //        if (this.lastMissionOperation == null && this.MissionOperation != null)
        //        {
        //            this.lastMissionOperation = this.MissionOperation;
        //            this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //        }
        //        else if (this.MissionOperation != null)
        //        {
        //            if (this.lastMissionOperation.MissionId == this.MissionOperation.MissionId && this.lastMissionOperation.ItemCode == this.MissionOperation.ItemCode)
        //            {
        //                if (this.lastMissionOperation.RequestedQuantity != this.MissionRequestedQuantity)
        //                {
        //                    //this.MissionOperation.RequestedQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    //this.RaisePropertyChanged(nameof(this.MissionOperation));
        //                    this.InputQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    this.RaisePropertyChanged(nameof(this.InputQuantity));
        //                }
        //            }
        //            else
        //            {
        //                this.lastMissionOperation = this.MissionOperation;
        //                this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //            }
        //        }

        //        if (this.lastSelectedCompartmentDetail == null && this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //        }
        //        else if (this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            if (this.lastSelectedCompartmentDetail.ItemCode == this.SelectedCompartmentDetail.ItemCode)
        //            {
        //                if (this.lastMissionOperation.CompartmentId == this.MissionOperation.CompartmentId && this.lastMissionOperation.MissionId == this.MissionOperation.MissionId)
        //                {
        //                    if (this.lastSelectedCompartmentDetail.Stock != this.SelectedCompartmentDetail.Stock)
        //                    {
        //                        //this.SelectedCompartmentDetail.Stock = this.lastSelectedCompartmentDetail.Stock;
        //                        //this.RaisePropertyChanged(nameof(this.SelectedCompartmentDetail));
        //                        this.AvailableQuantity = this.lastSelectedCompartmentDetail.Stock;
        //                        this.RaisePropertyChanged(nameof(this.AvailableQuantity));
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //
        //    }
        //}

        /*
        // NOT USED: To be removed
        private void ShowDraperyItemConfirmView(string barcode, bool isPartiallyConfirmOperation)
        {
            this.Logger.Debug($"Show the confirm view for drapery item {this.ItemId}, description {this.MissionOperation.ItemDescription}");

            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.DRAPERYCONFIRM,
                new ItemDraperyDataConfirm
                {
                    MissionId = this.MissionOperation.Id,
                    ItemDescription = this.MissionOperation.ItemDescription,
                    AvailableQuantity = this.AvailableQuantity.Value,
                    MissionRequestedQuantity = this.MissionRequestedQuantity,
                    InputQuantity = this.InputQuantity,
                    CanInputQuantity = this.CanInputQuantity,
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = this.MeasureUnitTxt,
                    Barcode = barcode,
                    IsPartiallyCompleteOperation = isPartiallyConfirmOperation,
                    FullyRequested = this.isCurrentDraperyItemFullyRequested,
                },
                trackCurrentView: true);
        }
        */
    }
}

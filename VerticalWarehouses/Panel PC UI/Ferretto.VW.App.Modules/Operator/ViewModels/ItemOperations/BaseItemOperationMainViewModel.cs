using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationMainViewModel : BaseItemOperationViewModel, IDataErrorInfo, IOperationalContextViewModel
    {
        #region Fields

        public ItemWeightChangedMessage lastItemQuantityMessage;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private double? availableQuantity;

        private Bay bay;

        private bool canConfirmPartialOperation;

        private bool canConfirmPresent;

        private bool canInputAvailableQuantity;

        private bool closeLine;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCanceledCommand;

        private DelegateCommand confirmOperationCommand;

        private DelegateCommand confirmPartialOperationCommand;

        private DelegateCommand confirmPresentOperationCommand;

        private string inputItemCode;

        private string inputLot;

        private double? inputQuantity;

        private string inputSerialNumber;

        private bool isBoxEnabled;

        private bool isBusyAbortingOperation;

        private bool isBusyConfirmingOperation;

        private bool isBusyConfirmingPartialOperation;

        private bool isCurrentDraperyItem = false;

        private bool isInputQuantityEnabled;

        private bool isInputQuantityValid;

        private bool isItemCodeValid = true;

        private bool isItemLotValid = true;

        private bool isItemSerialNumberValid = true;

        private bool isOperationCanceled;

        private SubscriptionToken itemWeightToken;

        private double loadingUnitDepth;

        private int? loadingUnitId;

        private double loadingUnitWidth;

        private double missionRequestedQuantity;

        private SubscriptionToken missionToken;

        private bool resetFieldsOnNextAction;

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedCompartmentDetail;

        private DelegateCommand showDetailsCommand;

        private DelegateCommand signallingDefectCommand;

        private DelegateCommand weightCommand;

        #endregion

        #region Constructors

        public BaseItemOperationMainViewModel(
            IMachineIdentityWebService machineIdentityWebService,
            INavigationService navigationService,
            IOperatorNavigationService operatorNavigationService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBayManager bayManager,
            IEventAggregator eventAggregator,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(loadingUnitsWebService, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.eventAggregator = eventAggregator;
            this.compartmentsWebService = compartmentsWebService;
            this.missionOperationsWebService = missionOperationsWebService;
            this.loadingUnitsWebService = loadingUnitsWebService;
            this.operatorNavigationService = operatorNavigationService;
            this.navigationService = navigationService;
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));

            this.CompartmentColoringFunction = (compartment, selectedCompartment) => compartment == selectedCompartment ? "#0288f7" : "#444444";
        }

        #endregion

        #region Properties

        public abstract string ActiveContextName { get; }

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value, () =>
                 {
                     this.RaiseCanExecuteChanged();
                     this.CanInputAvailableQuantity = true;
                     this.CanConfirmPresent = value.HasValue && this.selectedCompartmentDetail != null && value.Value != this.selectedCompartmentDetail.Stock;
                     this.CanInputQuantity = false;
                 });
        }

        public int BarcodeLenght { get; set; }

        public bool CanConfirmPartialOperation
        {
            get => this.canConfirmPartialOperation;
            set => this.SetProperty(ref this.canConfirmPartialOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool CanConfirmPresent
        {
            get => this.canConfirmPresent;
            set => this.SetProperty(ref this.canConfirmPresent, value, this.RaiseCanExecuteChanged);
        }

        public bool CanInputAvailableQuantity
        {
            get => this.canInputAvailableQuantity;
            set => this.SetProperty(ref this.canInputAvailableQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool CloseLine
        {
            get => this.closeLine;
            set => this.SetProperty(ref this.closeLine, value, this.RaiseCanExecuteChanged);
        }

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCanceledCommand =>
            this.confirmOperationCanceledCommand
            ??
            (this.confirmOperationCanceledCommand = new DelegateCommand(
                async () => await this.ConfirmOperationCanceledAsync(),
                this.CanConfirmOperationCanceled));

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperation_New_Async("0"),    // await this.ConfirmOperationAsync()
                this.CanConfirmOperation));

        public ICommand ConfirmPartialOperationCommand =>
            this.confirmPartialOperationCommand
            ??
            (this.confirmPartialOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPartialOperationAsync(),
                this.CanConfirmPartialOperationCommand));

        public ICommand ConfirmPresentOperationCommand =>
            this.confirmPresentOperationCommand
            ??
            (this.confirmPresentOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPresentOperationAsync(),   // await this.ConfirmPresentOperation_New_Async()
                this.CanConfirmPresentOperation));

        public override EnableMask EnableMask => EnableMask.Any;

        public string Error => string.Join(
                    Environment.NewLine,
                    this[nameof(this.InputQuantity)],
                    this[nameof(this.InputLot)],
                    this[nameof(this.InputItemCode)],
                    this[nameof(this.InputSerialNumber)]);

        public string InputItemCode
        {
            get => this.inputItemCode;
            protected set => this.SetProperty(
                ref this.inputItemCode,
                value,
                () => this.IsItemCodeValid = this.inputItemCode is null || this[nameof(this.InputItemCode)] != null);
        }

        public string InputLot
        {
            get => this.inputLot;
            protected set => this.SetProperty(
                ref this.inputLot,
                value,
                () => this.IsItemLotValid = this.inputLot is null || this[nameof(this.InputLot)] != null);
        }

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(
                ref this.inputQuantity,
                value,
                () =>
                {
                    this.CanInputAvailableQuantity = false;
                    this.CanConfirmPresent = false;
                    this.CanInputQuantity = true;
                    this.IsInputQuantityValid = this[nameof(this.InputQuantity)] != null;
                    this.RaiseCanExecuteChanged();
                });
        }

        public string InputSerialNumber
        {
            get => this.inputSerialNumber;
            protected set => this.SetProperty(
                ref this.inputSerialNumber,
                value,
                () => this.IsItemSerialNumberValid = this.inputSerialNumber is null || this[nameof(this.InputSerialNumber)] != null);
        }

        public bool IsBaySideBack => this.bay?.Side == WarehouseSide.Back;

        public bool IsBoxEnabled
        {
            get => this.isBoxEnabled;
            set => this.SetProperty(ref this.isBoxEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyAbortingOperation
        {
            get => this.isBusyAbortingOperation;
            set => this.SetProperty(ref this.isBusyAbortingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            set => this.SetProperty(ref this.isBusyConfirmingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingPartialOperation
        {
            get => this.isBusyConfirmingPartialOperation;
            set => this.SetProperty(ref this.isBusyConfirmingPartialOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItem
        {
            get => this.isCurrentDraperyItem;
            set => this.SetProperty(ref this.isCurrentDraperyItem, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInputQuantityEnabled
        {
            get => this.isInputQuantityEnabled;
            set => this.SetProperty(ref this.isInputQuantityEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInputQuantityValid
        {
            get => this.isInputQuantityValid;
            protected set
            {
                this.SetProperty(ref this.isInputQuantityValid, value);
                //this.CanConfirmPartialOperation = !this.isInputQuantityValid;
            }
        }

        public bool IsItemCodeValid
        {
            get => this.isItemCodeValid;
            protected set => this.SetProperty(ref this.isItemCodeValid, value);
        }

        public bool IsItemLotValid
        {
            get => this.isItemLotValid;
            protected set => this.SetProperty(ref this.isItemLotValid, value);
        }

        public bool IsItemSerialNumberValid
        {
            get => this.isItemSerialNumberValid;
            protected set => this.SetProperty(ref this.isItemSerialNumberValid, value);
        }

        public bool IsOperationCanceled
        {
            get => this.isOperationCanceled;
            set => this.SetProperty(ref this.isOperationCanceled, value);
        }

        public IMachineItemsWebService ItemsWebService => this.itemsWebService;

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value, this.RaiseCanExecuteChanged);
        }

        public double MissionRequestedQuantity
        {
            get => this.missionRequestedQuantity;
            set => this.SetProperty(ref this.missionRequestedQuantity, value, this.RaiseCanExecuteChanged);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public CompartmentDetails SelectedCompartmentDetail
        {
            get => this.selectedCompartmentDetail;
            set => this.SetProperty(ref this.selectedCompartmentDetail, value);
        }

        public ICommand ShowDetailsCommand =>
            this.showDetailsCommand
            ??
            (this.showDetailsCommand = new DelegateCommand(this.ShowOperationDetails));

        public ICommand SignallingDefectCommand =>
            this.signallingDefectCommand
            ??
            (this.signallingDefectCommand = new DelegateCommand(
                /*async*/ () => /*await*/ this.SignallingDefect(),
                this.CanOpenSignallingDefect));

        public ICommand WeightCommand =>
                    this.weightCommand
            ??
            (this.weightCommand = new DelegateCommand(
                () => this.Weight(),
                this.CanOpenWeightPage));

        protected bool IsOperationConfirmed { get; set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputLot):
                        {
                            if (this.InputLot != null && this.InputLot == this.MissionOperation?.Lot)
                            {
                                return columnName;
                            }

                            break;
                        }

                    case nameof(this.InputQuantity):
                        {
                            if (this.InputQuantity != null && this.InputQuantity == this.MissionRequestedQuantity)
                            {
                                return columnName;
                            }

                            break;
                        }

                    //case nameof(this.AvailableQuantity):
                    //    {
                    //        if (this.AvailableQuantity != null && this.AvailableQuantity != this.MissionOperation?.RequestedQuantity)
                    //        {
                    //            return columnName;
                    //        }
                    //        }

                    //        break;
                    //    }

                    case nameof(this.InputItemCode):
                        {
                            if (this.InputItemCode != null
                                &&
                                this.MissionOperation?.ItemCode != null
                                &&
                                this.InputItemCode == this.MissionOperation.ItemCode)
                            {
                                return columnName;
                            }

                            break;
                        }

                        // TODO - for future use
                        //case nameof(this.InputSerialNumber):
                        //    {
                        //        if (this.InputSerialNumber != null
                        //            &&
                        //            this.MissionOperation?.SerialNumber != null
                        //            &&
                        //            this.InputSerialNumber == this.MissionOperation.SerialNumber)
                        //        {
                        //            return columnName;
                        //        }

                        //        break;
                        //    }
                }
                return null;
            }
        }

        #endregion

        #region Methods

        public virtual bool CanConfirmOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.MissionOperation != null
                &&
                !this.IsBusyAbortingOperation
                //&&
                //!this.IsBusyConfirmingOperation
                &&
                !this.IsOperationConfirmed
                &&
                !this.isOperationCanceled
                &&
                this.InputQuantity.HasValue
                //&&
                //this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value == this.MissionRequestedQuantity;
        }

        public virtual bool CanConfirmOperationCanceled()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsOperationConfirmed
                &&
                this.isOperationCanceled;
        }

        public bool CanConfirmPartialOperationCommand()
        {
            var visibility =
                !this.IsWaitingForResponse
                &&
                this.MissionOperation != null
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsOperationConfirmed
                &&
                !this.isOperationCanceled
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value == this.MissionRequestedQuantity;

            return !visibility;
        }

        public bool CanOpenWeightPage()
        {
            //if (this.MachineService.Bay.Accessories.WeightingScale is null)
            //{
            //    return false;
            //}
            //else
            //{
            //    return this.MachineService.Bay.Accessories.WeightingScale.IsEnabledNew;
            //}

            return this.MachineService.Bay.Accessories.WeightingScale is null ? false : this.MachineService.Bay.Accessories.WeightingScale.IsEnabledNew;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.IsReset)
            {
                this.ResetInputFields();

                return;
            }

            if (this.resetFieldsOnNextAction)
            {
                this.ResetInputFields();
            }

            switch (e.UserAction)
            {
                case UserAction.VerifyItem:
                    {
                        this.InputItemCode = e.GetItemCode() ?? this.InputItemCode;

                        this.InputQuantity = e.GetItemQuantity() ?? this.InputQuantity;

                        this.AvailableQuantity = e.GetItemQuantity() ?? this.availableQuantity; //to fix

                        this.InputSerialNumber = e.GetItemSerialNumber() ?? this.InputSerialNumber;

                        this.InputLot = e.GetItemLot() ?? this.InputLot;

                        e.HasMismatch = !this.IsItemCodeValid || !this.IsItemLotValid || !this.IsItemSerialNumberValid;
                        if (e.HasMismatch
                            && !this.IsItemCodeValid
                            && e.GetItemCode() != null
                            && this.MissionOperation?.ItemCode != null
                            )
                        {
                            if (this.BarcodeLenght > 0 && e.GetItemCode().Length == this.BarcodeLenght)
                            {
                                e.HasMismatch = false;
                            }
                            else
                            {
                                var item = await this.itemsWebService.GetByBarcodeAsync(e.GetItemCode());
                                e.HasMismatch = (item.Code != this.MissionOperation.ItemCode);
                            }
                        }

                        if (e.HasMismatch)
                        {
                            if (e.RestartOnMismatch)
                            {
                                this.resetFieldsOnNextAction = true;
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatchRestart"), e.Code), Services.Models.NotificationSeverity.Warning);
                            }
                            else
                            {
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatch"), e.Code), Services.Models.NotificationSeverity.Warning);
                            }
                        }
                        else
                        {
                            //this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationValidated"), Services.Models.NotificationSeverity.Success);
                            this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + e.Code), Services.Models.NotificationSeverity.Success);

                            await this.ConfirmOperationAsync(e.Code);
                        }
                    }

                    break;

                case UserAction.ConfirmOperation:
                    {
                        this.InputItemCode = e.GetItemCode() ?? this.InputItemCode;

                        this.InputQuantity = e.GetItemQuantity() ?? this.InputQuantity;

                        //this.AvailableQuantity = e.GetItemQuantity() ?? this.availableQuantity; //to fix

                        this.InputSerialNumber = e.GetItemSerialNumber() ?? this.InputSerialNumber;

                        this.InputLot = e.GetItemLot() ?? this.InputLot;

                        e.HasMismatch = !this.IsItemCodeValid || !this.IsItemLotValid || !this.IsItemSerialNumberValid;
                        if (e.HasMismatch)
                        {
                            if (e.RestartOnMismatch)
                            {
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatchRestart"), e.Code), Services.Models.NotificationSeverity.Warning);
                                this.resetFieldsOnNextAction = true;
                            }
                            else
                            {
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatch"), e.Code), Services.Models.NotificationSeverity.Warning);
                            }
                        }
                        else
                        {
                            if (this.InputQuantity.HasValue)
                            {
                                this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + e.Code), Services.Models.NotificationSeverity.Success);

                                await this.ConfirmOperationAsync(e.Code);
                            }
                            else
                            {
                                this.ShowNotification(Localized.Get("OperatorApp.BarcodeMissingQuantity"), Services.Models.NotificationSeverity.Warning);
                            }
                        }
                    }

                    break;
            }
        }

        public async Task ConfirmOperation_New_Async(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            if (this.IsCurrentDraperyItem)
            {
                this.ShowDraperyItemConfirmView(
                    this.MissionOperation.ItemBarcode,
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

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght)
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
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
                    this.navigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT);
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

                bool canComplete = false;

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght)
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
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
                    this.navigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT);
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

        public async Task ConfirmOperationCanceledAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsBusyConfirmingPartialOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.ShowNotification(Localized.Get("OperatorApp.OperationCancelledConfirmed"));

                // ?????????????? this.NavigationService.GoBack();
                // this.MissionOperation = null;
                // this.Mission = null;
                await this.MissionOperationsService.RefreshAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
                this.IsBusyConfirmingPartialOperation = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        public async Task ConfirmPartialOperationAsync()
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingPartialOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;
                bool canComplete;

                if (this.closeLine)
                {
                    canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
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
                }

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingPartialOperation = false;
                this.IsOperationConfirmed = false;
            }
            catch (Exception ex2)
            {
                this.ShowNotification(ex2);
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

        public override void Disappear()
        {
            this.missionToken?.Dispose();
            this.missionToken = null;

            base.Disappear();
        }

        public void InitializeInputQuantity()
        {
            if (this.lastItemQuantityMessage != null)
            {
                if (this.lastItemQuantityMessage.MeasureadQuantity.HasValue)
                {
                    this.InputQuantity = this.lastItemQuantityMessage.MeasureadQuantity;
                }
                else if (this.lastItemQuantityMessage.RequestedQuantity.HasValue)
                {
                    this.InputQuantity = this.lastItemQuantityMessage.RequestedQuantity;
                }

                //this.lastItemQuantityMessage = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            //string value = System.Configuration.ConfigurationManager.AppSettings["Box"];

            //this.IsBoxEnabled = value.ToLower() == "true" ? true : false;

            this.IsBoxEnabled = await this.machineIdentityWebService.GetBoxEnableAsync();

            //value = System.Configuration.ConfigurationManager.AppSettings["ItemUniqueIdLength"];

            //if (int.TryParse(value, out var def))
            //{
            //    this.BarcodeLenght = def;
            //}
            //else
            //{
            //    this.BarcodeLenght = 0;
            //}

            this.BarcodeLenght = await this.machineIdentityWebService.GetItemUniqueIdLengthAsync();

            this.IsWaitingForResponse = false;
            this.IsBusyAbortingOperation = false;
            this.IsBusyConfirmingOperation = false;
            this.IsBusyConfirmingPartialOperation = false;
            this.IsOperationConfirmed = false;
            this.IsOperationCanceled = false;
            this.AvailableQuantity = null;
            this.SelectedCompartment = null;
            this.InitializeInputQuantity();

            this.bay = await this.BayManager.GetBayAsync();

            this.RaisePropertyChanged(nameof(this.IsBaySideBack));

            this.missionToken = this.missionToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnMissionChangedAsync(),
                        ThreadOption.UIThread,
                        false);

            this.itemWeightToken = this.itemWeightToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ItemWeightChangedMessage>>()
                    .Subscribe(
                        (e) => this.OnItemWeightChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            await base.OnAppearedAsync();

            await this.MissionOperationsService.RefreshAsync();
            await this.GetLoadingUnitDetailsAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.confirmPartialOperationCommand?.RaiseCanExecuteChanged();
            this.confirmPresentOperationCommand?.RaiseCanExecuteChanged();
            this.showDetailsCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCanceledCommand?.RaiseCanExecuteChanged();
            this.weightCommand?.RaiseCanExecuteChanged();
            this.signallingDefectCommand?.RaiseCanExecuteChanged();
        }

        protected void ShowOperationCanceledMessage()
        {
            this.IsOperationCanceled = true;
            this.CanInputQuantity = false;
            this.IsWaitingForResponse = false;
            this.lastItemQuantityMessage = null;
            this.IsBusyConfirmingOperation = false;
            this.IsBusyConfirmingPartialOperation = false;
            this.IsOperationConfirmed = false;

            var msg = this.GetNoLongerOperationMessageByType();
            this.DialogService.ShowMessage(msg, Localized.Get("OperatorApp.OperationCancelled"), DialogType.Error, DialogButtons.OK);
            this.ShowNotification(msg, Services.Models.NotificationSeverity.Warning);
            //this.HideNavigationBack();
        }

        protected abstract void ShowOperationDetails();

        private static IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentMissionInfo> compartmentsFromMission)
        {
            try
            {
                return compartmentsFromMission
                    .Where(c =>
                        c.Width.HasValue
                        ||
                        c.Depth.HasValue
                        ||
                        c.XPosition.HasValue
                        ||
                        c.YPosition.HasValue)
                    .Select(c => new TrayControlCompartment
                    {
                        Depth = c.Depth.Value,
                        Id = c.Id,
                        Width = c.Width.Value,
                        XPosition = c.XPosition.Value,
                        YPosition = c.YPosition.Value,
                        Barcode = c.Barcode,
                    });
            }
            catch (Exception)
            {
                return Array.Empty<TrayControlCompartment>();
            }
        }

        private bool CanConfirmPresentOperation()
        {
            this.CanConfirmPresent = !this.IsWaitingForResponse
                &&
                this.CanInputAvailableQuantity
                &&
                this.MissionOperation != null
                &&
                !this.IsBusyAbortingOperation
                //&&
                //!this.IsBusyConfirmingOperation
                &&
                !this.IsOperationConfirmed
                &&
                !this.isOperationCanceled;
            return this.CanConfirmPresent;
        }

        private bool CanOpenSignallingDefect()
        {
            return this.IsCurrentDraperyItem;
        }

        private async Task ConfirmPresentOperation_New_Async()
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The present quantity should have a value");

            if (this.IsCurrentDraperyItem)
            {
                this.ShowDraperyItemConfirmView(
                    this.MissionOperation.ItemBarcode,
                    isPartiallyConfirmOperation: false);

                return;
            }

            try
            {
                //this.IsBusyConfirmingPartialOperation = true;
                //this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                //var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                var reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Inventory);

                await this.compartmentsWebService.UpdateItemStockAsync(
                        this.selectedCompartment.Id,
                        this.selectedCompartmentDetail.ItemId.Value,
                        this.availableQuantity.Value,
                        reasons.First().Id,
                        "update present quantity");

                //await this.MissionOperationsService.RecallLoadingUnitAsync(this.loadingUnitId.Value);

                //this.NavigationService.GoBack();
                //this.operatorNavigationService.NavigateToDrawerViewConfirmPresent();

                this.ShowNotification(Localized.Get("OperatorApp.UpdatedValue"), Services.Models.NotificationSeverity.Info);

                await this.MissionOperationsService.RefreshAsync();
                await this.GetLoadingUnitDetailsAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                //this.IsBusyConfirmingPartialOperation = false;
                //this.IsOperationConfirmed = false;
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

        private async Task ConfirmPresentOperationAsync()
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The present quantity should have a value");

            try
            {
                //this.IsBusyConfirmingPartialOperation = true;
                //this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                //var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                var reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Inventory);

                await this.compartmentsWebService.UpdateItemStockAsync(
                        this.selectedCompartment.Id,
                        this.selectedCompartmentDetail.ItemId.Value,
                        this.availableQuantity.Value,
                        reasons.First().Id,
                        "update present quantity");

                //await this.MissionOperationsService.RecallLoadingUnitAsync(this.loadingUnitId.Value);

                //this.NavigationService.GoBack();
                //this.operatorNavigationService.NavigateToDrawerViewConfirmPresent();

                this.ShowNotification(Localized.Get("OperatorApp.UpdatedValue"), Services.Models.NotificationSeverity.Info);

                await this.MissionOperationsService.RefreshAsync();
                await this.GetLoadingUnitDetailsAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                //this.IsBusyConfirmingPartialOperation = false;
                //this.IsOperationConfirmed = false;
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

        private async Task GetLoadingUnitDetailsAsync()
        {
            if (this.Mission is null || this.Mission.LoadingUnit is null || this.MissionOperation is null)
            {
                this.Compartments = null;
                this.SelectedCompartment = null;
                this.AvailableQuantity = null;
                this.IsCurrentDraperyItem = false;
            }
            else
            {
                this.LoadingUnitWidth = this.Mission.LoadingUnit.Width;
                this.LoadingUnitDepth = this.Mission.LoadingUnit.Depth;

                this.Compartments = MapCompartments(this.Mission.LoadingUnit.Compartments);
                this.SelectedCompartment = this.Compartments.SingleOrDefault(c =>
                    c.Id == this.MissionOperation.CompartmentId);

                try
                {
                    this.loadingUnitId = this.Mission.LoadingUnit.Id;
                    //var unit = await this.missionOperationsWebService.GetUnitIdAsync(this.Mission.Id);
                    var itemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(this.loadingUnitId.Value);
                    itemsCompartments = itemsCompartments?.Where(ic => !(ic.ItemId is null));
                    this.SelectedCompartmentDetail = itemsCompartments.Where(s => s.Id == this.selectedCompartment.Id && s.ItemId == (this.MissionOperation?.ItemId ?? 0)).FirstOrDefault();
                    this.AvailableQuantity = this.selectedCompartmentDetail?.Stock;
                }
                catch (Exception)
                {
                    this.CanInputAvailableQuantity = true;
                    this.CanInputQuantity = true;
                    this.AvailableQuantity = null;
                }

                try
                {
                    var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                    //this.IsCurrentDraperyItem = item.IsDraperyItem;  // ---->  Uncomment this code       **//**//**//**
                    this.IsCurrentDraperyItem = true;
                }
                catch (Exception)
                {
                    this.IsCurrentDraperyItem = false;
                }
            }

            this.CanInputAvailableQuantity = true;
            this.CanInputQuantity = true;
        }

        private string GetNoLongerOperationMessageByType()
        {
            var noLongerOperationMsg = string.Empty;
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    noLongerOperationMsg = Localized.Get("OperatorApp.IfPickedItemsPutThemBackInTheOriginalCompartment");
                    break;

                case MissionOperationType.Put:
                    noLongerOperationMsg = Localized.Get("OperatorApp.RemoveAnySpilledItemsFromCompartment");
                    break;

                case MissionOperationType.Inventory:
                    noLongerOperationMsg = Localized.Get("OperatorApp.InventoryOperationCancelled");
                    break;

                default:
                    break;
            }

            return noLongerOperationMsg;
        }

        private void HideNavigationBack()
        {
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    this.IsBackNavigationAllowed = false;
                    break;

                case MissionOperationType.Put:
                    this.IsBackNavigationAllowed = false;
                    break;

                default:
                    break;
            }
        }

        private void OnItemWeightChangedAsync(ItemWeightChangedMessage itemWeightChanged)
        {
            this.lastItemQuantityMessage = itemWeightChanged;
            this.InitializeInputQuantity();
        }

        private async Task OnMissionChangedAsync()
        {
            if (this.IsOperationConfirmed || this.IsOperationCanceled)
            {
                this.IsOperationConfirmed = false;

                await this.RetrieveMissionOperationAsync();

                await this.GetLoadingUnitDetailsAsync();
            }

            this.IsBusyConfirmingOperation = false;
            this.IsBusyConfirmingPartialOperation = false;
            this.IsWaitingForResponse = false;
        }

        private void ResetInputFields()
        {
            this.InputSerialNumber = null;
            this.InputLot = null;
            this.InputItemCode = null;
            this.InputQuantity = this.MissionRequestedQuantity;
            //this.AvailableQuantity = this.MissionRequestedQuantity; //to fix
        }

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
                    AvailableQuantity = this.AvailableQuantity,
                    MissionRequestedQuantity = this.MissionRequestedQuantity,
                    InputQuantity = this.InputQuantity,
                    CanInputQuantity = this.CanInputQuantity,
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = string.Empty,
                    Barcode = barcode,
                    IsPartiallyCompleteOperation = isPartiallyConfirmOperation,
                },
                trackCurrentView: true);
        }

        private /*async Task*/ void SignallingDefect()
        {
            this.Logger.Debug("Signalling defect call....");

            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.SIGNALLINGDEFECT,
                this.MissionOperation);
            //this.MissionOperation,
            //trackCurrentView: true);
        }

        private void Weight()
        {
            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.WEIGHT,
                this.MissionOperation);
        }

        #endregion
    }
}

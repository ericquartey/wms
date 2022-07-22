using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;
using ZXing;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationMainViewModel : BaseItemOperationViewModel, IDataErrorInfo, IOperationalContextViewModel
    {
        #region Fields

        public string barcodeOk;

        public ItemWeightChangedMessage lastItemQuantityMessage;

        private const int DefaultPageSize = 20;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 4;

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly ILaserPointerService deviceService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private DelegateCommand addItemCommand;

        private List<ProductInMachine> allProducts = new List<ProductInMachine>();

        private int? areaId;

        private double? availableQuantity;

        private bool barcodeImageExist;

        private ImageSource barcodeImageSource;

        private Bay bay;

        private bool canConfirmPartialOperation;

        private bool canConfirmPresent;

        private bool canInputAvailableQuantity;

        private bool closeLine;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmItemOperationCommand;

        private DelegateCommand confirmOperationCanceledCommand;

        private DelegateCommand confirmOperationCommand;

        private DelegateCommand confirmPartialOperationCommand;

        private DelegateCommand confirmPresentOperationCommand;

        private int currentItemIndex;

        private bool emptyCompartment;

        private bool fullCompartment;

        private string inputItemCode;

        private string inputLot;

        private double? inputQuantity;

        private string inputSerialNumber;

        private bool isAddItem;

        private bool isBoxEnabled;

        private bool isBusyAbortingOperation;

        private bool isBusyConfirmingOperation;

        private bool isBusyConfirmingPartialOperation;

        private bool isBusyLoading;

        private bool isCurrentDraperyItem;

        private bool isDoubleConfirmBarcodePick;

        private bool isDoubleConfirmBarcodePut;

        private bool isInputQuantityEnabled;

        private bool isInputQuantityValid;

        private bool isItemCodeValid = true;

        private bool isItemLotValid = true;

        private bool isItemSerialNumberValid = true;

        private bool isOperationCanceled;

        private bool isQuantityLimited;

        private bool isSearching;

        private SubscriptionToken itemWeightToken;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private double maxInputQuantity;

        private int maxKnownIndexSelection;

        private double missionRequestedQuantity;

        private SubscriptionToken missionToken;

        private List<ItemInfo> products = new List<ItemInfo>();

        private SubscriptionToken productsChangedToken;

        private bool resetFieldsOnNextAction;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedCompartmentDetail;

        private TrayControlCompartment selectedItemCompartment;

        private string selectedItemTxt;

        private ItemInfo selectedProduct;

        private DelegateCommand showDetailsCommand;

        private DelegateCommand signallingDefectCommand;

        private CancellationTokenSource tokenSource;

        private DelegateCommand weightCommand;

        #endregion

        #region Constructors

        public BaseItemOperationMainViewModel(
            IMachineAreasWebService areasWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            INavigationService navigationService,
            IOperatorNavigationService operatorNavigationService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBayManager bayManager,
            IEventAggregator eventAggregator,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService,
            IWmsDataProvider wmsDataProvider,
            IAuthenticationService authenticationService,
            IMachineAccessoriesWebService accessoriesWebService)
            : base(loadingUnitsWebService, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.eventAggregator = eventAggregator;
            this.compartmentsWebService = compartmentsWebService;
            this.missionOperationsWebService = missionOperationsWebService;
            this.loadingUnitsWebService = loadingUnitsWebService;
            this.operatorNavigationService = operatorNavigationService;
            this.navigationService = navigationService;
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.accessoriesWebService = accessoriesWebService ?? throw new ArgumentNullException(nameof(accessoriesWebService));

            this.CompartmentColoringFunction = (compartment, selectedCompartment) => compartment == selectedCompartment ? "#0288f7" : "#444444";
        }

        #endregion

        #region Properties

        public abstract string ActiveContextName { get; }

        public ICommand AddItemCommand =>
                    this.addItemCommand
            ??
            (this.addItemCommand = new DelegateCommand(
                () =>
                {
                    this.isAddItem = !this.isAddItem;

                    if (!this.isAddItem)
                    {
                        this.SelectedProduct = this.products.FirstOrDefault();
                    }
                    this.RaisePropertyChanged(nameof(this.IsAddItem));
                },
                this.CanLoadItems));

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value, () =>
                 {
                     this.RaiseCanExecuteChanged();
                     //this.CanInputAvailableQuantity = true;
                     this.CanInputAvailableQuantity = this.IsEnableAvailableQtyItemEditingPick;
                     this.CanConfirmPresent = value.HasValue && this.selectedCompartmentDetail != null && value.Value != this.selectedCompartmentDetail.Stock;
                     this.CanInputQuantity = false;
                 });
        }

        public bool BarcodeImageExist
        {
            get => this.barcodeImageExist;
            set => this.SetProperty(ref this.barcodeImageExist, value);
        }

        public ImageSource BarcodeImageSource
        {
            get => this.barcodeImageSource;
            set => this.SetProperty(ref this.barcodeImageSource, value);
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

        public ICommand ConfirmItemOperationCommand =>
           this.confirmItemOperationCommand
           ??
           (this.confirmItemOperationCommand = new DelegateCommand(
               async () => await this.ConfirmItemOperationAsync(), this.CanConfirmItemOperation));

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
                async () => await this.ConfirmOperationAsync(this.barcodeOk),
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
                async () => await this.ConfirmPresentOperationAsync(),
                this.CanConfirmPresentOperation));

        public bool EmptyCompartment
        {
            get => this.emptyCompartment;
            set => this.SetProperty(ref this.emptyCompartment, value, this.RaiseCanExecuteChanged);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public string Error => string.Join(
                    Environment.NewLine,
                    this[nameof(this.InputQuantity)],
                    this[nameof(this.InputLot)],
                    this[nameof(this.InputItemCode)],
                    this[nameof(this.InputSerialNumber)]);

        public bool FullCompartment
        {
            get => this.fullCompartment;
            set => this.SetProperty(ref this.fullCompartment, value, this.RaiseCanExecuteChanged);
        }

        public string InputItemCode
        {
            get => this.inputItemCode;
            protected set => this.SetProperty(
                ref this.inputItemCode,
                value);
        }

        public string InputLot
        {
            get => this.inputLot;
            protected set => this.SetProperty(
                ref this.inputLot,
                value,
                () => this.IsItemLotValid = value is null || this[nameof(this.InputLot)] != null);
        }

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set
            {
                if (value >= 0
                    && value <= this.MaxInputQuantity)
                {
                    this.SetProperty(ref this.inputQuantity, value);
                    this.CanInputAvailableQuantity = false;
                    this.CanConfirmPresent = false;
                    this.CanInputQuantity = true;
                    this.IsInputQuantityValid = this[nameof(this.InputQuantity)] != null;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string InputSerialNumber
        {
            get => this.inputSerialNumber;
            protected set => this.SetProperty(
                ref this.inputSerialNumber,
                value,
                () => this.IsItemSerialNumberValid = this.inputSerialNumber is null || this[nameof(this.InputSerialNumber)] != null);
        }

        public bool IsAddItem
        {
            get => this.isAddItem;
            set => this.SetProperty(ref this.isAddItem, value);
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

        public bool IsBusyLoading
        {
            get => this.isBusyLoading;
            set => this.SetProperty(ref this.isBusyLoading, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItem
        {
            get => this.isCurrentDraperyItem;
            set => this.SetProperty(ref this.isCurrentDraperyItem, value, this.RaiseCanExecuteChanged);
        }

        public bool IsDoubleConfirmBarcodePick
        {
            get => this.isDoubleConfirmBarcodePick;
            set => this.SetProperty(ref this.isDoubleConfirmBarcodePick, value);
        }

        public bool IsDoubleConfirmBarcodePut
        {
            get => this.isDoubleConfirmBarcodePut;
            set => this.SetProperty(ref this.isDoubleConfirmBarcodePut, value);
        }

        public bool IsEnableAvailableQtyItemEditingPick { get; set; }

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

        public bool IsMinebeaScale { get; private set; }

        public bool IsOperationCanceled
        {
            get => this.isOperationCanceled;
            set => this.SetProperty(ref this.isOperationCanceled, value);
        }

        public bool IsQuantityLimited
        {
            get => this.isQuantityLimited;
            set => this.SetProperty(ref this.isQuantityLimited, value, this.RaiseCanExecuteChanged);
        }

        public bool IsSearching
        {
            get => this.isSearching;
            set => this.SetProperty(ref this.isSearching, value, this.RaiseCanExecuteChanged);
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

        public IMachineIdentityWebService MachineIdentityWebService => this.machineIdentityWebService;

        public double MaxInputQuantity
        {
            get => this.maxInputQuantity;
            set => this.SetProperty(ref this.maxInputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public double MissionRequestedQuantity
        {
            get => this.missionRequestedQuantity;
            set => this.SetProperty(ref this.missionRequestedQuantity, value, this.RaiseCanExecuteChanged);
        }

        public double? NetWeight { get; set; }

        public IList<ItemInfo> Products => new List<ItemInfo>(this.products);

        public ICommand ScrollCommand => this.scrollCommand ?? (this.scrollCommand = new DelegateCommand<object>((arg) => this.Scroll(arg)));

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value))
                {
                    this.IsSearching = true;
                    //this.TriggerSearchAsync().GetAwaiter();  // Do not perform the searching routine
                }
            }
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

        public TrayControlCompartment SelectedItemCompartment
        {
            get => this.selectedItemCompartment;
            set => this.SetProperty(ref this.selectedItemCompartment, value);
        }

        public string SelectedItemTxt
        {
            get => this.selectedItemTxt;
            set => this.SetProperty(ref this.selectedItemTxt, value, this.RaiseCanExecuteChanged);
        }

        public ItemInfo SelectedProduct
        {
            get => this.selectedProduct;
            set
            {
                if (value is null)
                {
                    this.RaisePropertyChanged();
                    this.selectedItemTxt = Resources.Localized.Get("OperatorApp.RequestedQuantityBase");
                    this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                    return;
                }

                this.SetProperty(ref this.selectedProduct, value);

                var selectedItemId = this.SelectedProduct?.Id;
                this.SetCurrentIndex(selectedItemId);
                this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                Task.Run(async () => await this.SelectNextItemAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                this.RaiseCanExecuteChanged();
            }
        }

        public ICommand ShowDetailsCommand =>
            this.showDetailsCommand
            ??
            (this.showDetailsCommand = new DelegateCommand(this.ShowOperationDetails));

        public ICommand SignallingDefectCommand =>
            this.signallingDefectCommand
            ??
            (this.signallingDefectCommand = new DelegateCommand(
                () => this.SignallingDefect(),
                this.CanOpenSignallingDefect));

        public double? Tare { get; set; }

        public int ToteBarcodeLength { get; set; }

        public double? UnitWeight { get; set; }

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
                case UserAction.NotSpecified:
                    {
                        if (this.isAddItem)
                        {
                            var bIsAddItemParameterConfigured = await this.MachineIdentityWebService.IsEnableAddItemDraperyAsync();

                            // Check the existence of drapery item for the adding operation
                            if (bIsAddItemParameterConfigured)
                            {
                                await this.ShowItemDetailsByBarcode_DraperyItemStuff_Async(e.Code);
                            }
                        }
                    }

                    break;

                case UserAction.VerifyItem:
                    {
                        if (this.isAddItem)
                        {
                            await this.ShowItemDetailsByBarcodeAsync(e);

                            break;
                        }

                        this.InputItemCode = e.GetItemCode() ?? this.InputItemCode;
                        this.IsItemCodeValid = this.InputItemCode is null || this.MissionOperation?.ItemCode is null || this.InputItemCode == this.MissionOperation.ItemCode;

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
                                try
                                {
                                    var item = await this.itemsWebService.GetByBarcodeAsync(e.GetItemCode());
                                    e.HasMismatch = (item?.Code != this.MissionOperation.ItemCode);
                                    if (!e.HasMismatch)
                                    {
                                        this.logger.Debug($"GetByBarcodeAsync '{item?.Code}'.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), e.GetItemCode()), Services.Models.NotificationSeverity.Warning);
                                }
                            }
                        }

                        if (e.HasMismatch)
                        {
                            this.barcodeOk = string.Empty;
                            if (e.RestartOnMismatch)
                            {
                                this.resetFieldsOnNextAction = true;
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatchRestart"), e.Code), Services.Models.NotificationSeverity.Warning);
                            }
                            else
                            {
                                this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeMismatch"), e.Code), Services.Models.NotificationSeverity.Error);
                            }
                        }
                        else
                        {
                            //this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationValidated"), Services.Models.NotificationSeverity.Success);
                            this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + e.Code), Services.Models.NotificationSeverity.Success);
                            if (e.GetDoubleConfirm())
                            {
                                this.barcodeOk = e.Code;
                            }
                            else
                            {
                                await this.ConfirmOperationAsync(e.Code);
                            }
                        }
                    }

                    break;

                case UserAction.ConfirmOperation:
                    {
                        this.InputItemCode = e.GetItemCode() ?? this.InputItemCode;
                        this.IsItemCodeValid = this.InputItemCode is null || this.MissionOperation?.ItemCode is null || this.InputItemCode == this.MissionOperation.ItemCode;

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

                case UserAction.FilterItems:
                    await this.ShowItemDetailsByBarcodeAsync(e);

                    break;
            }
        }

        public async Task ConfirmOperationAsync(string barcode)
        {
            if (await this.MissionOperationsService.IsMultiMachineAsync(this.Mission.Id))
            {
                this.DialogService.ShowMessage(Localized.Get("OperatorApp.OperationMultiMachineInfo"), Localized.Get("OperatorApp.OperationConfirmed"), DialogType.Information, DialogButtons.OK);
            }

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
                ItemDetails item = null;
                if (this.MissionOperation.ItemId > 0)
                {
                    item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                }

                bool canComplete = false;
                var loadUnitId = this.Mission.LoadingUnit.Id;
                var itemId = this.MissionOperation.Id;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

                if (this.MissionOperation.Type == MissionOperationType.Inventory
                    && this.selectedCompartmentDetail?.InventoryThreshold.HasValue == true
                    && Math.Abs(this.selectedCompartmentDetail.Stock - quantity) > (double)this.selectedCompartmentDetail.InventoryThreshold.Value)
                {
                    var messageBoxResult = this.DialogService.ShowMessage(
                        Localized.Get("InstallationApp.ConfirmationOperation"),
                        Localized.Get("OperatorApp.InventoryGap"),
                        DialogType.Question,
                        DialogButtons.YesNo);
                    if (messageBoxResult is DialogResult.No)
                    {
                        this.IsBusyConfirmingOperation = false;
                        this.IsOperationConfirmed = false;
                        this.InputQuantity = null;
                        return;
                    }
                }

                var isRequestConfirm = await this.MachineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                if (isRequestConfirm)
                {
                    var isLastMissionOnCurrentLoadingUnit = await this.MissionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionOperation.Id);
                    if (isLastMissionOnCurrentLoadingUnit)
                    {
                        var messageBoxResult = this.DialogService.ShowMessage(
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            DialogType.Question,
                            DialogButtons.OK);
                        if (messageBoxResult is DialogResult.OK)
                        {
                            // go away...
                        }
                    }
                }

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght || this.MissionOperation.MaximumQuantity == decimal.One)
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
                    quantity = 1;
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
                }

                if (canComplete)
                {
                    if (item != null && itemId > 0 && quantity > 0)
                    {
                        await this.UpdateWeight(loadUnitId, quantity, item.AverageWeight, type);
                    }

                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.navigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "ConfirmOperationAsync");
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

            if (this.IsCurrentDraperyItem)
            {
                this.ShowDraperyItemConfirmView(
                    this.MissionOperation.ItemBarcode,
                    isPartiallyConfirmOperation: true);

                return;
            }

            try
            {
                this.IsBusyConfirmingPartialOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;
                bool canComplete;

                ItemDetails item = null;
                if (this.MissionOperation.ItemId > 0)
                {
                    item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                }
                var loadUnitId = this.Mission.LoadingUnit.Id;
                var itemId = this.MissionOperation.Id;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

                var isRequestConfirm = await this.MachineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                if (isRequestConfirm)
                {
                    var isLastMissionOnCurrentLoadingUnit = await this.MissionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionOperation.Id);
                    if (isLastMissionOnCurrentLoadingUnit)
                    {
                        var messageBoxResult = this.DialogService.ShowMessage(
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            DialogType.Question,
                            DialogButtons.OK);
                        if (messageBoxResult is DialogResult.OK)
                        {
                            // go away...
                        }
                    }
                }

                if (this.closeLine)
                {
                    canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, 0, null, this.emptyCompartment, this.fullCompartment);
                }
                else if (this.fullCompartment)
                {
                    var compartmentId = this.MissionOperation.CompartmentId;
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);

                    await this.compartmentsWebService.SetFillPercentageAsync(compartmentId, 100);
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                }

                if (canComplete)
                {
                    if (item != null && itemId > 0 && quantity > 0)
                    {
                        await this.UpdateWeight(loadUnitId, quantity, item.AverageWeight, type);
                    }
                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                }

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT,
                    "ConfirmPartialOperationAsync");
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

            this.currentItemIndex = 0;
            this.maxKnownIndexSelection = 0;
            this.products = new List<ItemInfo>();

            this.productsChangedToken?.Dispose();
            this.productsChangedToken = null;

            base.Disappear();
        }

        public ImageSource GenerateBarcodeSource(string barcodeString)
        {
            try
            {
                if (!string.IsNullOrEmpty(barcodeString)
                    && this.bay.ShowBarcodeImage
                    && (this.MissionOperation.Type == MissionOperationType.Inventory
                        || this.MissionOperation.Type == MissionOperationType.Put
                        || this.MissionOperation.Type == MissionOperationType.Pick))
                {
                    int width = 400;

                    if (barcodeString.Length >= 30)
                    {
                        width = 900;
                    }
                    else if (barcodeString.Length >= 20)
                    {
                        width = 800;
                    }
                    else if (barcodeString.Length >= 10)
                    {
                        width = 600;
                    }

                    if (!barcodeString.All(char.IsDigit))
                    {
                        width += 150;
                    }

                    var barcode = new BarcodeWriter
                    {
                        Format = BarcodeFormat.CODE_128,
                        Options = new ZXing.Common.EncodingOptions
                        {
                            Height = 100,
                            Width = width,
                            Margin = 10,
                        },
                    };

                    var image = barcode.Write(barcodeString);

                    var stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;
                    if (stream != null)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        this.BarcodeImageExist = true;

                        return bitmapImage;
                    }
                    else
                    {
                        this.BarcodeImageExist = false;
                        return new BitmapImage();
                    }
                }
                else
                {
                    this.BarcodeImageExist = false;
                    return new BitmapImage();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error("BarcodeImage Error: " + ex);
                this.BarcodeImageExist = false;
                return new BitmapImage();
            }
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
                this.NetWeight = this.lastItemQuantityMessage.NetWeight;
                this.Tare = this.lastItemQuantityMessage.Tare;
                this.UnitWeight = this.lastItemQuantityMessage.UnitWeight;

                //this.lastItemQuantityMessage = null;
            }
            else
            {
                this.NetWeight = 0;
                this.Tare = 0;
                this.UnitWeight = 0;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.ClearNotifications();

            this.IsBusyLoading = false;
            //string value = System.Configuration.ConfigurationManager.AppSettings["Box"];

            //this.IsBoxEnabled = value.ToLower() == "true" ? true : false;

            var machine = await this.machineConfigurationWebService.GetMachineAsync();
            this.IsBoxEnabled = machine.Box;

            this.IsDoubleConfirmBarcodePut = machine.IsDoubleConfirmBarcodePut;
            this.IsDoubleConfirmBarcodePick = machine.IsDoubleConfirmBarcodePick;
            this.barcodeOk = null;

            var disableQtyItemEditingPick = machine.IsDisableQtyItemEditingPick;
            this.IsEnableAvailableQtyItemEditingPick = !disableQtyItemEditingPick;
            this.MaxInputQuantity = double.MaxValue;

            //value = System.Configuration.ConfigurationManager.AppSettings["ItemUniqueIdLength"];

            //if (int.TryParse(value, out var def))
            //{
            //    this.BarcodeLenght = def;
            //}
            //else
            //{
            //    this.BarcodeLenght = 0;
            //}

            this.BarcodeLenght = machine.ItemUniqueIdLength;
            this.ToteBarcodeLength = machine.ToteBarcodeLength;

            var accessories = await this.accessoriesWebService.GetAllAsync();
            this.IsMinebeaScale = accessories.WeightingScale?.IsEnabledNew == true
                && accessories.WeightingScale?.DeviceInformation?.ModelNumber == WeightingScaleModelNumber.MinebeaIntec.ToString();

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
                        async (e) => await this.OnItemWeightChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            await base.OnAppearedAsync();

            await this.MissionOperationsService.RefreshAsync(force: true);
            await this.GetLoadingUnitDetailsAsync();

            this.productsChangedToken =
               this.productsChangedToken
               ??
               this.EventAggregator
                   .GetEvent<PubSubEvent<ProductsChangedEventArgs>>()
                   .Subscribe(async e => await this.OnProductsChangedAsync(e), ThreadOption.UIThread, false);
            await this.OnAppearItem();
        }

        public async Task PartiallyCompleteOnFullCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                bool canComplete;
                var loadUnitId = this.Mission.LoadingUnit.Id;
                var itemId = this.MissionOperation.Id;
                var quantity = this.InputQuantity;

                var isRequestConfirm = await this.MachineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                if (isRequestConfirm)
                {
                    var isLastMissionOnCurrentLoadingUnit = await this.MissionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionOperation.Id);
                    if (isLastMissionOnCurrentLoadingUnit)
                    {
                        var messageBoxResult = this.DialogService.ShowMessage(
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            DialogType.Question,
                            DialogButtons.OK);
                        if (messageBoxResult is DialogResult.OK)
                        {
                            // go away...
                        }
                    }
                }
                if (this.closeLine)
                {
                    canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, 0, null, this.emptyCompartment, this.fullCompartment);
                }
                else if (this.fullCompartment)
                {
                    await this.compartmentsWebService.SetFillPercentageAsync(this.MissionOperation.CompartmentId, 100);
                    canComplete = false;
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                }

                if (!canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "PartiallyCompleteOnFullCompartmentAsync");
                }
                else
                {
                    //await this.UpdateWeight(loadUnitId, this.InputQuantity.Value, item.AverageWeight, type);
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
            }
        }

        public async Task PrintWeightAsync(int id, int? quantity)
        {
            if (this.IsMinebeaScale)
            {
                this.logger.Debug($"PrintWeight id {id}; NetWeight {this.NetWeight:0.00}; Tare {this.Tare:0.00}, count {quantity}; UnitWeight {this.UnitWeight:0.00}");
                await this.itemsWebService.PrintWeightAsync(id, this.NetWeight, this.Tare, quantity, this.UnitWeight);
            }
        }

        public void Scroll(object parameter)
        {
            var scrollChangedEventArgs = parameter as ScrollChangedEventArgs;
            if (scrollChangedEventArgs != null)
            {
                var last = (int)scrollChangedEventArgs.VerticalOffset + (int)scrollChangedEventArgs.ViewportHeight;

                if (last > this.maxKnownIndexSelection)
                {
                    this.maxKnownIndexSelection = last;
                }

                if (last >= Math.Max((this.products.Count + 1) - ItemsToCheckBeforeLoad, DefaultPageSize - ItemsToCheckBeforeLoad - 1))
                {
                    this.IsSearching = true;
                    this.tokenSource = new CancellationTokenSource();
                    Task.Run(async () => await this.SearchItemAsync(last, this.tokenSource.Token).ConfigureAwait(false)).GetAwaiter().GetResult();
                }
            }
        }

        public async Task SearchItemAsync(int skip, CancellationToken cancellationToken)
        {
            if (!this.areaId.HasValue)
            {
                return;
            }

            if (skip == 0)
            {
                this.products.Clear();
                this.maxKnownIndexSelection = 0;
            }

            var selectedItemId = this.selectedProduct?.Id;

            try
            {
                var newItems = this.allProducts.Skip(skip).Take(DefaultPageSize);

                if (!newItems.Any())
                {
                    this.RaisePropertyChanged(nameof(this.Products));
                    return;
                }

                this.products.AddRange(newItems.Select(i => new ItemInfo(i, this.MachineService.Bay.Id)));

                if (this.products.Count == 1)
                {
                    this.SelectedProduct = this.products.FirstOrDefault();
                }
            }
            catch (TaskCanceledException)
            {
                // normal situation
                this.products.Clear();
                this.SelectedProduct = null;
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
            }
            catch (Exception ex)
            {
                //if (this.appear)
                //{
                //    this.ShowNotification(ex);
                //}

                this.products.Clear();
                this.SelectedProduct = null;
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
            }
            finally
            {
                this.IsSearching = false;
            }

            this.RaisePropertyChanged(nameof(this.Products));
            this.RaisePropertyChanged(nameof(this.SelectedProduct));

            this.SetCurrentIndex(selectedItemId);
            this.AdjustItemsAppearance();
        }

        public async Task SelectNextItemAsync()
        {
            if (this.currentItemIndex > this.maxKnownIndexSelection)
            {
                this.maxKnownIndexSelection = this.currentItemIndex;
            }
        }

        public async Task ShowItemDetailsByBarcodeAsync(UserActionEventArgs e)
        {
            var itemCode = e.GetItemCode();
            if (itemCode is null)
            {
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheItemCode"), e.Code),
                    Services.Models.NotificationSeverity.Warning);

                return;
            }

            try
            {
                this.ClearNotifications();

                var items = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    0,
                    1,
                    itemCode,
                    false,
                    false);

                if (items.Any())
                {
                    this.SearchItem = itemCode;
                    this.products.AddRange(items.Select(i => new ItemInfo(i, this.MachineService.Bay.Id)));
                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.ItemsFilteredByCode")), Services.Models.NotificationSeverity.Info);
                    //}
                }
                else
                {
                    try
                    {
                        var item = await this.itemsWebService.GetByBarcodeAsync(itemCode);
                        if (item is null)
                        {
                            this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), itemCode), Services.Models.NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.SearchItem = item.Code;
                            this.products.Add(new ItemInfo(item, this.MachineService.Bay.Id));
                            this.logger.Debug($"GetByBarcodeAsync '{item.Code}'.");

                            this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.ItemsFilteredByCode")), Services.Models.NotificationSeverity.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), itemCode), Services.Models.NotificationSeverity.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Products));
            }
        }

        public async Task UpdateWeight(int loadingUnitId, double quantity, int? itemWeight, MissionOperationType missionOperationType)
        {
            if (itemWeight != null && itemWeight != 0)
            {
                var loadingUnit = await this.loadingUnitsWebService.GetByIdAsync(loadingUnitId);

                var grossWeight = default(double);
                if (missionOperationType == MissionOperationType.Put)
                {
                    grossWeight = loadingUnit.GrossWeight + (itemWeight.Value * quantity / 1000);
                }
                else if (missionOperationType == MissionOperationType.Pick)
                {
                    grossWeight = loadingUnit.GrossWeight - (itemWeight.Value * quantity / 1000);
                }
                else
                {
                    return;
                }

                this.logger.Debug($"Set weight {grossWeight:0.00} to LoadUnit {loadingUnitId} difference {quantity} unit weight {itemWeight.Value} original weight {loadingUnit.GrossWeight:0.00}");
                await this.loadingUnitsWebService.SetLoadingUnitWeightAsync(loadingUnitId, grossWeight);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.addItemCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.confirmItemOperationCommand?.RaiseCanExecuteChanged();
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

        private void AdjustItemsAppearance()
        {
            try
            {
                if (this.maxKnownIndexSelection == 0)
                {
                    this.maxKnownIndexSelection = Math.Min(this.products.Count, ItemsVisiblePageSize) - 1;
                }

                if (this.maxKnownIndexSelection >= ItemsVisiblePageSize
                    &&
                    this.Products.Count >= this.maxKnownIndexSelection)
                {
                    this.SelectedProduct = this.products?.ElementAtOrDefault(this.maxKnownIndexSelection);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanConfirmItemOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                //this.SelectedProduct != null   // actually the product is not selected
                //&&
                !this.IsBusyConfirmingOperation;
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
                !this.isOperationCanceled
                && !(this.IsDoubleConfirmBarcodePick && string.IsNullOrEmpty(this.barcodeOk));
            return this.CanConfirmPresent;
        }

        private bool CanLoadItems()
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
                        this.CanInputQuantity;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanOpenSignallingDefect()
        {
            return this.IsCurrentDraperyItem;
        }

        private async Task ConfirmItemOperationAsync()
        {
            // Note:
            // The add item operation to loading unit is based only the barcode value (for the item) given by the user.
            // No one product is selected in the grid items (the grid items is not visible).
            //
            // TODO: insert code to handle the generic (manual) add operation to loading unit
            //

            if (string.IsNullOrEmpty(this.SearchItem))
            {
                this.Logger.Debug($"Invalid search item - barcode value");

                this.ShowNotification(Localized.Get("OperatorApp.InvalidArgument"), Services.Models.NotificationSeverity.Error);
                return;
            }

            this.IsWaitingForResponse = true;

            if (this.SearchItem != null)
            {
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var barcode = this.SearchItem;

                try
                {
                    this.Logger.Debug($"Insert drapery barcode {barcode} into loading unit Id {loadingUnitId}");

                    var draperyItemInfoList = await this.loadingUnitsWebService.LoadDraperyItemInfoAsync(loadingUnitId, barcode);

                    if (draperyItemInfoList != null)
                    {
                        var draperyItemInfo = draperyItemInfoList.First();

                        this.Logger.Debug($"Show the adding view for drapery item [description: {draperyItemInfo.Description}] into loading unit {loadingUnitId}");

                        this.navigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.ADD_DRAPERYITEM_INTO_LOADINGUNIT,
                            draperyItemInfo,
                            trackCurrentView: true);
                    }
                    else
                    {
                        this.Logger.Error($"An error occurs");
                        this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), " "), Services.Models.NotificationSeverity.Error);
                    }
                }
                catch
                {
                    this.Logger.Error($"Invalid operation performed.");
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), " "), Services.Models.NotificationSeverity.Error);
                }
            }

            this.IsWaitingForResponse = false;
        }

        private async Task ConfirmPresentOperationAsync()
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

                await this.wmsDataProvider.UpdateItemStockAsync(
                        this.selectedCompartment.Id,
                        this.selectedCompartmentDetail.ItemId.Value,
                        this.availableQuantity.Value,
                        reasons.First().Id,
                        "update present quantity",
                        this.selectedCompartmentDetail.Lot,
                        this.selectedCompartmentDetail.ItemSerialNumber,
                        this.authenticationService.UserName);

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
            catch (InvalidOperationException exc)
            {
                this.ShowNotification(new Exception(exc.Message));
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

                this.Compartments = MapCompartments(this.Mission.LoadingUnit.Compartments).ToList();

                try
                {
                    var loadingUnitId = this.Mission?.LoadingUnit?.Id;
                    if (this.Compartments != null)
                    {
                        this.SelectedCompartment = this.Compartments.SingleOrDefault(c =>
                            c.Id == this.MissionOperation.CompartmentId);
                        //var unit = await this.missionOperationsWebService.GetUnitIdAsync(this.Mission.Id);
                        var itemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(loadingUnitId ?? 0);
                        itemsCompartments = itemsCompartments?.Where(ic => !(ic.ItemId is null));
                        this.SelectedCompartmentDetail = itemsCompartments.FirstOrDefault(s => s.Id == this.selectedCompartment?.Id
                            && s.ItemId == (this.MissionOperation?.ItemId ?? 0)
                            && (string.IsNullOrEmpty(this.MissionOperation?.Lot) || this.MissionOperation?.Lot == "*" || s.Lot == this.MissionOperation?.Lot)
                            && (string.IsNullOrEmpty(this.MissionOperation?.SerialNumber) || this.MissionOperation?.SerialNumber == "*" || s.ItemSerialNumber == this.MissionOperation?.SerialNumber)
                            );
                        this.AvailableQuantity = this.selectedCompartmentDetail?.Stock;
                    }
                }
                catch (Exception)
                {
                    //this.CanInputAvailableQuantity = true;
                    this.CanInputAvailableQuantity = this.IsEnableAvailableQtyItemEditingPick;
                    this.CanInputQuantity = true;
                    this.AvailableQuantity = null;
                }

                try
                {
                    if (this.MissionOperation != null && this.MissionOperation.ItemId > 0)
                    {
                        var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                        this.IsCurrentDraperyItem = item.IsDraperyItem; // check if current item is a drapery item
                    }
                    else
                    {
                        this.IsCurrentDraperyItem = false;
                    }
                }
                catch (Exception)
                {
                    this.IsCurrentDraperyItem = false;
                }
            }

            //this.CanInputAvailableQuantity = true;
            this.CanInputAvailableQuantity = this.IsEnableAvailableQtyItemEditingPick;
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

                case MissionOperationType.LoadingUnitCheck:
                    noLongerOperationMsg = Localized.Get("OperatorApp.CheckOperationCancelled");
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

        private async Task OnAppearItem()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();

            this.RaisePropertyChanged(nameof(this.Products));

            this.IsBusyLoading = true;
        }

        private async Task OnItemWeightChangedAsync(ItemWeightChangedMessage itemWeightChanged)
        {
            this.lastItemQuantityMessage = itemWeightChanged;
            this.InitializeInputQuantity();
            var mission = this.MissionOperationsService.ActiveWmsOperation;
            if (mission != null)
            {
                await this.PrintWeightAsync(mission.Id, (int?)this.InputQuantity);
            }
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

        private async Task OnProductsChangedAsync(ProductsChangedEventArgs e)
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();
        }

        private async Task RefreshItemsAsync()
        {
            var startIndex = ((this.maxKnownIndexSelection - ItemsVisiblePageSize) > 0) ? this.maxKnownIndexSelection - ItemsVisiblePageSize : 0;
            this.currentItemIndex = startIndex;

            this.tokenSource = new CancellationTokenSource();
            await this.SearchItemAsync(startIndex, this.tokenSource.Token);
        }

        private async Task ReloadAllItems(CancellationToken cancellationToken)
        {
            this.allProducts = new List<ProductInMachine>();
            try
            {
                if (this.areaId is null)
                {
                    var machineIdentity = await this.machineIdentityWebService.GetAsync();
                    this.areaId = machineIdentity.AreaId;
                }

                var totalProducts = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    0,
                    0,
                    this.searchItem,
                    false,
                    true,
                    cancellationToken);

                this.allProducts.AddRange(totalProducts);
            }
            catch (TaskCanceledException)
            {
                // normal situation
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void ResetInputFields()
        {
            this.InputSerialNumber = null;
            this.InputLot = null;
            this.InputItemCode = null;
            this.IsItemCodeValid = false;
            this.InputQuantity = this.MissionRequestedQuantity;
            //this.AvailableQuantity = this.MissionRequestedQuantity; //to fix
        }

        private void SetCurrentIndex(int? itemId)
        {
            if (itemId.HasValue
                &&
                this.products.FirstOrDefault(i => i.Id == itemId.Value) is ItemInfo itemFound)
            {
                this.currentItemIndex = this.products.IndexOf(itemFound) + 1;
            }
            else
            {
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
            }
        }

        private void ShowDraperyItemConfirmView(string barcode, bool isPartiallyConfirmOperation)
        {
            this.Logger.Debug($"Show the confirm view for drapery item {this.ItemId}, description {this.MissionOperation.ItemDescription}");

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.DRAPERYCONFIRM,
                new ItemDraperyDataConfirm
                {
                    MissionId = this.MissionOperation.Id,
                    ItemId = this.MissionOperation.ItemId,
                    MissionOperationType = this.MissionOperation.Type,
                    LoadingUnitId = this.Mission.LoadingUnit.Id,
                    ItemDescription = this.MissionOperation.ItemDescription,
                    AvailableQuantity = this.AvailableQuantity,
                    MissionRequestedQuantity = this.MissionRequestedQuantity,
                    InputQuantity = this.InputQuantity,
                    CanInputQuantity = this.CanInputQuantity,
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = string.Format(Localized.Get("OperatorApp.PickedQuantity"), ""),
                    Barcode = barcode,
                    BarcodeLength = this.BarcodeLenght,
                    IsPartiallyCompleteOperation = isPartiallyConfirmOperation,
                    FullyRequested = this.IsCurrentDraperyItem && this.MissionOperation.FullyRequested.HasValue && this.MissionOperation.FullyRequested.Value,
                    CloseLine = this.closeLine,
                },
                trackCurrentView: true);
        }

        /// <summary>
        /// Show details for items via barcode data.
        /// Only reserved for drapery items management.
        /// </summary>
        private async Task ShowItemDetailsByBarcode_DraperyItemStuff_Async(string itemCode)
        {
            if (itemCode is null)
            {
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheItemCode"), itemCode),
                    Services.Models.NotificationSeverity.Warning);

                return;
            }

            try
            {
                this.ClearNotifications();

                var draperyExists = await this.itemsWebService.IsDraperyExistByDraperyIdBarcodeAsync(itemCode);

                if (!draperyExists)
                {
                    this.SearchItem = itemCode;
                }
                else
                {
                    this.SearchItem = string.Empty;
                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.DraperyWithCodeAlreadyAvailable"), itemCode),
                        Services.Models.NotificationSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Products));
            }
        }

        private void SignallingDefect()
        {
            this.Logger.Debug("Signalling defect call....");

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.SIGNALLINGDEFECT,
                this.MissionOperation);
        }

        private async Task TriggerSearchAsync()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            try
            {
                const int callDelayMilliseconds = 500;

                await Task.Delay(callDelayMilliseconds, this.tokenSource.Token);
                await this.ReloadAllItems(this.tokenSource.Token);
                await this.SearchItemAsync(0, this.tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
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

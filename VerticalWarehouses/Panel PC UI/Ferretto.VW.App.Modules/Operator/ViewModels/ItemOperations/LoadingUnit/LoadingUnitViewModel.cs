using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseLoadingUnitViewModel, IOperationReasonsSelector, IOperationalContextViewModel
    {
        #region Fields

        private const int DefaultPageSize = 20;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 3;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly ILaserPointerService deviceService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IOperatorNavigationService operatorNavigationService;

        //x private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private List<ProductInMachine> allProducts = new List<ProductInMachine>();

        private int? areaId;

        private DelegateCommand cancelReasonCommand;

        private bool canInputQuantity;

        private bool chargeItemTextViewVisibility;

        private int confirmButtonColumnIndexPosition;

        private DelegateCommand confirmItemOperationCommand;

        private DelegateCommand confirmOperationCommand;

        private DelegateCommand confirmReasonCommand;

        private int currentItemIndex;

        private string inputBoxCode;

        private double? inputQuantity;

        private string inputQuantityInfo;

        private DelegateCommand insertOperationCommand;

        private bool isAddItemFeatureForDraperyManagementAvailable;

        private bool isAddItemVisible;

        private bool isAdjustmentVisible;

        private bool isBoxOperationVisible;

        private bool isBusyLoading;

        private bool isOperationVisible;

        private bool isPickVisible;

        private bool isPutVisible;

        private bool isSearching;

        private string itemSearchKeyTitleName;

        private SubscriptionToken itemWeightToken;

        private ItemWeightChangedMessage lastItemQuantityMessage;

        private int maxKnownIndexSelection;

        private string measureUnit;

        private DelegateCommand<string> operationCommand;

        private bool orderEnable;

        private int? orderId;

        private IEnumerable<OperationReason> orders;

        private List<ItemInfo> products = new List<ItemInfo>();

        private SubscriptionToken productsChangedToken;

        private bool productsDataGridViewVisibility;

        private double quantityIncrement;

        private int? quantityTolerance;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand recallLoadingUnitCommand;

        private DelegateCommand removeOperationCommand;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private string selectedItemTxt;

        private ItemInfo selectedProduct;

        private object socketLinkOperationToken;

        private CancellationTokenSource tokenSource;

        private double? unitHeight;

        private int? unitNumber;

        private double? unitWeight;

        private DelegateCommand weightCommand;

        private MissionOperation weightMission;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
            ILaserPointerService deviceService,
            IMachineIdentityWebService identityService,
            IMachineAreasWebService areasWebService,
            IMachineIdentityWebService machineIdentityWebService,
            INavigationService navigationService,
            IMachineItemsWebService itemsWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineService machineService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            IOperatorNavigationService operatorNavigationService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            ILaserPointerDriver laserPointerDriver,
            ISessionService sessionService,
            IWmsDataProvider wmsDataProvider,
            IAuthenticationService authenticationService)
            : base(machineIdentityWebService, machineLoadingUnitsWebService, missionOperationsService, eventAggregator, bayManager, laserPointerDriver, sessionService, wmsDataProvider)
        {
            this.deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.compartmentsWebService = compartmentsWebService ?? throw new ArgumentNullException(nameof(compartmentsWebService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => this.IsWaitingForReason ? OperationalContext.NoteDescription.ToString() : OperationalContext.ItemInventory.ToString();

        public ICommand CancelReasonCommand =>
            this.cancelReasonCommand
            ??
            (this.cancelReasonCommand = new DelegateCommand(
                this.CancelReason));

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            set => this.SetProperty(ref this.canInputQuantity, value);
        }

        public bool ChargeItemTextViewVisibility
        {
            get => this.chargeItemTextViewVisibility;
            set => this.SetProperty(ref this.chargeItemTextViewVisibility, value, this.RaiseCanExecuteChanged);
        }

        public int ConfirmButtonColumnIndexPosition
        {
            get => this.confirmButtonColumnIndexPosition;
            set => this.SetProperty(ref this.confirmButtonColumnIndexPosition, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ConfirmItemOperationCommand =>
                            this.confirmItemOperationCommand
            ??
            (this.confirmItemOperationCommand = new DelegateCommand(
                async () => await this.ConfirmItemOperationAsync(), this.CanConfirmItemOperation));

        public ICommand ConfirmOperationCommand =>
                    this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(), this.CanConfirmOperation));

        public ICommand ConfirmReasonCommand =>
                          this.confirmReasonCommand
          ??
          (this.confirmReasonCommand = new DelegateCommand(
              async () => await this.ExecuteOperationAsync(),
              this.CanExecuteItemPick));

        public string InputBoxCode
        {
            get => this.inputBoxCode;
            set => this.SetProperty(ref this.inputBoxCode, value);
        }

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public string InputQuantityInfo
        {
            get => this.inputQuantityInfo;
            set => this.SetProperty(ref this.inputQuantityInfo, value);
        }

        public ICommand InsertOperationCommand =>
            this.insertOperationCommand
            ??
            (this.insertOperationCommand = new DelegateCommand(
                async () => await this.BoxOperationAsync(), this.CanInsertOperation));

        /// <summary>
        /// Gets or sets a value indicating whether it makes visible the 'Add' button according to a well-defined configuration machine parameter.
        /// </summary>
        public bool IsAddItemFeatureForDraperyManagementAvailable
        {
            get => this.isAddItemFeatureForDraperyManagementAvailable;
            set => this.SetProperty(ref this.isAddItemFeatureForDraperyManagementAvailable, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAddItemVisible
        {
            get => this.isAddItemVisible;
            set
            {
                if (this.SetProperty(ref this.isAddItemVisible, value) && value)
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
            set
            {
                if (this.SetProperty(ref this.isAdjustmentVisible, value) && value)
                {
                    this.IsPickVisible = false;
                    this.IsPutVisible = false;
                    this.IsBoxOperationVisible = false;
                    this.IsAddItemVisible = false;
                }
            }
        }

        public bool IsBoxOperationVisible
        {
            get => this.isBoxOperationVisible;
            set
            {
                if (this.SetProperty(ref this.isBoxOperationVisible, value && this.IsBoxEnabled) && value)
                {
                    this.IsPickVisible = false;
                    this.IsPutVisible = false;
                    this.IsAdjustmentVisible = false;
                    this.IsAddItemVisible = false;
                }
            }
        }

        public bool IsBusyLoading
        {
            get => this.isBusyLoading;
            set => this.SetProperty(ref this.isBusyLoading, value, this.RaiseCanExecuteChanged);
        }

        public bool IsItemStockVisible => this.isPickVisible || this.isPutVisible;

        public bool IsOperationVisible
        {
            get => this.isOperationVisible;
            set => this.SetProperty(ref this.isOperationVisible, value);
        }

        public bool IsPickVisible
        {
            get => this.isPickVisible;
            set
            {
                if (this.SetProperty(ref this.isPickVisible, value) && value)
                {
                    this.IsPutVisible = false;
                    this.IsAdjustmentVisible = false;
                    this.IsBoxOperationVisible = false;
                    this.IsAddItemVisible = false;
                }
            }
        }

        public bool IsPutVisible
        {
            get => this.isPutVisible;
            set
            {
                if (this.SetProperty(ref this.isPutVisible, value) && value)
                {
                    this.IsPickVisible = false;
                    this.IsAdjustmentVisible = false;
                    this.IsBoxOperationVisible = false;
                    this.IsAddItemVisible = false;
                }
            }
        }

        public bool IsSearching
        {
            get => this.isSearching;
            set => this.SetProperty(ref this.isSearching, value, this.RaiseCanExecuteChanged);
        }

        public bool IsWaitingForReason { get; private set; }

        public string ItemSearchKeyTitleName
        {
            get => this.itemSearchKeyTitleName;
            set => this.SetProperty(ref this.itemSearchKeyTitleName, value, this.RaiseCanExecuteChanged);
        }

        public string MeasureUnit
        {
            get => this.measureUnit;
            set => this.SetProperty(ref this.measureUnit, value);
        }

        public ICommand OperationCommand =>
            this.operationCommand
            ??
            (this.operationCommand = new DelegateCommand<string>(
                async (param) => await this.ToggleOperation(param), this.CanDoOperation));

        public bool OrderEnable
        {
            get => this.orderEnable;
            set => this.SetProperty(ref this.orderEnable, value);
        }

        public int? OrderId
        {
            get => this.orderId;
            set => this.SetProperty(ref this.orderId, value, this.RaiseCanExecuteChanged);
        }

        public IEnumerable<OperationReason> Orders
        {
            get => this.orders;
            set => this.SetProperty(ref this.orders, value);
        }

        public IList<ItemInfo> Products => new List<ItemInfo>(this.products);

        public bool ProductsDataGridViewVisibility
        {
            get => this.productsDataGridViewVisibility;
            set => this.SetProperty(ref this.productsDataGridViewVisibility, value, this.RaiseCanExecuteChanged);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
                }
            }
        }

        public int? ReasonId
        {
            get => this.reasonId;
            set => this.SetProperty(ref this.reasonId, value, this.RaiseCanExecuteChanged);
        }

        public string ReasonNotes
        {
            get => this.reasonNotes;
            set => this.SetProperty(ref this.reasonNotes, value);
        }

        public IEnumerable<OperationReason> Reasons
        {
            get => this.reasons;
            set => this.SetProperty(ref this.reasons, value);
        }

        public ICommand RecallLoadingUnitCommand =>
            this.recallLoadingUnitCommand
            ??
            (this.recallLoadingUnitCommand = new DelegateCommand(
                async () => await this.RecallLoadingUnitAsync(),
                this.CanRecallLoadingUnit));

        public ICommand RemoveOperationCommand =>
            this.removeOperationCommand
            ??
            (this.removeOperationCommand = new DelegateCommand(
                async () => await this.BoxOperationAsync(), this.CanRemoveOperation));

        public ICommand ScrollCommand => this.scrollCommand ?? (this.scrollCommand = new DelegateCommand<object>((arg) => this.Scroll(arg)));

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value))
                {
                    this.IsSearching = true;

                    // Perform the searching routine (only if not drapery management)
                    if (!this.IsAddItemFeatureForDraperyManagementAvailable)
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            this.selectedProduct = null;
                            this.RaisePropertyChanged(nameof(this.SelectedProduct));
                        }

                        // Searching routine
                        this.TriggerSearchAsync().GetAwaiter();
                    }
                }
            }
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

        public double? UnitHeight
        {
            get => this.unitHeight;
            set => this.SetProperty(ref this.unitHeight, value, this.RaiseCanExecuteChanged);
        }

        public int? UnitNumber
        {
            get => this.unitNumber;
            set => this.SetProperty(ref this.unitNumber, value, this.RaiseCanExecuteChanged);
        }

        public double? UnitWeight
        {
            get => this.unitWeight;
            set => this.SetProperty(ref this.unitWeight, value, this.RaiseCanExecuteChanged);
        }

        public ICommand WeightCommand =>
                                                                                                                                                                                                                                                                                                                                                                                    this.weightCommand
            ??
            (this.weightCommand = new DelegateCommand(
                () => this.Weight(),
                this.CanOpenWeightPage));

        #endregion

        #region Methods

        public bool CanOpenWeightPage()
        {
            return this.MachineService.Bay.Accessories?.WeightingScale is null ? false : this.MachineService.Bay.Accessories.WeightingScale.IsEnabledNew &&
                this.MissionOperationsService.ActiveWmsOperation != null;
        }

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;
            this.OrderId = null;

            try
            {
                this.IsBusyConfirmingOperation = true;

                var missionOperationType = MissionOperationType.NotSpecified;
                if (this.IsPickVisible)
                {
                    missionOperationType = MissionOperationType.Pick;
                }
                else if (this.IsPutVisible)
                {
                    missionOperationType = MissionOperationType.Put;
                }
                else if (this.IsAdjustmentVisible)
                {
                    missionOperationType = (this.InputQuantity > this.SelectedItem.Stock) ? MissionOperationType.Positive : MissionOperationType.Negative;
                    this.Orders = await this.missionOperationsWebService.GetOrdersAsync();
                    this.OrderId = 0;
                    this.OrderEnable = this.orders?.Any(o => o.Name.Length > 0) == true;
                    if (this.OrderEnable)
                    {
                        this.OrderId = this.orders.First(o => o.Name.Length > 0).Id;
                    }
                }

                if (missionOperationType != MissionOperationType.NotSpecified)
                {
                    this.ReasonNotes = null;
                    this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(missionOperationType);
                }

                if (this.reasons?.Any() == true)
                {
                    if (this.reasons.Count() == 1)
                    {
                        this.ReasonId = this.reasons.First().Id;
                    }
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.Reasons = null;
                this.Orders = null;
            }
            finally
            {
                this.IsBusyConfirmingOperation = false;
            }

            return this.Reasons?.Any() == true;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (this.IsAddItemVisible)
            {
                var bIsAddItemParameterConfigured = await this.identityService.IsEnableAddItemAsync();

                // Check the existence of drapery item for the adding operation
                if (bIsAddItemParameterConfigured)
                {
                    await this.ShowItemDetailsByBarcode_DraperyItemStuff_Async(userAction);
                    return;
                }

                await this.ShowItemDetailsByBarcodeAsync(userAction);
                return;
            }

            if (this.IsBoxOperationVisible && userAction.UserAction == UserAction.VerifyItem)
            {
                this.InputBoxCode = userAction.Code;
                return;
            }

            if (this.IsWaitingForReason && userAction.UserAction == UserAction.Notes)
            {
                this.ReasonNotes = userAction.Code;
                await this.ExecuteOperationAsync();
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            this.currentItemIndex = 0;
            this.maxKnownIndexSelection = 0;
            this.products = new List<ItemInfo>();

            this.productsChangedToken?.Dispose();
            this.productsChangedToken = null;

            this.deviceService.ResetPoint();
        }

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                var count = 0;

                var moveUnitId = await this.machineMissionsWebService.GetAllUnitGoBayAllAsync();

                if (moveUnitId != null)
                {
                    foreach (var unit in moveUnitId)
                    {
                        count++;
                    }
                }

                var moveUnitIdToCell = await this.machineMissionsWebService.GetAllUnitGoCellAllAsync();

                if (moveUnitIdToCell != null)
                {
                    var userdifference = moveUnitIdToCell.Except(moveUnitId);

                    if (userdifference.Any())
                    {
                        foreach (var units in userdifference)
                        {
                            var selectedunit = this.machineService.Loadunits.Where(i => i.Id == units).SingleOrDefault();
                            this.unitNumber = selectedunit.Id;
                            this.unitHeight = selectedunit.Height;
                            this.unitWeight = selectedunit.NetWeight;
                        }
                    }
                    else
                    {
                        this.unitNumber = this.LoadingUnit.Id;
                        this.unitHeight = this.LoadingUnit.Height;
                        this.unitWeight = this.LoadingUnit.NetWeight;
                    }
                }
                else
                {
                    this.unitNumber = this.LoadingUnit.Id;
                    this.unitHeight = this.LoadingUnit.Height;
                    this.unitWeight = this.LoadingUnit.NetWeight;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.unitNumber));
                this.RaisePropertyChanged(nameof(this.unitHeight));
                this.RaisePropertyChanged(nameof(this.unitWeight));
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBusyLoading = false;
            this.ProductsDataGridViewVisibility = this.isBusyLoading && !this.IsAddItemFeatureForDraperyManagementAvailable;
            await this.GetSocketLinkOperation();

            this.socketLinkOperationToken = this.socketLinkOperationToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SocketLinkOperationChangeMessageData>>()
                    .Subscribe(
                        this.OnSocketLinkOperationChanged,
                        ThreadOption.UIThread,
                        false);

            await base.OnAppearedAsync();

            if (!this.CheckUDC())
            {
                return;
            }

            this.itemWeightToken = this.itemWeightToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ItemWeightChangedMessage>>()
                    .Subscribe(
                        (e) => this.OnItemWeightChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.productsChangedToken =
              this.productsChangedToken
              ??
              this.EventAggregator
                  .GetEvent<PubSubEvent<ProductsChangedEventArgs>>()
                  .Subscribe(async e => await this.OnProductsChangedAsync(e), ThreadOption.UIThread, false);

            await this.OnAppearItem();

            this.IsAddItemFeatureForDraperyManagementAvailable = await this.identityService.IsEnableAddItemAsync();

            // Update UI according to normal configuration or drapery management configuration
            this.ItemSearchKeyTitleName = Localized.Get(OperatorApp.ItemSearchKeySearch);
            this.ConfirmButtonColumnIndexPosition = 1;
            if (this.IsAddItemFeatureForDraperyManagementAvailable)
            {
                this.ItemSearchKeyTitleName = Localized.Get(OperatorApp.BarcodeLabel);
                this.SearchItem = string.Empty;
                this.ConfirmButtonColumnIndexPosition = 0;
            }

            this.Reasons = null;
            this.Orders = null;
            this.IsWaitingForReason = false;

            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(500);
                    await this.GetLoadingUnitsAsync();
                }
                while (this.IsVisible);
            });

            if (this.lastItemQuantityMessage != null)
            {
                await this.ToggleOperation(OperatorApp.Adjustment);
            }
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();

            this.RaisePropertyChanged(nameof(this.ConfirmOperationInfo));
            this.RaisePropertyChanged(nameof(this.IsItemStockVisible));
            this.RaisePropertyChanged(nameof(this.IsBoxOperationVisible));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsBusyConfirmingRecallOperation = true;
                this.IsWaitingForResponse = true;

                var activeOperation = this.MissionOperationsService.ActiveWmsOperation;
                this.Logger.Debug($"User requested recall of loading unit.");

                if (activeOperation != null)
                {
                    var quantity = this.ItemsCompartments.FirstOrDefault(ic => ic.Id == activeOperation.CompartmentId
                        && ic.ItemId == activeOperation.ItemId
                        && (activeOperation.Lot == null || activeOperation.Lot == "*" || ic.Lot == activeOperation.Lot)
                        && (activeOperation.SerialNumber == null || activeOperation.SerialNumber == "*" || ic.ItemSerialNumber == activeOperation.SerialNumber)
                        )?.Stock ?? activeOperation.RequestedQuantity;

                    var canComplete = await this.MissionOperationsService.CompleteAsync(activeOperation.Id, quantity);
                    if (!canComplete)
                    {
                        this.Logger.Debug($"Operation '{activeOperation.Id}' cannot be completed, forcing recall of loading unit.");

                        await this.MissionOperationsService.RecallLoadingUnitAsync(this.LoadingUnit.Id);
                    }
                }
                else
                {
                    await this.MissionOperationsService.RecallLoadingUnitAsync(this.LoadingUnit.Id);
                }

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT,
                    "RecallLoadingUnitAsync");

                this.Reset();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    this.ShowNotification(Resources.Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsBusyConfirmingRecallOperation = false;
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

                this.products.AddRange(newItems.Select(i => new ItemInfo(i, this.machineService.Bay.Id)));

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

        protected override async Task OnMachineModeChangedAsync(MAS.AutomationService.Contracts.Hubs.MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void OnSelectedCompartmentChanged()
        {
            this.HideOperation();
            this.InputBoxCode = "";
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.weightCommand?.RaiseCanExecuteChanged();
            this.operationCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.confirmItemOperationCommand?.RaiseCanExecuteChanged();
            this.recallLoadingUnitCommand?.RaiseCanExecuteChanged();
            this.confirmReasonCommand?.RaiseCanExecuteChanged();
            this.insertOperationCommand?.RaiseCanExecuteChanged();
            this.removeOperationCommand?.RaiseCanExecuteChanged();
        }

        private async Task AddItemOperationAsync()
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
                var loadingUnitId = this.LoadingUnit.Id;
                var selectedItemId = this.SelectedProduct?.Id;
                //var compartmentId = this.SelectedItemCompartment.Id;
                var compartmentId = this.SelectedCompartmentForImmediateAdding != null ? this.SelectedCompartmentForImmediateAdding.Id : -1;
                var item = await this.itemsWebService.GetByIdAsync(selectedItemId.Value);

                var itemAddedToLoadingUnitInfo = new ItemAddedToLoadingUnitDetail
                {
                    ItemId = selectedItemId.Value,
                    LoadingUnitId = loadingUnitId,
                    CompartmentId = compartmentId,
                    ItemDescription = item.Description,
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = string.Empty,
                };

                // Show the view to adding item into current loading unit
                this.navigationService.Appear(
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

        private async Task AddItemOperationDraperyManagementAsync()
        {
            // Note:
            // The add item operation to loading unit for drapery is based only the barcode value (for the item/drapery) given by the user.
            // No one product is selected in the grid items (the grid items is not visible).
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
                var loadingUnitId = this.LoadingUnit.Id;
                var barcode = this.SearchItem;

                try
                {
                    this.Logger.Debug($"Insert drapery barcode {barcode} into loading unit Id {loadingUnitId}");

                    var draperyItemInfoList = await this.LoadingUnitsWebService.LoadDraperyItemInfoAsync(loadingUnitId, barcode);
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

        private async Task BoxOperationAsync()
        {
            this.IsWaitingForResponse = true;

            if (this.CanInsertOperation())
            {
                if (this.InputBoxCode.Contains("VM"))
                {
                    await this.ExecuteOperationAsync(this.InputBoxCode, 1);
                }
                else
                {
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.BarcodeNotRecognized"), this.InputBoxCode), Services.Models.NotificationSeverity.Warning);
                }
            }
            else if (this.CanRemoveOperation())
            {
                await this.ExecuteOperationAsync(this.SelectedCompartment.Barcode, 2);
            }
        }

        private void CancelReason()
        {
            this.Reasons = null;
            this.Orders = null;
            this.IsBusyConfirmingOperation = false;
            this.IsWaitingForResponse = false;
            this.IsWaitingForReason = false;
        }

        private bool CanConfirmItemOperation()
        {
            var retValue = this.IsWmsEnabledAndHealthy &&
                !this.IsWaitingForResponse &&
                !this.IsBusyConfirmingRecallOperation &&
                !this.IsBusyConfirmingOperation;

            if (!this.IsAddItemFeatureForDraperyManagementAvailable)
            {
                // check if selected product is valid
                retValue = retValue && (this.SelectedProduct != null);
            }

            return retValue;
        }

        private bool CanConfirmOperation()
        {
            return
                this.IsWmsEnabledAndHealthy
                &&
                !this.IsWaitingForResponse
                &&
                this.InputQuantity.HasValue
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CanDoOperation(string param)
        {
            if (param == OperatorApp.Box)
            {
                return
                    this.SelectedCompartment != null
                    &&
                    !this.IsWaitingForResponse
                    &&
                    !this.IsBusyConfirmingRecallOperation
                    &&
                    !this.IsBusyConfirmingOperation
                    &&
                    this.IsWmsHealthy;
            }
            else if (param == OperatorApp.Add)
            {
                return
                    !this.IsWaitingForResponse
                    &&
                    !this.IsBusyConfirmingRecallOperation
                    &&
                    !this.IsBusyConfirmingOperation
                    &&
                    this.IsWmsHealthy;
            }

            return
                this.SelectedItemCompartment?.ItemId != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                this.IsWmsHealthy;
        }

        private bool CanExecuteItemPick()
        {
            return !(this.reasonId is null);
        }

        private bool CanInsertOperation()
        {
            return
                this.SelectedCompartment != null
                &&
                this.IsWmsEnabledAndHealthy
                &&
                string.IsNullOrEmpty(this.SelectedCompartment.Barcode)
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CanRecallLoadingUnit()
        {
            return
                !this.IsBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !(this.LoadingUnit is null);
        }

        private bool CanRemoveOperation()
        {
            return
                this.SelectedCompartment != null
                &&
                this.IsWmsEnabledAndHealthy
                &&
                !string.IsNullOrEmpty(this.SelectedCompartment.Barcode)
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CheckUDC()
        {
            try
            {
                var activeOperation = this.MissionOperationsService.ActiveWmsOperation;

                // if (activeOperation != null && activeOperation.CompartmentId != null && activeOperation.CompartmentId > 0)
                if (activeOperation != null && activeOperation.CompartmentId > 0 && activeOperation.ItemId > 0)
                {
                    this.SelectedItemCompartment = this.ItemsCompartments.FirstOrDefault(s => s.Id == activeOperation.CompartmentId && s.ItemId == activeOperation.ItemId
                        && (activeOperation.Lot == null || activeOperation.Lot == "*" || s.Lot == activeOperation.Lot)
                        && (activeOperation.SerialNumber == null || activeOperation.SerialNumber == "*" || s.ItemSerialNumber == activeOperation.SerialNumber));
                    this.RaisePropertyChanged(nameof(this.SelectedItemCompartment));
                }
                else if (!this.MachineService.Loadunits.Any(l => l.Id == this.LoadingUnit?.Id && l.Status == LoadingUnitStatus.InBay))
                {
                    this.navigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                       null,
                       trackCurrentView: false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            return true;
        }

        private async Task ConfirmItemOperationAsync()
        {
            if (this.IsAddItemFeatureForDraperyManagementAvailable)
            {
                await this.AddItemOperationDraperyManagementAsync();
            }
            else
            {
                await this.AddItemOperationAsync();
            }
        }

        private async Task ConfirmOperationAsync()
        {
            this.IsWaitingForResponse = true;

            var waitForReason = await this.CheckReasonsAsync();

            this.IsWaitingForReason = waitForReason;

            if (!waitForReason)
            {
                await this.ExecuteOperationAsync();
            }
        }

        private async Task ExecuteOperationAsync(string barcode = null, int operation = 0)
        {
            bool noteError = false;
            try
            {
                this.IsBusyConfirmingOperation = true;

                if (this.IsPickVisible)
                {
                    this.IsWaitingForResponse = true;

                    await this.WmsDataProvider.PickAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value,
                        this.reasonId,
                        this.reasonNotes,
                        this.SelectedItemCompartment.Id,
                        this.SelectedItem.Lot,
                        this.SelectedItem.ItemSerialNumber,
                        userName: this.authenticationService.UserName);
                }
                else if (this.IsPutVisible)
                {
                    this.IsWaitingForResponse = true;

                    await this.WmsDataProvider.PutAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value,
                        this.reasonId,
                        this.reasonNotes,
                        this.SelectedItemCompartment.Id,
                        this.SelectedItem.Lot,
                        this.SelectedItem.ItemSerialNumber,
                        userName: this.authenticationService.UserName);
                }
                else if (this.IsAdjustmentVisible)
                {
                    var noteEnabled = await this.machineMissionsWebService.IsEnabeNoteRulesAsync();

                    if (noteEnabled)
                    {
                        if (!string.IsNullOrEmpty(this.reasonNotes) &&
                           this.reasonNotes.Except(" ").Any())
                        {
                            await this.WmsDataProvider.UpdateItemStockAsync(
                                this.SelectedItemCompartment.Id,
                                this.SelectedItemCompartment.ItemId.Value,
                                this.InputQuantity.Value,
                                this.reasonId,
                                this.reasonNotes,
                                this.SelectedItem.Lot,
                                this.SelectedItem.ItemSerialNumber,
                                this.authenticationService.UserName,
                                this.orderId);

                            var item = await this.itemsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);
                            if (item.AverageWeight != null && item.AverageWeight != 0)
                            {
                                var loadingUnit = await this.machineLoadingUnitsWebService.GetByIdAsync(this.SelectedCompartment.LoadingUnitId.Value);

                                var grossWeight = default(double);
                                var differece = this.SelectedItemCompartment.Stock - this.InputQuantity.Value;
                                if (differece > 0)
                                {
                                    grossWeight = loadingUnit.GrossWeight - (item.AverageWeight.Value * Math.Abs(differece) / 1000);
                                }
                                else
                                {
                                    grossWeight = loadingUnit.GrossWeight + (item.AverageWeight.Value * Math.Abs(differece) / 1000);
                                }

                                this.logger.Debug($"Set weight {grossWeight:0.00} to LoadUnit {this.SelectedItemCompartment.ItemId.Value} item {item.Code} difference {differece} unit weight {item.AverageWeight.Value} original weight {loadingUnit.GrossWeight:0.00}");
                                await this.machineLoadingUnitsWebService.SetLoadingUnitWeightAsync(this.SelectedCompartment.LoadingUnitId.Value, grossWeight);
                                this.UnitWeight = grossWeight;
                                this.RaisePropertyChanged(nameof(this.UnitWeight));
                            }

                            this.ShowNotification(Localized.Get("InstallationApp.SuccessfullChange"), Services.Models.NotificationSeverity.Success);
                        }
                        else
                        {
                            noteError = true;
                            this.ShowNotification(Localized.Get("OperatorApp.NoteNotValid"), Services.Models.NotificationSeverity.Error);
                            this.IsBusyConfirmingOperation = false;
                        }
                    }
                    else
                    {
                        await this.WmsDataProvider.UpdateItemStockAsync(
                            this.SelectedItemCompartment.Id,
                            this.SelectedItemCompartment.ItemId.Value,
                            this.InputQuantity.Value,
                            this.reasonId,
                            this.reasonNotes,
                            this.SelectedItem.Lot,
                            this.SelectedItem.ItemSerialNumber,
                            this.authenticationService.UserName,
                            this.orderId);

                        var item = await this.itemsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);
                        if (item.AverageWeight != null && item.AverageWeight != 0)
                        {
                            var loadingUnit = await this.machineLoadingUnitsWebService.GetByIdAsync(this.SelectedItemCompartment.LoadingUnitId);

                            var grossWeight = default(double);
                            var differece = this.SelectedItemCompartment.Stock - this.InputQuantity.Value;
                            if (differece > 0)
                            {
                                grossWeight = loadingUnit.GrossWeight - (item.AverageWeight.Value * Math.Abs(differece) / 1000);
                            }
                            else
                            {
                                grossWeight = loadingUnit.GrossWeight + (item.AverageWeight.Value * Math.Abs(differece) / 1000);
                            }

                            this.logger.Debug($"Set weight {grossWeight:0.00} to LoadUnit {this.SelectedItemCompartment.ItemId.Value} item {item.Code} difference {differece} unit weight {item.AverageWeight.Value} original weight {loadingUnit.GrossWeight:0.00}");
                            await this.machineLoadingUnitsWebService.SetLoadingUnitWeightAsync(this.SelectedItemCompartment.LoadingUnitId, grossWeight);
                            this.UnitWeight = grossWeight;
                            this.RaisePropertyChanged(nameof(this.UnitWeight));
                        }
                    }

                    if (!noteError)
                    {
                        await this.OnDataRefreshAsync();
                        this.IsBusyConfirmingOperation = false;
                    }
                }
                else if (this.IsBoxOperationVisible)
                {
                    await this.compartmentsWebService.BoxToCompartmentAsync(
                        this.SelectedCompartment.Id,
                        barcode,
                        operation);

                    await this.OnDataRefreshAsync();
                    this.IsBusyConfirmingOperation = false;
                }
                else if (this.IsAddItemVisible)
                {
                }
                else
                {
                    this.IsBusyConfirmingOperation = false;
                }

                if (!noteError)
                {
                    this.HideOperation();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsBusyConfirmingOperation = false;
                this.ShowNotification(ex);
            }
            finally
            {
                if (!noteError)
                {
                    this.IsWaitingForResponse = false;
                    this.IsWaitingForReason = false;
                    this.Reasons = null;
                    this.Orders = null;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        private async Task GetItemInfoAsync()
        {
            if (this.SelectedItemCompartment?.ItemId is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;
                var item = await this.itemsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);

                this.QuantityTolerance = item.PickTolerance ?? 0;
                this.MeasureUnit = item.MeasureUnitDescription;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task GetSocketLinkOperation()
        {
            try
            {
                var operation = await this.missionOperationsWebService.GetSocketLinkOperationAsync();
                if (operation != null
                    && (!operation.IsCompleted ?? true)
                    )
                {
                    var newOperation = new SocketLinkOperationChangeMessageData
                    {
                        Id = operation.Id,
                        ItemCode = operation.ItemCode,
                        ItemDescription = operation.ItemDescription,
                        ItemListCode = operation.ItemListCode,
                        Message = operation.Message,
                        OperationType = operation.OperationType,
                        BayNumber = (CommonUtils.Messages.Enumerations.BayNumber)this.MachineService.BayNumber,
                        RequestedQuantity = operation.RequestedQuantity ?? 0,
                        CompartmentX1Position = operation.CompartmentX1Position,
                        CompartmentX2Position = operation.CompartmentX2Position,
                        CompartmentY1Position = operation.CompartmentY1Position,
                        CompartmentY2Position = operation.CompartmentY2Position,
                    };
                    this.SocketLinkOperationChange(newOperation);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void HideOperation()
        {
            this.IsAdjustmentVisible = false;
            this.IsAddItemVisible = false;
            this.IsPickVisible = false;
            this.IsPutVisible = false;
            this.IsBoxOperationVisible = false;

            this.IsOperationVisible = false;
        }

        private async Task OnAppearItem()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();

            this.RaisePropertyChanged(nameof(this.Products));

            this.IsBusyLoading = true;
            this.ProductsDataGridViewVisibility = this.isBusyLoading && !this.IsAddItemFeatureForDraperyManagementAvailable;
            this.ChargeItemTextViewVisibility = !this.isBusyLoading && !this.IsAddItemFeatureForDraperyManagementAvailable;
        }

        private async Task OnItemWeightChangedAsync(ItemWeightChangedMessage itemWeightChanged)
        {
            this.lastItemQuantityMessage = itemWeightChanged;
        }

        private async Task OnProductsChangedAsync(ProductsChangedEventArgs e)
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();
        }

        private void OnSocketLinkOperationChanged(NotificationMessageUI<SocketLinkOperationChangeMessageData> e)
        {
            this.SocketLinkOperationChange(e.Data);
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
                    var machineIdentity = await this.identityService.GetAsync(cancellationToken);
                    this.areaId = machineIdentity.AreaId;
                }

                if (this.areaId.HasValue)
                {
                    // retrieve all products from warehouse
                    var totalProducts = await this.areasWebService.GetAllProductsAsync(this.searchItem, cancellationToken);

                    if (totalProducts != null)
                    {
                        this.allProducts.AddRange(totalProducts);
                    }
                }
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

        private void Scroll(object parameter)
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

        /// <summary>
        /// Show details for items via barcode data.
        /// Only reserved for drapery items management.
        /// </summary>
        private async Task ShowItemDetailsByBarcode_DraperyItemStuff_Async(UserActionEventArgs e)
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

        /// <summary>
        /// Show details for items via barcode data.
        /// </summary>
        private async Task ShowItemDetailsByBarcodeAsync(UserActionEventArgs e)
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

                // retrieve all products from warehouse
                var items = await this.areasWebService.GetAllProductsAsync(itemCode);

                if (items.Any())
                {
                    this.SearchItem = itemCode;
                    this.products.AddRange(items.Select(i => new ItemInfo(i, this.machineService.Bay.Id)));
                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.ItemsFilteredByCode")), Services.Models.NotificationSeverity.Info);
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
                            this.products.Add(new ItemInfo(item, this.machineService.Bay.Id));

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

        private void SocketLinkOperationChange(SocketLinkOperationChangeMessageData e)
        {
            var isOperation = !string.IsNullOrEmpty(e.Id);

            if ((BayNumber)e.BayNumber == this.MachineService.BayNumber)
            {
                this.IsSocketLinkOperationVisible = isOperation;
                if (isOperation)
                {
                    this.SocketLinkOperation = new SocketLinkOperation
                    {
                        Id = e.Id,
                        ItemCode = e.ItemCode,
                        ItemDescription = e.ItemDescription,
                        ItemListCode = e.ItemListCode,
                        Message = e.Message,
                        OperationType = e.OperationType,
                        RequestedQuantity = e.RequestedQuantity,
                        CompartmentX1Position = e.CompartmentX1Position,
                        CompartmentX2Position = e.CompartmentX2Position,
                        CompartmentY1Position = e.CompartmentY1Position,
                        CompartmentY2Position = e.CompartmentY2Position,
                    };

                    this.InputQuantity = this.SocketLinkOperation.RequestedQuantity;
                    this.QuantityTolerance = 0;

                    this.navigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.SOCKETLINKOPERATION,
                        this.SocketLinkOperation,
                        trackCurrentView: false);
                }
                else
                {
                    this.SocketLinkOperation = null;
                }
            }
        }

        private async Task ToggleOperation(string operationType)
        {
            if (operationType is null)
            {
                throw new ArgumentNullException(nameof(operationType));
            }

            if (operationType == OperatorApp.Adjustment)
            {
                if (this.lastItemQuantityMessage != null)
                {
                    this.SelectedItemCompartment.ItemId = this.weightMission.ItemId;
                }

                if (!this.SelectedItemCompartment.ItemId.HasValue)
                {
                    return;
                }
            }

            var previousIsListModeEnabled = this.IsListModeEnabled;
            this.IsListModeEnabled = false;

            try
            {
                await this.GetItemInfoAsync();

                if (operationType == OperatorApp.Pick)
                {
                    this.InputQuantity = 0;
                    this.IsPickVisible = !this.IsPickVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.PickingQuantity"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Put)
                {
                    this.InputQuantity = 0;
                    this.IsPutVisible = !this.IsPutVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.PutQuantity"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Adjustment)
                {
                    if (this.lastItemQuantityMessage != null)
                    {
                        this.InputQuantity = this.lastItemQuantityMessage.MeasureadQuantity;
                        this.lastItemQuantityMessage = null;
                        this.weightMission = null;
                    }
                    else
                    {
                        this.InputQuantity = this.SelectedItemCompartment.Stock;
                    }

                    this.IsAdjustmentVisible = !this.IsAdjustmentVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.AdjustmentQuantity"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Box)
                {
                    this.InputQuantity = 1;
                    this.IsBoxOperationVisible = !this.IsBoxOperationVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.Box"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Add)
                {
                    this.IsAddItemVisible = !this.IsAddItemVisible;
                }
                else
                {
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), operationType));
                    return;
                }

                this.IsOperationVisible =
                    this.IsPickVisible
                    ||
                    this.IsPutVisible
                    ||
                    this.IsAdjustmentVisible
                    ||
                    this.IsAddItemVisible
                    ||
                    this.IsBoxOperationVisible
                    ||
                    this.IsSocketLinkOperationVisible;

                this.CanInputQuantity = this.IsOperationVisible;

                this.RaisePropertyChanged();
                this.RaiseCanExecuteChanged();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsListModeEnabled = previousIsListModeEnabled;
            }
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
            this.weightMission = this.MissionOperationsService.ActiveWmsOperation;
            this.weightMission.ItemId = this.SelectedItemCompartment.ItemId.Value;
            this.weightMission.RequestedQuantity = this.inputQuantity.Value;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.WEIGHT,
                this.weightMission);
        }

        #endregion
    }
}

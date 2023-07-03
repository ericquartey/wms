using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class AddMatrixViewModel : BaseOperatorViewModel
    {
        #region Fields

        private const int DefaultPageSize = 60;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 10;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IBayManager bayManager;

        private readonly IDialogService dialogService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly ISessionService sessionService;

        private readonly IWmsDataProvider wmsDataProvider;

        private readonly IMachineErrorsService errorsService;

        private bool appear;

        private int? areaId;

        private double? availableQuantity;

        private DelegateCommand confirmMatrixCommand;

        private int currentItemIndex;

        private bool isBusyConfirmingOperation;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isBusyRequestingItemPut;

        private bool isDistinctBySerialNumber;

        private bool isDistinctBySerialNumberEnabled;

        private bool isExpireDate;

        private bool isExpireDateEnable;

        private bool isGroupbyLot;

        private bool isGroupbyLotEnabled;

        private bool isSearching;

        private bool isSscc;

        private bool isSsccEnabled;

        private double? itemOccupiedSpaceValue;

        private List<ItemInfo> items = new List<ItemInfo>();

        private string itemSearchLabel;

        private int? loadingUnitId;

        private int maxKnownIndexSelection;

        private SubscriptionToken productsChangedToken;

        private List<ProductInMachine> productsInCurrentMachine;

        private double quantityIncrement;

        private int? quantityTolerance;

        private double? quantityValue;

        private bool reloadSearchItems;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private int? selectedCompartmentId;

        private ItemInfo selectedItem;

        private string selectedItemTxt;

        private double spaceIncrement;

        private CancellationTokenSource tokenSource;

        private bool type350 = true;

        #endregion

        #region Constructors

        public AddMatrixViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineIdentityWebService identityService,
            IBayManager bayManager,
            IMachineAreasWebService areasWebService,
            IDialogService dialogService,
            IMachineErrorsService errorsService,
            IMachineItemsWebService itemsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IBarcodeReaderService barcodeReaderService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IAuthenticationService authenticationService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineIdentityWebService machineIdentityWebService,
            ISessionService sessionService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        }

        #endregion

        #region Properties

        public bool Appear
        {
            get => this.appear;
            private set => this.SetProperty(ref this.appear, value, this.RaiseCanExecuteChanged);
        }

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value);
        }

        public ICommand ConfirmMatrixCommand =>
          this.confirmMatrixCommand
          ??
          (this.confirmMatrixCommand = new DelegateCommand(
              async () => await this.ConfirmMatrix(),
              this.CanConfirmMatrix));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            private set => this.SetProperty(ref this.isBusyConfirmingOperation, value);
        }

        public bool IsBusyLoadingNextPage
        {
            get => this.isBusyLoadingNextPage;
            private set => this.SetProperty(ref this.isBusyLoadingNextPage, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyRequestingItemPick
        {
            get => this.isBusyRequestingItemPick;
            private set => this.SetProperty(ref this.isBusyRequestingItemPick, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyRequestingItemPut
        {
            get => this.isBusyRequestingItemPut;
            private set => this.SetProperty(ref this.isBusyRequestingItemPut, value, this.RaiseCanExecuteChanged);
        }

        public bool IsDistinctBySerialNumber
        {
            get => this.isDistinctBySerialNumber;
            set
            {
                if (this.SetProperty(ref this.isDistinctBySerialNumber, value))
                {
                    new Task(async () =>
                    {
                        this.Appear = false;
                        this.IsSearching = true;
                        this.SelectedItem = null;
                        this.currentItemIndex = 0;
                        this.tokenSource = new CancellationTokenSource();
                        await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
                        await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
                        this.Appear = true;
                    }).Start();
                }
            }
        }

        public bool IsDistinctBySerialNumberEnabled
        {
            get => this.isDistinctBySerialNumberEnabled;
            private set => this.SetProperty(ref this.isDistinctBySerialNumberEnabled, value);
        }

        public bool IsExpireDate
        {
            get => this.isExpireDate;
            set
            {
                if (this.SetProperty(ref this.isExpireDate, value))
                {
                    new Task(async () =>
                    {
                        this.Appear = false;
                        this.IsSearching = true;
                        this.SelectedItem = null;
                        this.currentItemIndex = 0;
                        this.tokenSource = new CancellationTokenSource();
                        await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
                        await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
                        this.Appear = true;
                    }).Start();
                }
            }
        }

        public bool IsExpireDateEnable
        {
            get => this.isExpireDateEnable;
            private set => this.SetProperty(ref this.isExpireDateEnable, value);
        }

        public bool IsGroupbyLot
        {
            get => this.isGroupbyLot;
            set
            {
                if (this.SetProperty(ref this.isGroupbyLot, value))
                {
                    new Task(async () =>
                    {
                        this.Appear = false;
                        this.IsSearching = true;
                        this.SelectedItem = null;
                        this.currentItemIndex = 0;
                        this.tokenSource = new CancellationTokenSource();
                        await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
                        await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
                        this.Appear = true;
                    }).Start();
                }
            }
        }

        public bool IsGroupbyLotEnabled
        {
            get => this.isGroupbyLotEnabled;
            private set => this.SetProperty(ref this.isGroupbyLotEnabled, value);
        }

        public bool IsSearching
        {
            get => this.isSearching;
            set => this.SetProperty(ref this.isSearching, value, this.RaiseCanExecuteChanged);
        }

        public bool IsSscc
        {
            get => this.isSscc;
            set
            {
                if (this.SetProperty(ref this.isSscc, value))
                {
                    new Task(async () =>
                    {
                        this.Appear = false;
                        this.IsSearching = true;
                        this.SelectedItem = null;
                        this.currentItemIndex = 0;
                        this.tokenSource = new CancellationTokenSource();
                        await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
                        await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
                        this.Appear = true;
                    }).Start();
                }
            }
        }

        public bool IsSsccEnabled
        {
            get => this.isSsccEnabled;
            set => this.SetProperty(ref this.isSsccEnabled, value, this.RaiseCanExecuteChanged);
        }

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value) && value)
                {
                    this.ClearNotifications();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? ItemOccupiedSpaceValue
        {
            get => this.itemOccupiedSpaceValue;
            set => this.SetProperty(ref this.itemOccupiedSpaceValue, value, this.RaiseCanExecuteChanged);
        }

        public IList<ItemInfo> Items => new List<ItemInfo>(this.items);

        public string ItemSearchLabel
        {
            get => this.itemSearchLabel;
            set => this.SetProperty(ref this.itemSearchLabel, value);
        }

        public override bool KeepAlive => true;

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            private set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value, this.RaiseCanExecuteChanged);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = this.quantityTolerance.HasValue ? Math.Pow(10, -this.quantityTolerance.Value) : 1;
                }
            }
        }

        public double? QuantityValue
        {
            get => this.quantityValue;
            set
            {
                if (value >= 0)
                {
                    this.SetProperty(ref this.quantityValue, value, this.RaiseCanExecuteChanged);
                }
            }
        }

        public ICommand ScrollCommand => this.scrollCommand ?? (this.scrollCommand = new DelegateCommand<object>((arg) => this.Scroll(arg)));

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value)
                    && this.reloadSearchItems)
                {
                    this.IsSearching = true;
                    this.ItemSearchLabel = Localized.Get(OperatorApp.ItemSearchKeySearch);
                    this.TriggerSearchAsync().GetAwaiter();
                }
                this.reloadSearchItems = true;
            }
        }

        public int? SelectedCompartmentId
        {
            get => this.selectedCompartmentId;
            private set => this.SetProperty(ref this.selectedCompartmentId, value, this.RaiseCanExecuteChanged);
        }

        public ItemInfo SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (value is null)
                {
                    _ = this.SetProperty(ref this.selectedItem, value);

                    this.selectedItemTxt = Resources.Localized.Get("OperatorApp.RequestedQuantityBase");
                    this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                    this.AvailableQuantity = null;
                    return;
                }

                this.SetProperty(ref this.selectedItem, value);

                var machineId = this.bayManager.Identity.Id;
                this.AvailableQuantity = this.selectedItem.AvailableQuantity;
                this.QuantityValue = 0;

                this.SetCurrentIndex(this.selectedItem?.Id);
                this.selectedItemTxt = string.Format(Resources.Localized.Get("OperatorApp.RequestedQuantity"), this.selectedItem.MeasureUnit);
                this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                this.QuantityTolerance = this.selectedItem?.PickTolerance;
                Task.Run(async () => await this.SelectNextItemAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                this.RaiseCanExecuteChanged();
            }
        }

        public string SelectedItemTxt
        {
            get => this.selectedItemTxt;
            set => this.SetProperty(ref this.selectedItemTxt, value, this.RaiseCanExecuteChanged);
        }

        public double SpaceIncrement
        {
            get => this.spaceIncrement;
            set => this.SetProperty(ref this.spaceIncrement, value);
        }

        public bool Type350
        {
            get => this.type350;
            set => this.SetProperty(ref this.type350, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task ConfirmMatrix()
        {
            try
            {
                await this.machineLoadingUnitsWebService.SendMatrixRequestAsync(this.LoadingUnitId, this.SelectedCompartmentId, 10, this.SelectedItem.Id, this.QuantityValue.Value, this.ItemOccupiedSpaceValue.Value, this.Type350);

                this.NavigationService.GoBack();

                this.ShowNotification(OperatorApp.OperationSuccess);
            }
            catch (Exception)
            {
                this.ShowNotification(OperatorApp.MatrixExeptionCode403, Services.Models.NotificationSeverity.Warning);
            }


        }

        public override void Disappear()
        {
            base.Disappear();

            this.currentItemIndex = 0;
            this.maxKnownIndexSelection = 0;
            this.items = new List<ItemInfo>();
            this.SelectedItem = null;
            this.QuantityValue = null;
            this.ItemOccupiedSpaceValue = null;
            this.AvailableQuantity = null;

            this.productsChangedToken?.Dispose();
            this.productsChangedToken = null;

            this.IsBusyConfirmingOperation = false;
            this.IsBusyRequestingItemPut = false;
            this.IsBusyRequestingItemPick = false;
            this.IsWaitingForResponse = false;
            this.IsBusyLoadingNextPage = false;
        }

        public override async Task OnAppearedAsync()
        {
            var configuration = await this.machineConfigurationWebService.GetConfigAsync();

            if (this.Data is List<int?> dataBundle)
            {
                this.LoadingUnitId = dataBundle[0];
                this.SelectedCompartmentId = dataBundle[1];
            }

            this.Appear = false;
            this.QuantityValue = 0;
            this.ItemOccupiedSpaceValue = 0;
            this.IsBusyConfirmingOperation = false;
            this.reloadSearchItems = true;
            this.ItemSearchLabel = Localized.Get(OperatorApp.ItemSearchKeySearch);
            this.Type350 = true;

            this.IsKeyboardButtonVisible = await this.machineIdentityWebService.GetTouchHelperEnableAsync();

            this.productsChangedToken =
              this.productsChangedToken
              ??
              this.EventAggregator
                  .GetEvent<PubSubEvent<ProductsChangedEventArgs>>()
                  .Subscribe(async e => await this.OnProductsChangedAsync(e), ThreadOption.UIThread, false);

            if (this.selectedItem is null)
            {
                this.selectedItemTxt = Resources.Localized.Get("OperatorApp.RequestedQuantityBase");
                this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
            }

            await base.OnAppearedAsync();

            await this.OnAppearItem();
        }

        public async Task SearchItemAsync(int skip, CancellationToken cancellationToken)
        {
            if (!this.areaId.HasValue)
            {
                return;
            }

            if (skip == 0)
            {
                this.items.Clear();
                this.maxKnownIndexSelection = 0;
            }

            try
            {
                var newItems = this.productsInCurrentMachine.Skip(skip).Take(DefaultPageSize);

                if (!newItems.Any())
                {
                    this.RaisePropertyChanged(nameof(this.Items));
                    return;
                }

                this.items.AddRange(newItems.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));

                if (this.items.Count == 0)
                {
                    this.IsGroupbyLot = false;
                    this.IsDistinctBySerialNumber = false;

                    this.IsGroupbyLotEnabled = false;
                    this.IsDistinctBySerialNumberEnabled = false;

                    this.IsExpireDate = false;
                    this.IsExpireDateEnable = false;
                }
                else if (this.items.Count == 1)
                {
                    this.IsExpireDateEnable = true;
                    this.IsGroupbyLotEnabled = true;
                    this.IsDistinctBySerialNumberEnabled = true;
                }
                else
                {
                    this.IsExpireDateEnable = true;
                    this.IsGroupbyLotEnabled = true;
                    this.IsDistinctBySerialNumberEnabled = true;
                }
            }
            catch (TaskCanceledException)
            {
                // normal situation
                this.items.Clear();
                this.SelectedItem = null;
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
            }
            catch (Exception ex)
            {
                if (this.appear)
                {
                    this.ShowNotification(ex);
                }

                this.items.Clear();
                this.SelectedItem = null;
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
            }
            finally
            {
                this.IsSearching = false;
                this.IsBusyLoadingNextPage = false;
            }

            this.RaisePropertyChanged(nameof(this.Items));
            this.AdjustItemsAppearance();
        }

        public async Task SelectNextItemAsync()
        {
            if (this.currentItemIndex > this.maxKnownIndexSelection)
            {
                this.maxKnownIndexSelection = this.currentItemIndex;
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                if (this.areaId is null)
                {
                    var machineIdentity = this.sessionService.MachineIdentity;
                    this.areaId = machineIdentity.AreaId;
                }

                await this.RefreshItemsAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmMatrixCommand?.RaiseCanExecuteChanged();
        }

        private void AdjustItemsAppearance()
        {
            try
            {
                if (this.maxKnownIndexSelection == 0)
                {
                    this.maxKnownIndexSelection = Math.Min(this.items.Count, ItemsVisiblePageSize) - 1;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanConfirmMatrix()
        {
            return this.SelectedItem != null
                && this.QuantityValue > 0
                && this.ItemOccupiedSpaceValue > 0;
        }

        private async Task OnAppearItem()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
            await this.RefreshItemsAsync();

            this.RaisePropertyChanged(nameof(this.Items));
            this.Appear = true;
        }

        private async Task OnProductsChangedAsync(ProductsChangedEventArgs e)
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
            await this.RefreshItemsAsync();
        }

        private async Task RefreshItemsAsync()
        {
            var startIndex = ((this.maxKnownIndexSelection - ItemsVisiblePageSize) > 0) ? this.maxKnownIndexSelection - ItemsVisiblePageSize : 0;
            this.currentItemIndex = startIndex;

            this.tokenSource = new CancellationTokenSource();
            await this.SearchItemAsync(startIndex, this.tokenSource.Token);
        }

        private async Task ReloadAllItems(string searchItem, CancellationToken cancellationToken)
        {
            this.productsInCurrentMachine = new List<ProductInMachine>();

            try
            {
                if (this.areaId is null)
                {
                    var machineIdentity = this.sessionService.MachineIdentity;
                    this.areaId = machineIdentity.AreaId;
                }

                var totalProducts = await this.areasWebService.GetAllProductsAsync(searchItem, cancellationToken);

                var model = this.sessionService.MachineIdentity;

                if (model is null)
                {
                    this.items.AddRange(totalProducts.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));
                }
                else
                {
                    this.productsInCurrentMachine.Clear();

                    foreach (var item in totalProducts.ToList())
                    {
                        var newItem = new ProductInMachine()
                        {
                            Machines = item.Machines,
                            Lot = item.Lot,
                            SerialNumber = item.SerialNumber,
                            Sscc = item.Sscc,
                            Item = item.Item,
                        };
                        this.productsInCurrentMachine.Add(newItem);
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
            if (scrollChangedEventArgs != null && !this.IsBusyLoadingNextPage)
            {
                var last = (int)scrollChangedEventArgs.VerticalOffset + (int)scrollChangedEventArgs.ViewportHeight;

                if (last > this.maxKnownIndexSelection)
                {
                    this.maxKnownIndexSelection = last;
                }

                if (last >= Math.Max((this.items.Count + 1) - ItemsToCheckBeforeLoad, DefaultPageSize - ItemsToCheckBeforeLoad - 1))
                {
                    this.IsSearching = true;
                    this.tokenSource = new CancellationTokenSource();
                    this.IsBusyLoadingNextPage = true;
                    Task.Run(async () => await this.SearchItemAsync(last, this.tokenSource.Token).ConfigureAwait(false)).GetAwaiter().GetResult();
                }
            }
        }

        private void SetCurrentIndex(int? itemId)
        {
            if (itemId.HasValue
                &&
                this.items.FirstOrDefault(i => i.Id == itemId.Value) is ItemInfo itemFound)
            {
                this.currentItemIndex = this.items.IndexOf(itemFound) + 1;
            }
            else
            {
                this.currentItemIndex = 0;
                this.maxKnownIndexSelection = 0;
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
                await this.ReloadAllItems(this.searchItem, this.tokenSource.Token);
                await this.SearchItemAsync(0, this.tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        #endregion
    }
}

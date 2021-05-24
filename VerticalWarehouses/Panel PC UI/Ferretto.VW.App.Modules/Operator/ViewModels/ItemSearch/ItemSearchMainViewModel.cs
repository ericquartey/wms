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
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchMainViewModel : BaseOperatorViewModel, IOperationalContextViewModel, IOperationReasonsSelector
    {
        #region Fields

        private const int DefaultPageSize = 60;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 10;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private bool appear;

        private int? areaId;

        private double? availableQuantity;

        private DelegateCommand cancelReasonCommand;

        private DelegateCommand confirmReasonCommand;

        private int currentItemIndex;

        private int? inputQuantity;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isBusyRequestingItemPut;

        private bool isDistinctBySerialNumber;

        private bool isDistinctBySerialNumberEnabled;

        private bool isGroupbyLot;

        private bool isGroupbyLotEnabled;

        private bool isSearching;

        private List<ItemInfo> items = new List<ItemInfo>();

        //private string itemToPickCode;

        //private int? itemToPickId;

        private int maxKnownIndexSelection;

        private SubscriptionToken productsChangedToken;

        private List<ProductInMachine> productsInCurrentMachine;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand requestItemPickCommand;

        private DelegateCommand requestItemPutCommand;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private ItemInfo selectedItem;

        private string selectedItemTxt;

        private DelegateCommand showItemDetailsCommand;

        private CancellationTokenSource tokenSource;

        private DelegateCommand unitsPageCommand;

        #endregion

        #region Constructors

        public ItemSearchMainViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineIdentityWebService identityService,
            IBayManager bayManager,
            IMachineAreasWebService areasWebService,
            IMachineItemsWebService itemsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBarcodeReaderService barcodeReaderService,
            IAuthenticationService authenticationService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            this.maxKnownIndexSelection = ItemsVisiblePageSize;
        }

        #endregion

        #region Properties

        public string ActiveContextName => this.isBusyLoadingNextPage ? null : OperationalContext.ItemsSearch.ToString();

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

        public ICommand CancelReasonCommand =>
              this.cancelReasonCommand
              ??
              (this.cancelReasonCommand = new DelegateCommand(
                  this.CancelReason));

        public ICommand ConfirmReasonCommand =>
          this.confirmReasonCommand
          ??
          (this.confirmReasonCommand = new DelegateCommand(
              async () => await this.ExecuteItemAsync(),
              this.CanExecuteItemPick));

        public override EnableMask EnableMask => EnableMask.Any;

        public int? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingOperation { get; private set; }

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
                        await this.ReloadAllItems(this.tokenSource.Token);
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
                        await this.ReloadAllItems(this.tokenSource.Token);
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

        public ICommand ItemDetailButtonCommand =>
           this.showItemDetailsCommand
           ??
           (this.showItemDetailsCommand = new DelegateCommand(
               () => this.ShowItemDetails(this.SelectedItem),
               this.CanShowItemDetails));

        public IList<ItemInfo> Items => new List<ItemInfo>(this.items);

        public override bool KeepAlive => true;

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

        public ICommand RequestItemPickCommand =>
            this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
                async () => await this.RequestItemPickAsync(this.selectedItem.Id, this.selectedItem.Code),
                this.CanRequestItemPick));

        public ICommand RequestItemPutCommand =>
            this.requestItemPutCommand
            ??
            (this.requestItemPutCommand = new DelegateCommand(
                async () => await this.RequestItemPutAsync(this.selectedItem.Id, this.selectedItem.Code),
                this.CanRequestItemPut));

        public ICommand ScrollCommand => this.scrollCommand ?? (this.scrollCommand = new DelegateCommand<object>((arg) => this.Scroll(arg)));

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value))
                {
                    this.IsSearching = true;
                    this.TriggerSearchAsync().GetAwaiter();
                }
            }
        }

        public ItemInfo SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (value is null)
                {
                    //this.RaisePropertyChanged();
                    this.selectedItemTxt = Resources.Localized.Get("OperatorApp.RequestedQuantityBase");
                    this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                    return;
                }

                this.SetProperty(ref this.selectedItem, value);

                var machineId = this.bayManager.Identity.Id;
                this.AvailableQuantity = this.selectedItem.AvailableQuantity;
                this.InputQuantity = 0;
                //this.itemToPickId = value.Id;
                //this.itemToPickCode = value.Code;

                this.SetCurrentIndex(this.selectedItem?.Id);
                this.selectedItemTxt = string.Format(Resources.Localized.Get("OperatorApp.RequestedQuantity"), this.selectedItem.MeasureUnit);
                this.RaisePropertyChanged(nameof(this.SelectedItemTxt));
                Task.Run(async () => await this.SelectNextItemAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                this.RaiseCanExecuteChanged();
            }
        }

        public string SelectedItemTxt
        {
            get => this.selectedItemTxt;
            set => this.SetProperty(ref this.selectedItemTxt, value, this.RaiseCanExecuteChanged);
        }

        public ICommand UnitsPageCommand => this.unitsPageCommand
            ??
            (this.unitsPageCommand = new DelegateCommand(
                () => this.ShowUnitsPage(this.SelectedItem),
                this.CanShowUnitsPage));

        #endregion

        #region Methods

        public async Task AutoPickItem(string barcode)
        {
            this.ClearNotifications();
            try
            {
                var item = await this.itemsWebService.GetByBarcodeAsync(barcode);
                if (item is null)
                {
                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), barcode), Services.Models.NotificationSeverity.Warning);
                }
                else
                {
                    var reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Put);

                    await this.wmsDataProvider.PutAsync(
                            item.Id,
                            1,
                            reasons.FirstOrDefault().Id,
                            barcode,
                            null,
                            null,
                            null,
                            userName: this.authenticationService.UserName);

                    this.ShowNotification(
                        string.Format(
                            Resources.Localized.Get("OperatorApp.PutRequestWasAccepted"),
                            item.Code,
                            this.InputQuantity),
                        Services.Models.NotificationSeverity.Success);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), barcode), Services.Models.NotificationSeverity.Warning);
                this.logger.Error(ex.ToString());
            }
        }

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;

            try
            {
                this.IsBusyLoadingNextPage = true;
                this.ReasonNotes = null;

                if (this.IsBusyRequestingItemPick)
                {
                    this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Pick);
                }
                else
                {
                    this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Put);
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
            }
            finally
            {
                this.IsBusyLoadingNextPage = false;
            }

            return this.Reasons?.Any() == true;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            switch (userAction.UserAction)
            {
                case UserAction.FilterItems:
                    if (this.MachineService.Bay.BarcodeAutomaticPut)
                    {
                        await this.AutoPickItem(userAction.Code);
                    }
                    else
                    {
                        await this.ShowItemDetailsByBarcodeAsync(userAction);
                    }

                    break;

                case UserAction.PickItem:
                    await this.PickItemByBarcodeAsync(userAction);

                    break;
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            this.currentItemIndex = 0;
            this.maxKnownIndexSelection = 0;
            this.items = new List<ItemInfo>();
            this.selectedItem = null;
            this.inputQuantity = null;
            this.AvailableQuantity = null;

            this.productsChangedToken?.Dispose();
            this.productsChangedToken = null;

            this.IsBusyConfirmingOperation = false;
            this.IsBusyRequestingItemPut = false;
            this.IsBusyRequestingItemPick = false;
            this.IsWaitingForResponse = false;
            this.IsBusyLoadingNextPage = false;
        }

        public async Task ExecuteItemAsync()
        {
            if (this.isBusyRequestingItemPick)
            {
                await this.ExecuteItemPickAsync(this.selectedItem.Id, this.selectedItem.Code, this.selectedItem.Lot, this.selectedItem.SerialNumber);
            }
            else
            {
                await this.ExecuteItemPutAsync(this.selectedItem.Id, this.selectedItem.Code, this.selectedItem.Lot, this.selectedItem.SerialNumber);
            }

            this.selectedItem = null;
            this.RaisePropertyChanged(nameof(this.SelectedItem));
        }

        //public async Task ExecuteItemPickAsync()
        //{
        //    try
        //    {
        //        this.IsWaitingForResponse = true;
        //        this.IsBusyRequestingItemPick = true;

        //        await this.wmsDataProvider.PickAsync(
        //            this.itemToPickId.Value,
        //            this.InputQuantity.Value,
        //            this.reasonId,
        //            this.reasonNotes);

        //        this.Reasons = null;

        //        this.ShowNotification(
        //            string.Format(
        //                Resources.Localized.Get("OperatorApp.PickRequestWasAccepted"),
        //                this.itemToPickCode,
        //                this.InputQuantity),
        //            Services.Models.NotificationSeverity.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ShowNotification(ex);
        //    }
        //    finally
        //    {
        //        this.InputQuantity = 0;
        //        this.IsBusyRequestingItemPick = false;
        //        this.IsWaitingForResponse = false;

        //        this.itemToPickCode = null;
        //        this.itemToPickId = null;
        //    }
        //}

        public async Task ExecuteItemPickAsync(int itemId, string itemCode, string lot, string serialNumber)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPick = true;

                await this.wmsDataProvider.PickAsync(
                    itemId,
                    this.InputQuantity.Value,
                    this.reasonId,
                    this.reasonNotes,
                    lot: lot,
                    serialNumber: serialNumber,
                    userName: this.authenticationService.UserName);

                this.Reasons = null;

                this.ShowNotification(
                    string.Format(
                        Resources.Localized.Get("OperatorApp.PickRequestWasAccepted"),
                        itemCode,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = 0;
                this.IsBusyRequestingItemPick = false;
                this.IsWaitingForResponse = false;
            }
        }

        //public async Task ExecuteItemPutAsync()
        //{
        //    try
        //    {
        //        this.IsWaitingForResponse = true;
        //        this.IsBusyRequestingItemPut = true;

        //        await this.wmsDataProvider.PutAsync(
        //            this.itemToPickId.Value,
        //            this.InputQuantity.Value,
        //            this.reasonId,
        //            this.reasonNotes);

        //        this.Reasons = null;

        //        this.ShowNotification(
        //            string.Format(
        //                Resources.Localized.Get("OperatorApp.PutRequestWasAccepted"),
        //                this.itemToPickCode,
        //                this.InputQuantity),
        //            Services.Models.NotificationSeverity.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ShowNotification(ex);
        //    }
        //    finally
        //    {
        //        this.InputQuantity = 0;
        //        this.IsBusyRequestingItemPut = false;
        //        this.IsWaitingForResponse = false;

        //        this.itemToPickCode = null;
        //        this.itemToPickId = null;
        //    }
        //}

        public async Task ExecuteItemPutAsync(int itemId, string itemCode, string lot, string serialNumber)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPut = true;

                await this.wmsDataProvider.PutAsync(
                    itemId,
                    this.InputQuantity.Value,
                    this.reasonId,
                    this.reasonNotes,
                    lot: lot,
                    serialNumber: serialNumber,
                    userName: this.authenticationService.UserName);

                this.Reasons = null;

                this.ShowNotification(
                    string.Format(
                        Resources.Localized.Get("OperatorApp.PutRequestWasAccepted"),
                        itemCode,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = 0;
                this.IsBusyRequestingItemPut = false;
                this.IsWaitingForResponse = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.Appear = false;
            this.InputQuantity = 0;
            this.Reasons = null;
            this.IsBusyConfirmingOperation = false;

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

            this.NoteEnabled = true;

            await this.OnAppearItem();
        }

        public async Task RequestItemPickAsync(int itemId, string itemCode)
        {
            this.IsBusyRequestingItemPick = true;
            this.IsWaitingForResponse = true;

            //this.itemToPickId = itemId;
            //this.itemToPickCode = itemCode;

            this.NoteEnabled = false;
            this.RaisePropertyChanged(nameof(this.NoteEnabled));

            var waitForReason = await this.CheckReasonsAsync();

            if (!waitForReason)
            {
                await this.ExecuteItemPickAsync(this.selectedItem.Id, this.selectedItem.Code, this.selectedItem.Lot, this.selectedItem.SerialNumber);
                this.selectedItem = null;
                this.RaisePropertyChanged(nameof(this.SelectedItem));
            }
        }

        public async Task RequestItemPutAsync(int itemId, string itemCode)
        {
            this.IsBusyRequestingItemPut = true;
            this.IsWaitingForResponse = true;

            //this.itemToPickId = itemId;
            //this.itemToPickCode = itemCode;

            this.NoteEnabled = false;
            this.RaisePropertyChanged(nameof(this.NoteEnabled));

            var waitForReason = await this.CheckReasonsAsync();

            if (!waitForReason)
            {
                await this.ExecuteItemPutAsync(this.selectedItem.Id, this.selectedItem.Code, this.selectedItem.Lot, this.selectedItem.SerialNumber);
                this.selectedItem = null;
                this.RaisePropertyChanged(nameof(this.SelectedItem));
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
                this.items.Clear();
                this.maxKnownIndexSelection = 0;
            }

            var selectedItemId = this.SelectedItem?.Id;

            try
            {
                //var newItems = await this.areasWebService.GetProductsAsync(
                //    this.areaId.Value,
                //    skip,
                //    DefaultPageSize,
                //    this.searchItem,
                //    this.IsGroupbyLot,
                //    this.isDistinctBySerialNumber,
                //    cancellationToken);

                //var totalNewItems = await this.areasWebService.GetProductsAsync(
                //    this.areaId.Value,
                //    0,
                //    0,
                //    this.searchItem,
                //    this.IsGroupbyLot,
                //    this.isDistinctBySerialNumber,
                //    cancellationToken);

                //if (totalNewItems.Count() <= DefaultPageSize && this.lastSearchItem == this.searchItem && this.searchItem != null)
                //{
                //    this.lastSearchItem = this.searchItem;
                //    return;
                //}

                //this.lastSearchItem = this.searchItem;

                var newItems = this.productsInCurrentMachine.Skip(skip).Take(DefaultPageSize);

                if (!newItems.Any())
                {
                    this.RaisePropertyChanged(nameof(this.Items));
                    return;
                }

                //var model = await this.identityService.GetAsync();

                //if (model is null)
                //{
                //    this.items.AddRange(newItems.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));
                //}
                //else
                //{
                //    this.productInCurrentMachine.Clear();

                //    foreach (var item in newItems.ToList())
                //    {
                //        for (int i = 0; i < item.Machines.Count(); i++)
                //        {
                //            if (item.Machines.ElementAt(i).Id == model.Id)
                //            {
                //                this.productInCurrentMachine.Add(item);
                //            }
                //        }
                //    }
                //}

                //this.items.AddRange(this.productInCurrentMachine.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));

                this.items.AddRange(newItems.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));

                if (this.items.Count == 0)
                {
                    this.IsGroupbyLot = false;
                    this.IsDistinctBySerialNumber = false;
                    this.IsGroupbyLotEnabled = false;
                    this.IsDistinctBySerialNumberEnabled = false;
                }
                else if (this.items.Count == 1)
                {
                    this.IsGroupbyLotEnabled = true;
                    this.IsDistinctBySerialNumberEnabled = true;
                    this.SelectedItem = this.items.FirstOrDefault();
                }
                else
                {
                    this.IsGroupbyLotEnabled = true;
                    this.IsDistinctBySerialNumberEnabled = true;
                }
            }
            //catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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
            this.RaisePropertyChanged(nameof(this.SelectedItem));

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

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                if (this.areaId is null)
                {
                    var machineIdentity = await this.identityService.GetAsync();
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

            this.requestItemPickCommand?.RaiseCanExecuteChanged();
            this.requestItemPutCommand?.RaiseCanExecuteChanged();
            this.showItemDetailsCommand?.RaiseCanExecuteChanged();
            this.unitsPageCommand?.RaiseCanExecuteChanged();

            this.confirmReasonCommand?.RaiseCanExecuteChanged();
            this.cancelReasonCommand?.RaiseCanExecuteChanged();
        }

        private void AdjustItemsAppearance()
        {
            try
            {
                if (this.maxKnownIndexSelection == 0)
                {
                    this.maxKnownIndexSelection = Math.Min(this.items.Count, ItemsVisiblePageSize) - 1;
                }

                if (this.maxKnownIndexSelection >= ItemsVisiblePageSize
                    &&
                    this.Items.Count >= this.maxKnownIndexSelection)
                {
                    this.SelectedItem = this.items?.ElementAtOrDefault(this.maxKnownIndexSelection);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void CancelReason()
        {
            this.Reasons = null;
            this.IsBusyConfirmingOperation = false;
            this.IsBusyRequestingItemPut = false;
            this.IsBusyRequestingItemPick = false;
            this.IsWaitingForResponse = false;
        }

        private bool CanExecuteItemPick()
        {
            return !(this.reasonId is null);
        }

        private bool CanRequestItemPick()
        {
            return
                this.SelectedItem != null
                &&
                this.AvailableQuantity.HasValue
                &&
                this.AvailableQuantity.Value > 0
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity > 0
                &&
                this.InputQuantity <= this.AvailableQuantity.Value
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanRequestItemPut()
        {
            return
                this.SelectedItem != null
                &&
                this.AvailableQuantity.HasValue
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity > 0
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanSelectNextItem()
        {
            return
                this.currentItemIndex < this.items.Count - 1
                &&
                !this.IsSearching
                &&
                !this.IsBusyLoadingNextPage;
        }

        private bool CanSelectPreviousItem()
        {
            return
                this.currentItemIndex > 0
                &&
                !this.IsSearching
                &&
                !this.IsBusyLoadingNextPage;
        }

        private bool CanShowItemDetails()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.SelectedItem != null;
        }

        private bool CanShowUnitsPage()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.SelectedItem != null;
        }

        private async Task OnAppearItem()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();

            this.RaisePropertyChanged(nameof(this.Items));
            this.Appear = true;
        }

        private async Task OnProductsChangedAsync(ProductsChangedEventArgs e)
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReloadAllItems(this.tokenSource.Token);
            await this.RefreshItemsAsync();
        }

        private async Task PickItemByBarcodeAsync(UserActionEventArgs e)
        {
            var itemCode = e.GetItemCode();
            if (itemCode is null)
            {
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheItemCode"), e.Code),
                    Services.Models.NotificationSeverity.Warning);

                return;
            }

            var itemQuantity = e.GetItemQuantity();
            if (itemQuantity is null)
            {
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheItemQuantity"), e.Code),
                    Services.Models.NotificationSeverity.Warning);

                return;
            }

            var items = await this.areasWebService.GetProductsAsync(
                   this.areaId.Value,
                   0,
                   1,
                   itemCode,
                   false,
                   false);

            if (items.Any() && itemQuantity.HasValue)
            {
                if (items.Count() == 1)
                {
                    this.InputQuantity = (int)itemQuantity;

                    await this.RequestItemPickAsync(items.First().Item.Id, itemCode);
                }
            }
            else
            {
                this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), itemCode), Services.Models.NotificationSeverity.Warning);
            }
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
            this.productsInCurrentMachine = new List<ProductInMachine>();

            try
            {
                if (this.areaId is null)
                {
                    var machineIdentity = await this.identityService.GetAsync();
                    this.areaId = machineIdentity.AreaId;
                }

                var totalProducts = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    0,
                    0,
                    this.searchItem,
                    this.IsGroupbyLot,
                    this.isDistinctBySerialNumber,
                    cancellationToken);

                var model = await this.identityService.GetAsync();

                if (model is null)
                {
                    this.items.AddRange(totalProducts.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));
                }
                else
                {
                    this.productsInCurrentMachine.Clear();

                    foreach (var item in totalProducts.ToList())
                    {
                        for (int i = 0; i < item.Machines.Count(); i++)
                        {
                            if (item.Machines.ElementAt(i).Id == model.Id)
                            {
                                this.productsInCurrentMachine.Add(item);
                            }
                        }
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

        private void ShowItemDetails(ItemInfo item)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.ITEM_DETAILS,
                item,
                trackCurrentView: true);
        }

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

                var items = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    0,
                    1,
                    itemCode,
                    false,
                    false);

                if (items.Any())
                {
                    //if (items.Count() == 1)
                    //{
                    //    this.ShowItemDetails(new ItemInfo(items.First(), this.bayManager.Identity.Id));
                    //}
                    //else
                    //{
                    this.SearchItem = itemCode;
                    this.items.AddRange(items.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));
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
                            this.items.Add(new ItemInfo(item, this.bayManager.Identity.Id));

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
        }

        private void ShowUnitsPage(ItemInfo item)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.UNITS,
                item,
                trackCurrentView: true);
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

        #endregion
    }
}

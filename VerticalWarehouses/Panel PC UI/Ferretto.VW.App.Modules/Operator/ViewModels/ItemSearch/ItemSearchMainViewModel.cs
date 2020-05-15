﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
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

        private const int DefaultPageSize = 20;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 10;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private int? areaId;

        private double? availableQuantity;

        private DelegateCommand cancelReasonCommand;

        private DelegateCommand confirmReasonCommand;

        private int currentItemIndex;

        private double? inputQuantity;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isDistinctBySerialNumber;

        private bool isDistinctBySerialNumberEnabled;

        private bool isGroupbyLot;

        private bool isGroupbyLotEnabled;

        private bool isSearching;

        private List<ItemInfo> items = new List<ItemInfo>();

        private int maxKnownIndexSelection;

        private SubscriptionToken productsChangedToken;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand requestItemPickCommand;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private ItemInfo selectedItem;

        private string selectedItemTxt;

        private DelegateCommand showItemDetailsCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public ItemSearchMainViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineIdentityWebService identityService,
            IBayManager bayManager,
            IMachineAreasWebService areasWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBarcodeReaderService barcodeReaderService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.maxKnownIndexSelection = ItemsVisiblePageSize;
        }

        #endregion

        #region Properties

        public string ActiveContextName => this.isBusyLoadingNextPage ? null : OperationalContext.ItemsSearch.ToString();

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
              async () => await this.ExecuteItemPickAsync(this.selectedItem.Id, this.selectedItem.Code),
              this.CanExecuteItemPick));

        public override EnableMask EnableMask => EnableMask.Any;

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
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

        public bool IsDistinctBySerialNumber
        {
            get => this.isDistinctBySerialNumber;
            set => this.SetProperty(ref this.isDistinctBySerialNumber, value);
        }

        public bool IsDistinctBySerialNumberEnabled
        {
            get => this.isDistinctBySerialNumberEnabled;
            private set => this.SetProperty(ref this.isDistinctBySerialNumberEnabled, value);
        }

        public bool IsGroupbyLot
        {
            get => this.isGroupbyLot;
            set => this.SetProperty(ref this.isGroupbyLot, value);
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
                async () => await this.RequestItemPickAsync(this.selectedItem.Id, this.selectedItem.
                    Code),
                this.CanRequestItemPick));

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
                    this.RaisePropertyChanged();
                    return;
                }

                this.SetProperty(ref this.selectedItem, value);

                var machineId = this.bayManager.Identity.Id;
                this.AvailableQuantity = this.SelectedItem.AvailableQuantity;
                this.InputQuantity = null;
                var selectedItemId = this.SelectedItem?.Id;
                this.SetCurrentIndex(selectedItemId);
                this.selectedItemTxt = String.Format(Resources.Localized.Get("OperatorApp.RequestedQuantity"), this.selectedItem.MeasureUnit);
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

        #endregion

        #region Methods

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;

            try
            {
                this.IsBusyLoadingNextPage = true;

                this.Reasons = null;
                //this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Pick);

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

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (Enum.TryParse<UserAction>(e.UserAction, out var userAction))
            {
                switch (userAction)
                {
                    case UserAction.FilterItems:
                        await this.ShowItemDetailsByBarcodeAsync(e);

                        break;

                    case UserAction.PickItem:
                        await this.PickItemByBarcodeAsync(e);

                        break;
                }
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            this.currentItemIndex = 0;
            this.maxKnownIndexSelection = 0;
            this.items = new List<ItemInfo>();

            this.productsChangedToken?.Dispose();
            this.productsChangedToken = null;
        }

        public async Task ExecuteItemPickAsync(int itemId, string itemCode)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPick = true;

                await this.wmsDataProvider.PickAsync(
                    itemId,
                    this.InputQuantity.Value,
                    this.reasonId,
                    this.reasonNotes);

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
                this.InputQuantity = null;
                this.IsBusyRequestingItemPick = false;
                this.IsWaitingForResponse = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.InputQuantity = null;
            this.Reasons = null;
            this.productsChangedToken =
              this.productsChangedToken
              ??
              this.EventAggregator
                  .GetEvent<PubSubEvent<ProductsChangedEventArgs>>()
                  .Subscribe(async e => await this.OnProductsChangedAsync(e), ThreadOption.UIThread, false);

            await base.OnAppearedAsync();
        }

        public async Task RequestItemPickAsync(int itemId, string itemCode)
        {
            this.IsWaitingForResponse = true;
            this.IsBusyRequestingItemPick = true;

            var waitForReason = await this.CheckReasonsAsync();

            if (!waitForReason)
            {
                await this.ExecuteItemPickAsync(itemId, itemCode);
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
                var newItems = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    skip,
                    DefaultPageSize,
                    this.searchItem,
                    this.IsGroupbyLot,
                    this.isDistinctBySerialNumber,
                    cancellationToken);

                this.items.AddRange(newItems.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)));

                if (this.items.Count == 0)
                {
                    this.IsGroupbyLot = false;
                    this.IsDistinctBySerialNumber = false;
                    this.IsGroupbyLotEnabled = false;
                    this.IsDistinctBySerialNumberEnabled = false;
                }
                else
                {
                    this.IsGroupbyLotEnabled = true;
                    this.IsDistinctBySerialNumberEnabled = true;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
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

            this.SetCurrentIndex(selectedItemId);
            this.AdjustItemsAppearance();
        }

        public async Task SelectNextItemAsync()
        {
            if (this.currentItemIndex > this.maxKnownIndexSelection)
            {
                this.maxKnownIndexSelection = this.currentItemIndex;
            }

            if (this.currentItemIndex > Math.Max((this.items.Count - 1) - ItemsToCheckBeforeLoad, DefaultPageSize - ItemsToCheckBeforeLoad))
            {
                this.IsSearching = true;
                this.tokenSource = new CancellationTokenSource();
                this.IsBusyLoadingNextPage = true;
                await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
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
            catch (Exception)
            {
                throw;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.requestItemPickCommand?.RaiseCanExecuteChanged();
            this.showItemDetailsCommand?.RaiseCanExecuteChanged();

            this.confirmReasonCommand?.RaiseCanExecuteChanged();
        }

        private void AdjustItemsAppearance()
        {
            if (this.maxKnownIndexSelection == 0)
            {
                this.maxKnownIndexSelection = Math.Min(this.items.Count, ItemsVisiblePageSize) - 1;
            }

            if (this.maxKnownIndexSelection >= ItemsVisiblePageSize
                &&
                this.Items.Count >= this.maxKnownIndexSelection)
            {
                this.SelectedItem = this.items?.ElementAt(this.maxKnownIndexSelection);
            }
        }

        private void CancelReason()
        {
            this.Reasons = null;
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

        private async Task OnProductsChangedAsync(ProductsChangedEventArgs e)
        {
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
                    this.InputQuantity = itemQuantity;

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

                if (last > Math.Max((this.items.Count + 1) - ItemsToCheckBeforeLoad, DefaultPageSize - ItemsToCheckBeforeLoad))
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

        private void SetSelectedItem()
        {
            if (this.items.Count == 0)
            {
                this.SelectedItem = null;
                return;
            }

            this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
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
                    if (items.Count() == 1)
                    {
                        this.ShowItemDetails(new ItemInfo(items.First(), this.bayManager.Identity.Id));
                    }
                    else
                    {
                        this.SearchItem = itemCode;
                        this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.ItemsFilteredByCode")), Services.Models.NotificationSeverity.Info);
                    }
                }
                else
                {
                    this.ShowNotification(string.Format(Resources.Localized.Get("OperatorApp.NoItemWithCodeIsAvailable"), itemCode), Services.Models.NotificationSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task TriggerSearchAsync()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            try
            {
                const int callDelayMilliseconds = 500;

                await Task.Delay(callDelayMilliseconds, this.tokenSource.Token)
                    .ContinueWith(
                        async t => await this.SearchItemAsync(0, this.tokenSource.Token),
                        this.tokenSource.Token,
                        TaskContinuationOptions.NotOnCanceled,
                        TaskScheduler.Current);
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        #endregion
    }
}

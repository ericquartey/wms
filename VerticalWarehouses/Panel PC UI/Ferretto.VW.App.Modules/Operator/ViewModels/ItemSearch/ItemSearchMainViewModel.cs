using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Printing.ExportHelpers;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchMainViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private const int DefaultPageSize = 20;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 10;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IWmsDataProvider wmsDataProvider;

        private int? areaId;

        private double? availableQuantity;

        private int currentItemIndex;

        private double? inputQuantity;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isSearching;

        private List<ItemInfo> items = new List<ItemInfo>();

        private int maxKnownIndexSelection;

        private DelegateCommand requestItemPickCommand;

        private DelegateCommand<object> scrollCommand;

        private string searchItem;

        private ItemInfo selectedItem;

        private DelegateCommand showItemDetailsCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public ItemSearchMainViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineIdentityWebService identityService,
            IBayManager bayManager,
            IMachineAreasWebService areasWebService,
            IBarcodeReaderService barcodeReaderService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
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
                this.ShowItemDetails,
                this.CanShowItemDetails));

        public IList<ItemInfo> Items => new List<ItemInfo>(this.items);

        public override bool KeepAlive => true;

        public ICommand RequestItemPickCommand =>
            this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
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
                Task.Run(async () => await this.SelectNextItemAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                this.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Methods

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (Enum.TryParse<UserAction>(e.UserAction, out var userAction))
            {
                switch (userAction)
                {
                    case UserAction.FilterItems:
                        await this.ShowItemDetailsByBarcodeAsync(e);

                        break;

                    case UserAction.PickItem:
                        // TODO da definire con Danilo

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
        }

        public override async Task OnAppearedAsync()
        {
            this.InputQuantity = null;

            await base.OnAppearedAsync();
        }

        public async Task RequestItemPickAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPick = true;

                await this.wmsDataProvider.PickAsync(
                    this.SelectedItem.Id,
                    this.InputQuantity.Value);

                this.ShowNotification(
                    string.Format(
                        Resources.OperatorApp.PickRequestWasAccepted,
                        this.SelectedItem.Code,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);

                await this.RefreshItemsAsync();
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
                var groupByProduct = false;
                var distinctBySerialNumber = false;

                var newItems = await this.areasWebService.GetProductsAsync(
                    this.areaId.Value,
                    skip,
                    DefaultPageSize,
                    this.searchItem,
                    groupByProduct,
                    distinctBySerialNumber,
                    cancellationToken);

                var mappedItems = newItems.Select(i => new ItemInfo(i, this.bayManager.Identity.Id)).ToArray();
                this.items.AddRange(mappedItems);
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

        private async Task RefreshItemsAsync()
        {
            var startIndex = ((this.maxKnownIndexSelection - ItemsVisiblePageSize) > 0) ? this.maxKnownIndexSelection - ItemsVisiblePageSize : 0;
            this.currentItemIndex = startIndex;

            this.tokenSource = new CancellationTokenSource();
            await this.SearchItemAsync(startIndex, this.tokenSource.Token);
        }

        private void Scroll(object parameter)
        {
            ScrollChangedEventArgs scrollChangedEventArgs = parameter as ScrollChangedEventArgs;
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

        private void ShowItemDetails()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.ITEM_DETAILS,
                this.SelectedItem,
                trackCurrentView: true);
        }

        private async Task ShowItemDetailsByBarcodeAsync(UserActionEventArgs e)
        {
            var itemCode = e.GetItemCode();
            if (itemCode != null)
            {
                try
                {
                    var items = await this.areasWebService.GetProductsAsync(
                        this.areaId.Value,
                        0,
                        1,
                        itemCode,
                        false,
                        false);

                    if (items.Any())
                    {
                        this.ClearNotifications();
                        this.SearchItem = itemCode;
                    }
                    else
                    {
                        this.ShowNotification(string.Format(Ferretto.VW.App.Resources.OperatorApp.NoItemWithCodeIsAvailable, itemCode), Services.Models.NotificationSeverity.Warning);
                    }
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
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

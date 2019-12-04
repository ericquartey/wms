using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemSearchMainViewModel : BaseOperatorViewModel
    {
        #region Fields

        private const int DefaultPageSize = 20;

        private const int ItemsToCheckBeforeLoad = 2;

        private const int ItemsVisiblePageSize = 12;

        private readonly IAreasDataService areasDataService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly List<Item> items = new List<Item>();

        private readonly IWmsDataProvider wmsDataProvider;

        private int? areaId;

        private double? availableQuantity;

        private int currentItemIndex;

        private int? inputQuantity;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isSearching;

        private bool isWaitingForResponse;

        private int maxKnownIndexSelection;

        private DelegateCommand requestItemPickCommand;

        private string searchItem;

        private Item selectedItem;

        private DelegateCommand selectNextItemCommand;

        private DelegateCommand selectPreviousItemCommand;

        private DelegateCommand showItemDetailsCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public ItemSearchMainViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineIdentityWebService identityService,
            IBayManager bayManager,
            IAreasDataService areasDataService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.areasDataService = areasDataService ?? throw new ArgumentNullException(nameof(areasDataService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.maxKnownIndexSelection = ItemsVisiblePageSize;
        }

        #endregion

        #region Properties

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value);
        }

        public ICommand DownDataGridButtonCommand =>
            this.selectNextItemCommand
            ??
            (this.selectNextItemCommand = new DelegateCommand(
                async () => await this.SelectNextItemAsync(),
                this.CanSelectNextItem));

        public override EnableMask EnableMask => EnableMask.Any;

        public int? InputQuantity
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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
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

        public IList<Item> Items => new List<Item>(this.items);

        public override bool KeepAlive => true;

        public ICommand RequestItemPickCommand =>
            this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
                this.CanRequestItemPick));

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

        public Item SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    var machineId = this.bayManager.Identity.Id;
                    this.AvailableQuantity = this.SelectedItem?.Machines.SingleOrDefault(m => m.Id == machineId)?.AvailableQuantityItem;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand UpDataGridButtonCommand =>
            this.selectPreviousItemCommand
            ??
            (this.selectPreviousItemCommand = new DelegateCommand(
                async () => await this.SelectPreviousItemAsync(),
                this.CanSelectPreviousItem));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.InputQuantity = null;
            this.SearchItem = null;
            var machineIdentity = await this.identityService.GetAsync();
            this.areaId = machineIdentity.AreaId;
            this.tokenSource = new CancellationTokenSource();

            await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
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
                var newItems = await this.areasDataService.GetItemsAsync(
                this.areaId.Value,
                skip,
                DefaultPageSize,
                null,
                null,
                this.searchItem,
                cancellationToken);

                foreach (var item in newItems)
                {
                    this.items.Add(item);
                }
            }
            catch (Exception ex)
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
                this.RaisePropertyChanged(nameof(this.Items));
            }
            
            this.RaisePropertyChanged(nameof(this.Items));

            this.SetCurrentIndex(selectedItemId);
            this.AdjustItemsAppearance();
            this.SetSelectedItem();
        }

        public async Task SelectNextItemAsync()
        {
            this.currentItemIndex++;
            if (this.currentItemIndex > this.maxKnownIndexSelection)
            {
                this.maxKnownIndexSelection = this.currentItemIndex;
            }

            if (this.currentItemIndex > Math.Max(this.items.Count - ItemsToCheckBeforeLoad, DefaultPageSize - ItemsToCheckBeforeLoad))
            {
                this.IsSearching = true;
                this.tokenSource = new CancellationTokenSource();
                this.IsBusyLoadingNextPage = true;
                await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
            }

            this.SetSelectedItem();
        }

        public async Task SelectPreviousItemAsync()
        {
            this.currentItemIndex--;
            if ((this.maxKnownIndexSelection - ItemsVisiblePageSize) > this.currentItemIndex)
            {
                this.maxKnownIndexSelection--;
            }

            if (this.currentItemIndex > (DefaultPageSize - ItemsToCheckBeforeLoad))
            {
                this.IsSearching = true;
                this.tokenSource = new CancellationTokenSource();
                await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
            }

            this.SetSelectedItem();
        }

        private void AdjustItemsAppearance()
        {
            if (this.maxKnownIndexSelection == 0)
            {
                this.maxKnownIndexSelection = Math.Min(this.items.Count(), ItemsVisiblePageSize);
            }

            if (this.maxKnownIndexSelection >= ItemsVisiblePageSize
                &&
                this.Items.Count() > this.maxKnownIndexSelection)
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

        private void RaiseCanExecuteChanged()
        {
            this.requestItemPickCommand?.RaiseCanExecuteChanged();
            this.showItemDetailsCommand?.RaiseCanExecuteChanged();
            this.selectPreviousItemCommand?.RaiseCanExecuteChanged();
            this.selectNextItemCommand?.RaiseCanExecuteChanged();
        }

        private void SetCurrentIndex(int? itemId)
        {
            if (itemId.HasValue
                &&
                this.items.FirstOrDefault(l => l.Id == itemId.Value) is Item itemFound)
            {
                this.currentItemIndex = this.items.IndexOf(itemFound);
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

            var elemSelected = this.items.ElementAt(this.currentItemIndex);
            if (elemSelected?.Id != this.SelectedItem?.Id)
            {
                this.SelectedItem = elemSelected;
            }
        }

        private void ShowItemDetails()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.ITEM_DETAILS,
                this.SelectedItem,
                trackCurrentView: true);
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

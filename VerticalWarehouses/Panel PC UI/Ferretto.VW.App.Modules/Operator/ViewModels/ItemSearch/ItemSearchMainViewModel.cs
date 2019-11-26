using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemSearchMainViewModel : BaseMainViewModel
    {
        #region Fields

        private const int DefaultPageSize = 20;

        private readonly IAreasDataService areasDataService;

        private readonly IMachineIdentityWebService identityService;

        private readonly List<Item> items = new List<Item>();

        private readonly IItemSearchedModel itemSearchViewModel;

        private readonly IWmsDataProvider wmsDataProvider;

        private int? areaId;

        private double? availableQuantity;

        private int currentItemIndex;

        private int? inputQuantity;

        private bool isBusyLoadingNextPage;

        private bool isBusyRequestingItemPick;

        private bool isSearching;

        private bool isWaitingForResponse;

        private DelegateCommand requestItemPickCommand;

        private string searchItemCode;

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
            IItemSearchedModel itemSearchedModel,
            IAreasDataService areasDataService)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemSearchViewModel = itemSearchedModel ?? throw new ArgumentNullException(nameof(itemSearchedModel));
            this.areasDataService = areasDataService ?? throw new ArgumentNullException(nameof(areasDataService));
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

        public IEnumerable<Item> Items => new BindingList<Item>(this.items);

        public ICommand RequestItemPickCommand =>
            this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
                this.CanRequestItemPick));

        public string SearchItemCode
        {
            get => this.searchItemCode;
            set
            {
                if (this.SetProperty(ref this.searchItemCode, value))
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
                    this.itemSearchViewModel.SelectedItem = this.selectedItem;
                    this.AvailableQuantity = this.SelectedItem?.TotalAvailable;

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

            if (this.items != null
               &&
               this.selectedItem != null)
            {
                return;
            }

            this.currentItemIndex = 0;
            this.InputQuantity = null;
            this.items.Clear();
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

                this.ShowNotification($"TODO**Successfully requested {this.InputQuantity} pieces of item '{this.SelectedItem.Code}'.", Services.Models.NotificationSeverity.Success);
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
            }

            try
            {
                var newItems = await this.areasDataService.GetItemsAsync(
                    this.areaId.Value,
                    skip,
                    DefaultPageSize,
                    null,
                    null,
                    this.searchItemCode,
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
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Items));

                if (skip == 0 || this.currentItemIndex >= this.items.Count - 1)
                {
                    this.SelectedItem = this.items?.FirstOrDefault();
                    this.currentItemIndex = 0;
                }

                this.IsSearching = false;
                this.IsBusyLoadingNextPage = false;
            }
        }

        public async Task SelectNextItemAsync()
        {
            this.currentItemIndex++;

            if (this.currentItemIndex > Math.Max(this.items.Count - 2, DefaultPageSize - 2))
            {
                this.IsSearching = true;
                this.tokenSource = new CancellationTokenSource();
                this.IsBusyLoadingNextPage = true;
                await this.SearchItemAsync(this.currentItemIndex + 2, this.tokenSource.Token);
            }

            this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
        }

        public async Task SelectPreviousItemAsync()
        {
            this.currentItemIndex--;

            if (this.currentItemIndex > (DefaultPageSize - 2))
            {
                this.IsSearching = true;
                this.tokenSource = new CancellationTokenSource();
                await this.SearchItemAsync(this.currentItemIndex + 2, this.tokenSource.Token);
            }

            this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
        }

        private bool CanRequestItemPick()
        {
            return
                this.SelectedItem != null
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

        private void RaiseCanExecuteChanged()
        {
            this.requestItemPickCommand?.RaiseCanExecuteChanged();
            this.showItemDetailsCommand?.RaiseCanExecuteChanged();
            this.selectPreviousItemCommand?.RaiseCanExecuteChanged();
            this.selectNextItemCommand?.RaiseCanExecuteChanged();
        }

        private void ShowItemDetails()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.ITEM_DETAILS,
                null,
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

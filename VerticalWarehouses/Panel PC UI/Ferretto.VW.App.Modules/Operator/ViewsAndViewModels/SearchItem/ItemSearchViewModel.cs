using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.SearchItem
{
    public class ItemSearchViewModel : BaseViewModel, IItemSearchViewModel
    {
        #region Fields

        private const int DEFAULT_DELAY = 300;

        private const int DEFAULT_QUANTITY_ITEM = 20;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityService identityService;

        private readonly IItemsDataService itemsDataService;

        private readonly Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService navigationService;

        private readonly IStatusMessageService statusMessageService;

        private readonly IWmsDataProvider wmsDataProvider;

        private int? areaId;

        private string availableQuantity;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private bool isSearching;

        private ICommand itemCallCommand;

        private ICommand itemDetailButtonCommand;

        private List<Item> items;

        private string requestedQuantity;

        private string searchItemCode;

        private Item selectedItem;

        private CancellationTokenSource tokenSource;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public ItemSearchViewModel(
            IStatusMessageService statusMessageService,
            IWmsDataProvider wmsDataProvider,
            IBayManager bayManager,
            Operator.Interfaces.INavigationService navigationService,
            IMachineIdentityService identityService,
            IItemsDataService itemsDataService)
        {
            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (itemsDataService == null)
            {
                throw new ArgumentNullException(nameof(itemsDataService));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.identityService = identityService;
            this.statusMessageService = statusMessageService;
            this.wmsDataProvider = wmsDataProvider;
            this.bayManager = bayManager;
            this.itemsDataService = itemsDataService;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
            this.currentItemIndex = 0;
            this.requestedQuantity = "0";
        }

        #endregion

        #region Properties

        public string AvailableQuantity { get => this.availableQuantity; set => this.SetProperty(ref this.availableQuantity, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public bool IsSearching { get => this.isSearching; set => this.SetProperty(ref this.isSearching, value); }

        public ICommand ItemCallCommand => this.itemCallCommand ?? (this.itemCallCommand = new DelegateCommand(() => this.ItemCallMethodAsync(), this.CanItemCall));

        public ICommand ItemDetailButtonCommand =>
            this.itemDetailButtonCommand
            ??
            (this.itemDetailButtonCommand = new DelegateCommand(() =>
                {
                    if (this.SelectedItem != null)
                    {
                        this.navigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>(this.SelectedItem);
                    }
                },
                this.CanShowDetails));

        public BindingList<Item> Items => new BindingList<Item>(this.items);

        public string RequestedQuantity
        {
            get => this.requestedQuantity;
            set
            {
                if (this.SetProperty(ref this.requestedQuantity, value))
                {
                    ((DelegateCommand)this.ItemCallCommand).RaiseCanExecuteChanged();
                }
            }
        }

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
                    ((DelegateCommand)this.ItemCallCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)this.ItemDetailButtonCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true)));

        #endregion

        #region Methods

        public async Task ChangeSelectedItemAsync(bool isUp)
        {
            if (this.Items == null)
            {
                return;
            }

            if (this.Items.Count() != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= this.Items.Count())
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : this.Items.Count() - 1;
                }

                if (this.currentItemIndex > (DEFAULT_QUANTITY_ITEM - 2) &&
                    this.currentItemIndex >= this.Items.Count() - 2)
                {
                    this.IsSearching = true;
                    this.tokenSource = new CancellationTokenSource();
                    await this.SearchItemAsync(this.currentItemIndex + 2, this.tokenSource.Token);
                }

                this.SelectedItem = this.Items?.ToList()[this.currentItemIndex];
            }
        }

        public async Task ItemCallMethodAsync()
        {
            var itemToPick = this.SelectedItem;
            if (itemToPick == null)
            {
                return;
            }

            if (!int.TryParse(this.requestedQuantity, out var qty))
            {
                return;
            }

            var success = await this.wmsDataProvider.PickAsync(
                itemToPick.Id,
                2, // TODO remove this hardcoded value
                this.bayManager.BayNumber,
                qty);

            if (success)
            {
                this.statusMessageService.Notify(
                    $"Successfully called {qty} pieces of item {itemToPick.Id}.",
                    StatusMessageLevel.Success);
            }
            else
            {
                this.statusMessageService.Notify(
                    $"Couldn't get {qty} pieces of item {itemToPick.Id}.",
                    StatusMessageLevel.Error);
            }

            this.RequestedQuantity = "0";
        }

        public override async Task OnEnterViewAsync()
        {
            if (this.items != null
                &&
                this.selectedItem != null)
            {
                return;
            }

            this.currentItemIndex = 0;
            this.RequestedQuantity = "0";
            this.items = new List<Item>();
            var machineIdentity = await this.identityService.GetAsync();
            this.areaId = machineIdentity.AreaId;
            this.tokenSource = new CancellationTokenSource();
            await this.SearchItemAsync(this.currentItemIndex, this.tokenSource.Token);
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
                var newItems = await this.itemsDataService.GetAllAsync(
                    skip,
                    DEFAULT_QUANTITY_ITEM,
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
                this.statusMessageService.Notify(ex);
                this.items.Clear();
                this.SelectedItem = null;
                this.currentItemIndex = 0;
            }

            this.RaisePropertyChanged(nameof(this.Items));
            if (skip == 0)
            {
                this.SelectedItem = this.items?.FirstOrDefault();
            }
            this.IsSearching = false;
        }

        private bool CanItemCall()
        {
            if (this.selectedItem == null)
            {
                return false;
            }

            if (int.TryParse(this.requestedQuantity, out var qty))
            {
                if (qty <= 0)
                {
                    return false;
                }

                return this.SelectedItem.TotalAvailable >= qty;
            }

            return false;
        }

        private bool CanShowDetails()
        {
            return this.SelectedItem != null;
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

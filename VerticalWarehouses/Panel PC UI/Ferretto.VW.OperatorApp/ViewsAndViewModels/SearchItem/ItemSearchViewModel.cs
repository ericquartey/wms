using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemSearchViewModel : BaseViewModel, IItemSearchViewModel
    {
        #region Fields

        private const int DEFAULT_DELAY = 300;

        private const int DEFAULT_QUANTITY_ITEM = 20;

        private readonly IBayManager bayManager;

        private readonly CustomControlArticleDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly SynchronizationContext uiContext;

        private readonly IWmsDataProvider wmsDataProvider;

        private string availableQuantity;

        private int currentItemIndex;

        private BindableBase dataGridViewModel;

        private ICommand downDataGridButtonCommand;

        private bool hasUserTyped;

        private bool isItemCallButtonActive = true;

        private bool isSearching;

        private ICommand itemCallCommand;

        private ICommand itemDetailButtonCommand;

        private ObservableCollection<WMS.Data.WebAPI.Contracts.Item> loadedItems;

        private int requestedQuantity;

        private string searchArticleCode;

        private Timer timer;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public ItemSearchViewModel(
            IEventAggregator eventAggregator,
            IStatusMessageService statusMessageService,
            IWmsDataProvider wmsDataProvider,
            IBayManager bayManager,
            INavigationService navigationService,
            ICustomControlArticleDataGridViewModel articleDataGridViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

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

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.eventAggregator = eventAggregator;
            this.StatusMessageService = statusMessageService;
            this.wmsDataProvider = wmsDataProvider;
            this.bayManager = bayManager;
            this.navigationService = navigationService;
            this.ArticleDataGridViewModel = articleDataGridViewModel;
            this.dataGridViewModelRef = articleDataGridViewModel as CustomControlArticleDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
            this.uiContext = SynchronizationContext.Current;
            this.loadedItems = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
        }

        #endregion

        #region Properties

        public ICustomControlArticleDataGridViewModel ArticleDataGridViewModel { get; }

        public string AvailableQuantity { get => this.availableQuantity; set => this.SetProperty(ref this.availableQuantity, value); }

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public bool IsItemCallButtonActive { get => this.isItemCallButtonActive; set => this.SetProperty(ref this.isItemCallButtonActive, value); }

        public bool IsSearching { get => this.isSearching; set => this.SetProperty(ref this.isSearching, value); }

        public ICommand ItemCallCommand =>
            this.itemCallCommand
            ??
            (this.itemCallCommand = new DelegateCommand(() => this.ItemCallMethodAsync()));

        public ICommand ItemDetailButtonCommand =>
            this.itemDetailButtonCommand
            ??
            (this.itemDetailButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>(this.dataGridViewModelRef.SelectedArticle);
                }));

        public int RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        public string SearchArticleCode
        {
            get => this.searchArticleCode;
            set
            {
                this.SetProperty(ref this.searchArticleCode, value);

                if (!this.hasUserTyped)
                {
                    this.hasUserTyped = true;
                    this.IsSearching = true;
                    this.timer = new Timer(this.SearchItemAsync, new AutoResetEvent(false), DEFAULT_DELAY, 0);
                }
                this.timer?.Change(DEFAULT_DELAY, 0);
            }
        }

        public IStatusMessageService StatusMessageService { get; }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true)));

        #endregion

        #region Methods

        public async void ChangeSelectedItemAsync(bool isUp)
        {
            if (this.dataGridViewModel is CustomControlArticleDataGridViewModel dataGrid && (dataGrid.Articles != null && dataGrid.Articles?.Count != 0))
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= dataGrid.Articles.Count)
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : dataGrid.Articles.Count - 1;
                }

                if (this.currentItemIndex >= dataGrid.Articles.Count - 2)
                {
                    this.IsSearching = true;
                    try
                    {
                        var items = await this.wmsDataProvider.GetItemsAsync(this.searchArticleCode, this.currentItemIndex, DEFAULT_QUANTITY_ITEM);
                        this.IsSearching = false;

                        if (items != null && items.Any())
                        {
                            var viewItems = new ObservableCollection<DataGridItem>();
                            var random = new Random();
                            foreach (var item in items)
                            {
                                var machines = string.Empty;
                                if (item.Machines != null)
                                {
                                    for (var j = 0; j < item.Machines.Count; j++)
                                    {
                                        machines = string.Concat(machines, $" {item.Machines[j].Id},");
                                    }
                                }
                                else
                                {
                                    for (var k = 0; k < random.Next(1, 4); k++)
                                    {
                                        machines = string.Concat(machines, $" {random.Next(1, 200)},");
                                    }
                                }
                                var gridItem = new DataGridItem
                                {
                                    Article = item.Code,
                                    Description = item.Description,
                                    AvailableQuantity = item.TotalAvailable,
                                    ImageCode = item.Image,
                                    Machine = machines
                                };
                                viewItems.Add(gridItem);
                                this.loadedItems.Add(item);
                            }
                            for (var i = 0; i < viewItems.Count; i++)
                            {
                                (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles.Add(viewItems[i]);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.IsSearching = false;
                    }

                    this.AvailableQuantity = (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles[this.currentItemIndex].AvailableQuantity.ToString();
                    (this.DataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles[this.currentItemIndex];
                }
            }
        }

        public async void ItemCallMethodAsync()
        {
            this.IsItemCallButtonActive = false;

            var itemToPick = this.loadedItems[this.currentItemIndex];

            var success = await this.wmsDataProvider.PickAsync(
                itemToPick.Id,
                2,
                this.bayManager.BayId,
                this.RequestedQuantity);

            if (success)
            {
                this.StatusMessageService.Notify(
                    $"Successfully called {this.RequestedQuantity} pieces of item {this.loadedItems[this.currentItemIndex].Id}.",
                    StatusMessageLevel.Success);
            }
            else
            {
                this.StatusMessageService.Notify(
                    $"Couldn't get {this.RequestedQuantity} pieces of item {this.loadedItems[this.currentItemIndex].Id}.",
                    StatusMessageLevel.Error);
            }

            this.RequestedQuantity = 0;
            this.IsItemCallButtonActive = true;
        }

        public override async Task OnEnterViewAsync()
        {
            var viewItems = new ObservableCollection<DataGridItem>();

            try
            {
                this.loadedItems = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>(
                    await this.wmsDataProvider.GetItemsAsync(" ", 0, DEFAULT_QUANTITY_ITEM));

                if (this.loadedItems != null && this.loadedItems.Any())
                {
                    var random = new Random();
                    foreach (var item in this.loadedItems)
                    {
                        var machines = string.Empty;
                        if (item.Machines != null)
                        {
                            for (var j = 0; j < item.Machines.Count; j++)
                            {
                                machines = string.Concat(machines, $" {item.Machines[j].Id},");
                            }
                        }
                        else
                        {
                            for (var k = 0; k < random.Next(1, 4); k++)
                            {
                                machines = string.Concat(machines, $" {random.Next(1, 200)},");
                            }
                        }

                        var gridItem = new DataGridItem
                        {
                            Article = item.Code,
                            Description = item.Description,
                            AvailableQuantity = item.TotalAvailable,
                            ImageCode = item.Image,
                            Machine = machines
                        };
                        viewItems.Add(gridItem);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear(), null);
            }

            this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems, null);
            this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = viewItems[0], null);
            this.currentItemIndex = 0;
            this.AvailableQuantity = viewItems[0].AvailableQuantity.ToString();
        }

        public async void SearchItemAsync(object stateInfo)
        {
            var autoEvent = (AutoResetEvent)stateInfo;

            try
            {
                this.loadedItems = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>(
                    await this.wmsDataProvider.GetItemsAsync(this.searchArticleCode, 0, DEFAULT_QUANTITY_ITEM));

                if (this.loadedItems?.Any() == true)
                {
                    var viewItems = new ObservableCollection<DataGridItem>();
                    var random = new Random();
                    foreach (var item in this.loadedItems)
                    {
                        var machines = string.Empty;
                        if (item.Machines != null)
                        {
                            for (var j = 0; j < item.Machines.Count; j++)
                            {
                                machines = string.Concat(machines, $" {item.Machines[j].Id},");
                            }
                        }
                        else
                        {
                            for (var k = 0; k < random.Next(1, 4); k++)
                            {
                                machines = string.Concat(machines, $" {random.Next(1, 200)},");
                            }
                        }
                        var gridItem = new DataGridItem
                        {
                            Article = item.Code,
                            Description = item.Description,
                            AvailableQuantity = item.TotalAvailable,
                            ImageCode = item.Image,
                            Machine = machines
                        };

                        viewItems.Add(gridItem);
                    }

                    this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems, null);
                    this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = viewItems[0], null);
                    this.currentItemIndex = 0;
                    this.AvailableQuantity = viewItems[0].AvailableQuantity.ToString();
                }
            }
            catch (Exception)
            {
                this.currentItemIndex = 0;
                this.IsSearching = false;
                this.hasUserTyped = false;
            }
            finally
            {
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear(), null);
            }

            autoEvent.Set();
            this.timer.Dispose();
            this.IsSearching = false;
            this.hasUserTyped = false;
        }

        #endregion
    }
}

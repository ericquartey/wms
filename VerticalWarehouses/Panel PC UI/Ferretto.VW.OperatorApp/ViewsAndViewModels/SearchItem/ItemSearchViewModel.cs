using System.Threading.Tasks;
using System;
using System.Windows.Input;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CustomControls;
using System.Collections.ObjectModel;
using System.Threading;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemSearchViewModel : BindableBase, IItemSearchViewModel
    {
        #region Fields

        private const int DEFAULT_DELAY = 300;

        private const int DEFAULT_QUANTITY_ITEM = 10;

        private readonly SynchronizationContext uiContext;

        private string availableQuantity;

        private IUnityContainer container;

        private int currentItemIndex;

        private BindableBase dataGridViewModel;

        private CustomControlArticleDataGridViewModel dataGridViewModelRef;

        private ICommand downDataGridButtonCommand;

        private IEventAggregator eventAggregator;

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

        private IWmsDataProvider wmsDataProvider;

        #endregion

        #region Constructors

        public ItemSearchViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.uiContext = SynchronizationContext.Current;
            this.loadedItems = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
        }

        #endregion

        #region Properties

        public string AvailableQuantity { get => this.availableQuantity; set => this.SetProperty(ref this.availableQuantity, value); }

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public bool IsItemCallButtonActive { get => this.isItemCallButtonActive; set => this.SetProperty(ref this.isItemCallButtonActive, value); }

        public bool IsSearching { get => this.isSearching; set => this.SetProperty(ref this.isSearching, value); }

        public ICommand ItemCallCommand => this.itemCallCommand ?? (this.itemCallCommand = new DelegateCommand(() => this.ItemCallMethodAsync()));

        public ICommand ItemDetailButtonCommand => this.itemDetailButtonCommand ?? (this.itemDetailButtonCommand = new DelegateCommand(() =>
                                {
                                    NavigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>(this.dataGridViewModelRef.SelectedArticle);
                                }));

        public BindableBase NavigationViewModel { get; set; }

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
                    var items = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
                    try
                    {
                        items = await this.wmsDataProvider.GetItemsAsync(this.searchArticleCode, this.currentItemIndex, DEFAULT_QUANTITY_ITEM);
                        this.IsSearching = false;
                    }
                    catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
                    {
                        this.IsSearching = false;
                    }
                    catch (Exception)
                    {
                        this.IsSearching = false;
                    }
                    if (items != null && items.Count > 0)
                    {
                        var viewItems = new ObservableCollection<TestArticle>();
                        var random = new Random();
                        for (var i = 0; i < items.Count; i++)
                        {
                            var machines = string.Empty;
                            if (items[i].Machines != null)
                            {
                                for (var j = 0; j < items[i].Machines.Count; j++)
                                {
                                    machines = string.Concat(machines, $" {items[i].Machines[j].Id},");
                                }
                            }
                            else
                            {
                                for (var k = 0; k < random.Next(1, 4); k++)
                                {
                                    machines = string.Concat(machines, $" {random.Next(1, 200)},");
                                }
                            }
                            var item = new TestArticle
                            {
                                Article = items[i].Code,
                                Description = items[i].Description,
                                AvailableQuantity = items[i].TotalAvailable,
                                Machine = machines
                            };
                            viewItems.Add(item);
                            this.loadedItems.Add(items[i]);
                        }
                        for (int i = 0; i < viewItems.Count; i++)
                        {
                            (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles.Add(viewItems[i]);
                        }
                    }
                }
                this.AvailableQuantity = (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles[this.currentItemIndex].AvailableQuantity.ToString();
                (this.DataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles[this.currentItemIndex];
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlArticleDataGridViewModel>() as CustomControlArticleDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;
            this.wmsDataProvider = this.container.Resolve<IWmsDataProvider>();
        }

        public async void ItemCallMethodAsync()
        {
            var bay = this.container.Resolve<IBayManager>();
            this.IsItemCallButtonActive = false;

            var successfullRequest = await this.wmsDataProvider.PickAsync(this.loadedItems[this.currentItemIndex].Id, 2, bay.BayId, this.RequestedQuantity);
            if (successfullRequest)
            {
                this.container.Resolve<IFeedbackNotifier>().Notify($"Successfully called {this.RequestedQuantity} pieces of item {this.loadedItems[this.currentItemIndex].Id}.");
                this.RequestedQuantity = 0;
                this.IsItemCallButtonActive = true;
            }
            else
            {
                this.container.Resolve<IFeedbackNotifier>().Notify($"Couldn't get {this.RequestedQuantity} pieces of item {this.loadedItems[this.currentItemIndex].Id}.");
                this.RequestedQuantity = 0;
                this.IsItemCallButtonActive = true;
            }
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public async void SearchItemAsync(object stateInfo)
        {
            var autoEvent = (AutoResetEvent)stateInfo;
            var items = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
            try
            {
                items = await this.wmsDataProvider.GetItemsAsync(this.searchArticleCode, 0, DEFAULT_QUANTITY_ITEM);
            }
            catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
            {
                this.currentItemIndex = 0;
                this.IsSearching = false;
                this.hasUserTyped = false;
            }
            catch (Exception)
            {
                this.currentItemIndex = 0;
                this.IsSearching = false;
                this.hasUserTyped = false;
            }
            finally
            {
                this.loadedItems = null;
                this.loadedItems = items;
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear(), null);
            }
            if (items != null && items.Count > 0)
            {
                var viewItems = new ObservableCollection<TestArticle>();
                var random = new Random();
                for (var i = 0; i < items.Count; i++)
                {
                    var machines = string.Empty;
                    if (items[i].Machines != null)
                    {
                        for (var j = 0; j < items[i].Machines.Count; j++)
                        {
                            machines = string.Concat(machines, $" {items[i].Machines[j].Id},");
                        }
                    }
                    else
                    {
                        for (var k = 0; k < random.Next(1, 4); k++)
                        {
                            machines = string.Concat(machines, $" {random.Next(1, 200)},");
                        }
                    }
                    var item = new TestArticle
                    {
                        Article = items[i].Code,
                        Description = items[i].Description,
                        AvailableQuantity = items[i].TotalAvailable,
                        Machine = machines
                    };
                    viewItems.Add(item);
                }
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems, null);
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = viewItems[0], null);
                this.currentItemIndex = 0;
                this.AvailableQuantity = viewItems[0].AvailableQuantity.ToString();
            }
            autoEvent.Set();
            this.timer.Dispose();
            this.IsSearching = false;
            this.hasUserTyped = false;
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}

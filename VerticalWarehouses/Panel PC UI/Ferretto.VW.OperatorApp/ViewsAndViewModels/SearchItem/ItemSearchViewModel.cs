using System.Linq;
using System.Threading.Tasks;
using System;
using System.Windows.Input;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CustomControls;
using System;
using Ferretto.VW.MAS_AutomationService.Contracts;
using System.Collections.ObjectModel;
using System.Threading;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemSearchViewModel : BindableBase, IItemSearchViewModel
    {
        #region Fields

        private const int DEFAULT_DELAY = 300;

        private const int DEFAULT_QUANTITY_ITEM = 10;

        private readonly SynchronizationContext uiContext;

        private IUnityContainer container;

        private int currentItemIndex;

        private BindableBase dataGridViewModel;

        private CustomControlArticleDataGridViewModel dataGridViewModelRef;

        private ICommand downDataGridButtonCommand;

        private IEventAggregator eventAggregator;

        private bool hasUserTyped;

        private bool isSearching;

        private ICommand itemCallCommand;

        private ICommand itemDetailButtonCommand;

        private IItemsDataService itemsDataService;

        private string searchArticleCode;

        private Timer timer;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public ItemSearchViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.uiContext = SynchronizationContext.Current;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public bool IsSearching { get => this.isSearching; set => this.SetProperty(ref this.isSearching, value); }

        public ICommand ItemCallCommand => this.itemCallCommand ?? (this.itemCallCommand = new DelegateCommand(() => ));

        public ICommand ItemDetailButtonCommand => this.itemDetailButtonCommand ?? (this.itemDetailButtonCommand = new DelegateCommand(() =>
                                {
                                    NavigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>(this.dataGridViewModelRef.SelectedArticle);
                                }));

        public BindableBase NavigationViewModel { get; set; }

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
                        items = await this.itemsDataService.GetAllAsync(search: this.searchArticleCode, skip: this.currentItemIndex, take: DEFAULT_QUANTITY_ITEM);
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
                                Machine = machines
                            };
                            viewItems.Add(item);
                        }
                        for (int i = 0; i < viewItems.Count; i++)
                        {
                            (this.DataGridViewModel as CustomControlArticleDataGridViewModel).Articles.Add(viewItems[i]);
                        }
                    }
                }
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
            this.itemsDataService = this.container.Resolve<IItemsDataService>();
        }

        public async void ItemCallMethodAsync()
        {
            try
            {
                await this.itemsDataService.PickAsync(1, new ItemOptions { AreaId = 2, BayId = 2, RequestedQuantity = 10, RunImmediately = true });
            }
            catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
            {
                // TODO inform the operator of an error during the Item Call request to the WMS service
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
                items = await this.itemsDataService.GetAllAsync(search: this.searchArticleCode, take: DEFAULT_QUANTITY_ITEM);
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
                        Machine = machines
                    };
                    viewItems.Add(item);
                }
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems, null);
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).SelectedArticle = viewItems[0], null);
                this.currentItemIndex = 0;
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

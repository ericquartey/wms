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

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlArticleDataGridViewModel dataGridViewModelRef;

        private int delayBeforeRequest = DEFAULT_DELAY;

        private IEventAggregator eventAggregator;

        private bool hasUserTyped;

        private bool isSearching;

        private ICommand itemDetailButtonCommand;

        private IItemsDataService itemsDataService;

        private string searchArticleCode;

        private Timer timer;

        private SynchronizationContext uiContext;

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

        public bool IsSearching { get => this.isSearching; set => this.SetProperty(ref this.isSearching, value); }

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

                this.delayBeforeRequest = DEFAULT_DELAY;
                if (!this.hasUserTyped)
                {
                    this.hasUserTyped = true;
                    this.timer = new Timer(this.Method, new AutoResetEvent(false), DEFAULT_DELAY, 0);
                }
                this.timer?.Change(DEFAULT_DELAY, 0);
            }
        }

        #endregion

        #region Methods

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

        public async void Method(object stateInfo)
        {
            var autoEvent = (AutoResetEvent)stateInfo;
            var items = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
            try
            {
                items = await this.itemsDataService.GetAllAsync(search: this.searchArticleCode, take: DEFAULT_QUANTITY_ITEM);
            }
            catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
            {
                this.hasUserTyped = false;
            }
            catch (Exception)
            {
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
            }
            else
            {
                this.uiContext.Send(x => (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear(), null);
            }
            autoEvent.Set();
            this.timer.Dispose();
            this.hasUserTyped = false;
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
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

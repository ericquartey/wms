using System.Linq;
using System.Threading.Tasks;
using System;
using System.Windows.Input;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
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

        private const int DEFAULT_QUANTITY_ITEM = 20;

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlArticleDataGridViewModel dataGridViewModelRef;

        private int delayBeforeRequest = DEFAULT_DELAY;

        private IEventAggregator eventAggregator;

        private bool hasUserTyped;

        private ICommand itemDetailButtonCommand;

        private ObservableCollection<Item> items;

        private IOperatorService operatorService;

        private string searchArticleCode;

        #endregion

        #region Constructors

        public ItemSearchViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand ItemDetailButtonCommand => this.itemDetailButtonCommand ?? (this.itemDetailButtonCommand = new DelegateCommand(() =>
                {
                    NavigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>(this.dataGridViewModelRef.SelectedArticle);
                }));

        public ObservableCollection<Item> Items
        {
            get => this.items;
            set
            {
                this.SetProperty(ref this.items, value);
            }
        }

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
                    this.StartDelayAndRequest();
                }
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
            this.operatorService = this.container.Resolve<IOperatorService>();
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public async void StartDelayAndRequest()
        {
            this.hasUserTyped = true;
            while (this.delayBeforeRequest > 0)
            {
                await Task.Delay(10);
                this.delayBeforeRequest -= 10;
            }
            try
            {
                this.items = await this.operatorService.ItemsAsync(this.searchArticleCode, DEFAULT_QUANTITY_ITEM);
            }
            catch (SwaggerException)
            {
                this.hasUserTyped = false;
            }
            catch (Exception)
            {
                this.hasUserTyped = false;
            }
            if (this.items != null && this.items.Count > 0)
            {
                var viewItems = new ObservableCollection<TestArticle>();
                for (int i = 0; i < this.items.Count; i++)
                {
                    string machines = "";
                    if (this.items[i].Machines != null)
                    {
                        for (int j = 0; j < this.items[i].Machines.Count; j++)
                        {
                            machines = string.Concat(machines, $" {this.items[i].Machines[j].Id},");
                        }
                    }
                    else
                    {
                        for (int k = 0; k < new Random().Next(1, 4); k++)
                        {
                            machines = string.Concat(machines, $" {new Random().Next(1, 200)},");
                        }
                    }
                    var item = new TestArticle
                    {
                        Article = this.items[i].Code,
                        Description = this.items[i].Description,
                        Machine = machines
                    };
                    viewItems.Add(item);
                }
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = new ObservableCollection<TestArticle>();
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems;
            }
            else
            {
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = new ObservableCollection<TestArticle>();
            }
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

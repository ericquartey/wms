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

        private ICommand itemDetailButtonCommand;

        private IItemsDataService itemsDataService;

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
            //this.itemsDataService = this.container.Resolve<IItemsDataService>();
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public async void StartDelayAndRequest()
        {
            this.hasUserTyped = true;
            var items = new ObservableCollection<WMS.Data.WebAPI.Contracts.Item>();
            while (this.delayBeforeRequest > 0)
            {
                await Task.Delay(10);
                this.delayBeforeRequest -= 10;
            }
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
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear();
            }
            if (items != null && items.Count > 0)
            {
                var viewItems = new ObservableCollection<TestArticle>();
                var random = new Random();
                for (int i = 0; i < items.Count; i++)
                {
                    string machines = "";
                    if (items[i].Machines != null)
                    {
                        for (int j = 0; j < items[i].Machines.Count; j++)
                        {
                            machines = string.Concat(machines, $" {items[i].Machines[j].Id},");
                        }
                    }
                    else
                    {
                        for (int k = 0; k < random.Next(1, 4); k++)
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
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles = viewItems;
            }
            else
            {
                (this.dataGridViewModel as CustomControlArticleDataGridViewModel).Articles?.Clear();
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

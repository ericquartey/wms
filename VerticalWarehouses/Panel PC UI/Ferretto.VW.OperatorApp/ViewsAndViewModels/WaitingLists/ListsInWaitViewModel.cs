using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists.ListDetail;
using Ferretto.VW.WmsCommunication.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists
{
    public class ListsInWaitViewModel : BaseViewModel, IListsInWaitViewModel
    {
        #region Fields

        private readonly CustomControlListDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private ICommand buttonDown;

        private ICommand buttonUp;

        private int currentSelectedItem;

        private BindableBase dataGridViewModel;

        private ICommand detailListButtonCommand;

        private ObservableCollection<DataGridList> lists;

        #endregion

        #region Constructors

        public ListsInWaitViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            ICustomControlListDataGridViewModel listDataGridViewModel,
            IWmsDataProvider wmsDataProvider)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.wmsDataProvider = wmsDataProvider;
            this.ListDataGridViewModel = listDataGridViewModel;
            this.dataGridViewModelRef = listDataGridViewModel as CustomControlListDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand ButtonDown => this.buttonDown ?? (this.buttonDown = new DelegateCommand(() => this.ChangeSelectedItem(false)));

        public ICommand ButtonUp => this.buttonUp ?? (this.buttonUp = new DelegateCommand(() => this.ChangeSelectedItem(true)));

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DetailListButtonCommand => this.detailListButtonCommand ?? (this.detailListButtonCommand = new DelegateCommand(
            () => this.navigationService.NavigateToView<DetailListInWaitViewModel, IDetailListInWaitViewModel>(this.lists[this.currentSelectedItem])));

        public ICustomControlListDataGridViewModel ListDataGridViewModel { get; }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            var listsFromWms = await this.wmsDataProvider.GetItemLists();

            this.lists = new ObservableCollection<DataGridList>();
            for (var i = 0; i < listsFromWms.Count; i++)
            {
                this.lists.Add(new DataGridList
                {
                    Type = listsFromWms[i].ItemListType.ToString(),
                    List = listsFromWms[i].Id.ToString(),
                    Description = listsFromWms[i].Description,
                    Machines = (listsFromWms[i].Machines == null) ? "---" : listsFromWms[i].Machines.ToString()
                }
                );
            }

            this.dataGridViewModelRef.Lists = this.lists;
            this.dataGridViewModelRef.SelectedList = this.lists[this.currentSelectedItem];
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private void ChangeSelectedItem(bool isUp)
        {
            this.currentSelectedItem = (isUp) ? --this.currentSelectedItem : ++this.currentSelectedItem;
            if (this.currentSelectedItem < 0)
            {
                this.currentSelectedItem = 0;
            }
            if (this.currentSelectedItem >= this.lists.Count)
            {
                this.currentSelectedItem = this.lists.Count - 1;
            }
            (this.dataGridViewModel as CustomControlListDataGridViewModel).SelectedList = this.lists[this.currentSelectedItem];
        }

        #endregion
    }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists
{
    public class ListsInWaitViewModel : BaseViewModel, IListsInWaitViewModel
    {
        #region Fields

        private readonly CustomControlListDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly DataGridList selectedList;

        private BindableBase dataGridViewModel;

        private ICommand detailListButtonCommand;

        private ObservableCollection<DataGridList> lists;

        #endregion

        #region Constructors

        public ListsInWaitViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            ICustomControlListDataGridViewModel listDataGridViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.ListDataGridViewModel = listDataGridViewModel;
            this.dataGridViewModelRef = listDataGridViewModel as CustomControlListDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DetailListButtonCommand => this.detailListButtonCommand ?? (this.detailListButtonCommand = new DelegateCommand(
            () => this.navigationService.NavigateToView<DetailListInWaitViewModel, IDetailListInWaitViewModel>()));

        public ICustomControlListDataGridViewModel ListDataGridViewModel { get; }

        #endregion

        #region Methods

        public override Task OnEnterViewAsync()
        {
            var random = new Random();
            this.lists = new ObservableCollection<DataGridList>();
            for (var i = 0; i < random.Next(1, 30); i++)
            {
                this.lists.Add(new DataGridList
                {
                    Type = "Type",
                    List = $"List {i}",
                    Description = $"List {i} description",
                    Machines = $"{random.Next(1, 10)}, {random.Next(1, 10)}, {random.Next(1, 10)}"
                }
                );
            }
            this.dataGridViewModelRef.Lists = this.lists;
            this.dataGridViewModelRef.SelectedList = this.lists[0];
            this.DataGridViewModel = this.dataGridViewModelRef;

            return Task.CompletedTask;
        }

        #endregion
    }
}

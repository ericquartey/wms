using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists
{
    public class ListsInWaitViewModel : BindableBase, IListsInWaitViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlListDataGridViewModel dataGridViewModelRef;

        private ICommand detailListButtonCommand;

        private IEventAggregator eventAggregator;

        private ObservableCollection<DataGridList> lists;

        private DataGridList selectedList;

        #endregion

        #region Constructors

        public ListsInWaitViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DetailListButtonCommand => this.detailListButtonCommand ?? (this.detailListButtonCommand = new DelegateCommand(
            () => NavigationService.NavigateToView<DetailListInWaitViewModel, IDetailListInWaitViewModel>()));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlListDataGridViewModel>() as CustomControlListDataGridViewModel;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.lists = new ObservableCollection<DataGridList>();
            for (int i = 0; i < random.Next(0, 30); i++)
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

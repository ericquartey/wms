using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail
{
    public class DetailListInWaitViewModel : BaseViewModel, IDetailListInWaitViewModel
    {
        #region Fields

        private readonly CustomControlListDetailDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridListDetail> lists;

        #endregion

        #region Constructors

        public DetailListInWaitViewModel(
            IEventAggregator eventAggregator,
            ICustomControlListDetailDataGridViewModel listDetailDataGridViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.ListDetailDataGridViewModel = listDetailDataGridViewModel;
            this.dataGridViewModelRef = listDetailDataGridViewModel as CustomControlListDetailDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICustomControlListDetailDataGridViewModel ListDetailDataGridViewModel { get; }

        #endregion

        #region Methods

        public override Task OnEnterViewAsync()
        {
            var random = new Random();
            this.lists = new ObservableCollection<DataGridListDetail>();
            for (var i = 0; i < random.Next(1, 30); i++)
            {
                this.lists.Add(new DataGridListDetail
                {
                    Item = $"Item {i}",
                    Description = $"This is item {i}",
                    Machine = $"{random.Next(0, 30)}",
                    Quantity = $"{random.Next(10, 1000)}",
                    Row = $"{random.Next(0, 20)}",
                }
                );
            }
            this.dataGridViewModelRef.Lists = this.lists;
            this.dataGridViewModelRef.SelectedList = this.lists[0];

            return Task.CompletedTask;
        }

        #endregion
    }
}

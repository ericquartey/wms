using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class MachineStatisticsViewModel : BaseViewModel, IMachineStatisticsViewModel
    {
        #region Fields

        private readonly CustomControlItemStatisticsDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridItemStatistics> items;

        #endregion

        #region Constructors

        public MachineStatisticsViewModel(
            IEventAggregator eventAggregator,
            ICustomControlItemStatisticsDataGridViewModel itemStatisticsDataGridViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.dataGridViewModelRef = itemStatisticsDataGridViewModel as CustomControlItemStatisticsDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.items = new ObservableCollection<DataGridItemStatistics>();
            for (var i = 0; i < random.Next(1, 30); i++)
            {
                this.items.Add(new DataGridItemStatistics
                {
                    ItemClass = $"Item class {i}",
                    ItemPercentage = $"{random.Next(0, 100)}",
                    ItemQuantity = $"{random.Next(1, 100)}",
                    MovementeQuantity = $"{random.Next(100, 1000)}",
                    MovementPercentage = $"{random.Next(0, 100)}",
                }
                );
            }
            this.dataGridViewModelRef.Items = this.items;
            this.dataGridViewModelRef.SelectedItem = this.items[0];
        }

        #endregion
    }
}

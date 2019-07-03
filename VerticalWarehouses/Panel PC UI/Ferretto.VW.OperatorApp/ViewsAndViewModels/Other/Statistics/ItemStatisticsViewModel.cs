using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class ItemStatisticsViewModel : BindableBase, IItemStatisticsViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlItemStatisticsDataGridViewModel dataGridViewModelRef;

        private IEventAggregator eventAggregator;

        private ObservableCollection<DataGridItemStatistics> items;

        #endregion

        #region Constructors

        public ItemStatisticsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

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
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlItemStatisticsDataGridViewModel>() as CustomControlItemStatisticsDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.items = new ObservableCollection<DataGridItemStatistics>();
            for (int i = 0; i < random.Next(1, 30); i++)
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

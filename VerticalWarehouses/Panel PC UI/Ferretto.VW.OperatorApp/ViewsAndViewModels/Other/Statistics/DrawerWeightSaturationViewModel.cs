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
    public class DrawerWeightSaturationViewModel : BaseViewModel, IDrawerWeightSaturationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private BindableBase dataGridViewModel;

        private readonly CustomControlDrawerWeightSaturationDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridDrawerWeightSaturation> drawers;

        #endregion

        #region Constructors

        public DrawerWeightSaturationViewModel(
            IEventAggregator eventAggregator,
            ICustomControlDrawerWeightSaturationDataGridViewModel drawerWeightSaturationDataGridViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.dataGridViewModelRef = drawerWeightSaturationDataGridViewModel as CustomControlDrawerWeightSaturationDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;

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
            this.drawers = new ObservableCollection<DataGridDrawerWeightSaturation>();

            for (var i = 0; i < random.Next(2, 30); i++)
            {
                this.drawers.Add(new DataGridDrawerWeightSaturation
                {
                    Drawer = $"Drawer {i}",
                    Height = $"{random.Next(30, 150)}",
                    Tare = $"{random.Next(100, 200)}",
                    ActualGrossWeight = $"{random.Next(200, 1000)}",
                    MaxWeight = $"{random.Next(500, 1200)}",
                    WeightPercentage = $"{random.Next(0, 100)}",
                }
                );
            }
            this.dataGridViewModelRef.Drawers = this.drawers;
            this.dataGridViewModelRef.SelectedDrawer = this.drawers[0];
        }

        #endregion
    }
}

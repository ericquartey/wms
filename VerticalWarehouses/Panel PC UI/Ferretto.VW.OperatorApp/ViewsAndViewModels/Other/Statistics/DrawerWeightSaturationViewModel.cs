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
using Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class DrawerWeightSaturationViewModel : BindableBase, IDrawerWeightSaturationViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlDrawerWeightSaturationDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridDrawerWeightSaturation> drawers;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public DrawerWeightSaturationViewModel(IEventAggregator eventAggregator)
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
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlDrawerWeightSaturationDataGridViewModel>() as CustomControlDrawerWeightSaturationDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.drawers = new ObservableCollection<DataGridDrawerWeightSaturation>();

            for (int i = 0; i < random.Next(2, 30); i++)
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
            this.dataGridViewModelRef.Drawers = drawers;
            this.dataGridViewModelRef.SelectedDrawer = drawers[0];
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

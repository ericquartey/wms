using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CustomControls.Controls;
using System.Collections.ObjectModel;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.CustomControls.Interfaces;

    public class DrawerSpaceSaturationViewModel : BindableBase, IDrawerSpaceSaturationViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlDrawerSaturationDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridDrawerSaturation> drawers;

        private ICommand drawerWeightSaturationButtonCommand;

        private IEventAggregator eventAggregator;

        private DataGridDrawerSaturation selectedDrawer;

        private IWmsDataProvider wmsDataProvider;

        #endregion

        #region Constructors

        public DrawerSpaceSaturationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ObservableCollection<DataGridDrawerSaturation> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public ICommand DrawerWeightSaturationButtonCommand => this.drawerWeightSaturationButtonCommand ?? (this.drawerWeightSaturationButtonCommand = new DelegateCommand(() =>
                        {
                            NavigationService.NavigateToView<DrawerWeightSaturationViewModel, IDrawerWeightSaturationViewModel>();
                        }));

        public BindableBase NavigationViewModel { get; set; }

        public DataGridDrawerSaturation SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlDrawerSaturationDataGridViewModel>() as CustomControlDrawerSaturationDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;
            this.wmsDataProvider = this.container.Resolve<IWmsDataProvider>();
        }

        public async Task OnEnterViewAsync()
        {
            this.Drawers = new ObservableCollection<DataGridDrawerSaturation>();
            var random = new Random();
            for (int i = 0; i < random.Next(5, 20); i++)
            {
                var cell = new DataGridDrawerSaturation
                {
                    DrawerId = random.Next(0, 50).ToString(),
                    Missions = string.Concat(random.Next(0, 4).ToString(), ", ", random.Next(0, 4).ToString()),
                    Compartments = random.Next(0, 100).ToString(),
                    Filling = random.Next(0, 30).ToString(),
                    FillingPercentage = random.Next(0, 100).ToString()
                };
                this.Drawers.Add(cell);
            }
            this.SelectedDrawer = this.Drawers[0];

            this.dataGridViewModelRef.Drawers = this.Drawers;
            this.dataGridViewModelRef.SelectedDrawer = this.SelectedDrawer;

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

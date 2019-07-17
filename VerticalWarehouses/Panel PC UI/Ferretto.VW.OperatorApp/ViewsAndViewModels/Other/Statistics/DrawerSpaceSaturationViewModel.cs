using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class DrawerSpaceSaturationViewModel : BaseViewModel, IDrawerSpaceSaturationViewModel
    {
        #region Fields

        private readonly CustomControlDrawerSaturationDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridDrawerSaturation> drawers;

        private ICommand drawerWeightSaturationButtonCommand;

        private DataGridDrawerSaturation selectedDrawer;

        #endregion

        #region Constructors

        public DrawerSpaceSaturationViewModel(
            IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider,
            INavigationService navigationService,
            ICustomControlDrawerSaturationDataGridViewModel drawerSaturationDataGridViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.wmsDataProvider = wmsDataProvider;
            this.navigationService = navigationService;
            this.dataGridViewModelRef = drawerSaturationDataGridViewModel as CustomControlDrawerSaturationDataGridViewModel;
            this.dataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ObservableCollection<DataGridDrawerSaturation> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public ICommand DrawerWeightSaturationButtonCommand => this.drawerWeightSaturationButtonCommand ?? (this.drawerWeightSaturationButtonCommand = new DelegateCommand(() =>
                        {
                            this.navigationService.NavigateToView<DrawerWeightSaturationViewModel, IDrawerWeightSaturationViewModel>();
                        }));

        public DataGridDrawerSaturation SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            this.Drawers = new ObservableCollection<DataGridDrawerSaturation>();
            var random = new Random();
            for (var i = 0; i < random.Next(5, 20); i++)
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

        #endregion
    }
}

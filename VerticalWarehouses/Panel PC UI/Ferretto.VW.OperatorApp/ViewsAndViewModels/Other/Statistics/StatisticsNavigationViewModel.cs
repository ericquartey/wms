using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsNavigationViewModel : BindableBase, IStatisticsNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand machineStatisticsButtonCommand;
        private ICommand drawerSpaceSaturationButtonCommand;
        private ICommand cellsStatisticsButtonCommand;
        private ICommand errorsStatisticsButtonCommand;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public StatisticsNavigationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand MachineStatisticsButtonCommand => this.machineStatisticsButtonCommand ?? (this.machineStatisticsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<MachineStatisticsViewModel, IMachineStatisticsViewModel>();
        }));

        public ICommand DrawerSpaceSaturationButtonCommand => this.drawerSpaceSaturationButtonCommand ?? (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<DrawerSpaceSaturationViewModel, IDrawerSpaceSaturationViewModel>();
        }));

        public ICommand CellsStatisticsButtonCommand => this.cellsStatisticsButtonCommand ?? (this.cellsStatisticsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<CellsStatisticsViewModel, ICellsStatisticsViewModel>();
        }));

        public ICommand ErrorsStatisticsButtonCommand => this.errorsStatisticsButtonCommand ?? (this.errorsStatisticsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<ErrorsStatisticsViewModel, IErrorsStatisticsViewModel>();
        }));

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
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
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

using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsNavigationViewModel : BaseViewModel, IStatisticsNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private ICommand cellsStatisticsButtonCommand;

        private ICommand drawerSpaceSaturationButtonCommand;

        private ICommand errorsStatisticsButtonCommand;

        private ICommand machineStatisticsButtonCommand;

        #endregion

        #region Constructors

        public StatisticsNavigationViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand CellsStatisticsButtonCommand => this.cellsStatisticsButtonCommand ?? (this.cellsStatisticsButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<CellsStatisticsViewModel, ICellsStatisticsViewModel>();
        }));

        public ICommand DrawerSpaceSaturationButtonCommand =>
            this.drawerSpaceSaturationButtonCommand
            ??
            (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<DrawerWeightSaturationViewModel, IDrawerWeightSaturationViewModel>();
                }));

        public ICommand ErrorsStatisticsButtonCommand =>
            this.errorsStatisticsButtonCommand
            ??
            (this.errorsStatisticsButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<ErrorsStatisticsViewModel, IErrorsStatisticsViewModel>();
                }));

        public ICommand MachineStatisticsButtonCommand =>
            this.machineStatisticsButtonCommand
            ??
            (this.machineStatisticsButtonCommand = new DelegateCommand(() =>
                {
                }));

        #endregion
    }
}

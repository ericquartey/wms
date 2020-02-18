using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsNavigationViewModel : BaseViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand cellsStatisticsButtonCommand;

        private ICommand drawerSpaceSaturationButtonCommand;

        private ICommand errorsStatisticsButtonCommand;

        private ICommand machineStatisticsButtonCommand;

        #endregion

        #region Constructors

        public StatisticsNavigationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand CellsStatisticsButtonCommand => this.cellsStatisticsButtonCommand ?? (this.cellsStatisticsButtonCommand = new DelegateCommand(() =>
        {
        }));

        public ICommand DrawerSpaceSaturationButtonCommand =>
            this.drawerSpaceSaturationButtonCommand
            ??
            (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
                {
                }));

        public ICommand ErrorsStatisticsButtonCommand =>
            this.errorsStatisticsButtonCommand
            ??
            (this.errorsStatisticsButtonCommand = new DelegateCommand(() =>
                {
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

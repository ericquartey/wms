using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other
{
    public class OtherNavigationViewModel : BaseViewModel, IOtherNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private ICommand drawerCompactingButtonCommand;

        private ICommand immediateDrawerCallButtonCommand;

        private ICommand maintenanceMainPageButtonCommand;

        private ICommand statisticsButtonCommand;

        #endregion

        #region Constructors

        public OtherNavigationViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DrawerCompactingButtonCommand =>
            this.drawerCompactingButtonCommand
            ??
            (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>()));

        public ICommand ImmediateDrawerCallButtonCommand =>
            this.immediateDrawerCallButtonCommand
            ??
            (this.immediateDrawerCallButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<ImmediateDrawerCallViewModel, IImmediateDrawerCallViewModel>()));

        public ICommand MaintenanceMainPageButtonCommand =>
            this.maintenanceMainPageButtonCommand
            ??
            (this.maintenanceMainPageButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<MaintenanceMainPageViewModel, IMaintenanceMainPageViewModel>()));

        public ICommand StatisticsButtonCommand =>
            this.statisticsButtonCommand
            ??
            (this.statisticsButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<StatisticsGeneralDataViewModel, IStatisticsGeneralDataViewModel>()));

        #endregion
    }
}

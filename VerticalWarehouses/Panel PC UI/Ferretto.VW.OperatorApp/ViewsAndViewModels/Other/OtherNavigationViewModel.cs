using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class OtherNavigationViewModel : BindableBase, IOtherNavigationViewModel
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

        public BindableBase NavigationViewModel { get; set; }

        public ICommand StatisticsButtonCommand =>
            this.statisticsButtonCommand
            ??
            (this.statisticsButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<StatisticsGeneralDataViewModel, IStatisticsGeneralDataViewModel>()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
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

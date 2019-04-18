using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class OtherNavigationViewModel : BindableBase, IOtherNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand drawerCompactingButtonCommand;
        private ICommand immediateDrawerCallButtonCommand;
        private ICommand maintenanceMainPageButtonCommand;
        private ICommand statisticsButtonCommand;
        private IUnityContainer container;

        #endregion

        #region Constructors

        public OtherNavigationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DrawerCompactingButtonCommand => this.drawerCompactingButtonCommand ?? (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>();
        }));

        public ICommand ImmediateDrawerCallButtonCommand => this.immediateDrawerCallButtonCommand ?? (this.immediateDrawerCallButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<ImmediateDrawerCallViewModel, IImmediateDrawerCallViewModel>();
        }));

        public ICommand MaintenanceMainPageButtonCommand => this.maintenanceMainPageButtonCommand ?? (this.maintenanceMainPageButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<MaintenanceMainPageViewModel, IMaintenanceMainPageViewModel>();
        }));

        public ICommand StatisticsButtonCommand => this.statisticsButtonCommand ?? (this.statisticsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<StatisticsGeneralDataViewModel, IStatisticsGeneralDataViewModel>();
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

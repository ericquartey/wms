using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class MaintenanceMainPageViewModel : BindableBase, IMaintenanceMainPageViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private ICommand maintenanceDetailButtonCommand;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public MaintenanceMainPageViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand MaintenanceDetailButtonCommand => this.maintenanceDetailButtonCommand ?? (this.maintenanceDetailButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<MaintenanceDetailViewModel, IMaintenanceDetailViewModel>();
        }));

        public BindableBase NavigationViewModel { get; set; }

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

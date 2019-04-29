using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class CellsStatisticsViewModel : BindableBase, ICellsStatisticsViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private ICommand drawerCompactingButtonCommand;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public CellsStatisticsViewModel(IEventAggregator eventAggregator)
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

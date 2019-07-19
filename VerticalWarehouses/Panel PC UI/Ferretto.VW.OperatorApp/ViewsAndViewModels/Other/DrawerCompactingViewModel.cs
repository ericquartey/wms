//Header test C#
using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class DrawerCompactingViewModel : BaseViewModel, IDrawerCompactingViewModel
    {
        #region Private Fields

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private ICommand drawerCompactingDetailButtonCommand;

        #endregion

        #region Public Constructors

        public DrawerCompactingViewModel(IEventAggregator eventAggregator, INavigationService navigationService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Public Properties

        public ICommand DrawerCompactingDetailButtonCommand => this.drawerCompactingDetailButtonCommand ?? (this.drawerCompactingDetailButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<DrawerCompactingDetailViewModel, IDrawerCompactingDetailViewModel>();
        }));

        #endregion
    }
}

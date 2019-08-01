//Header test C#

using System;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.Other
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

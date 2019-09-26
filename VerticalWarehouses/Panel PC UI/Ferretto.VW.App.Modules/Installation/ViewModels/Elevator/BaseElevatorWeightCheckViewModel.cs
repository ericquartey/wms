using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ferretto.VW.App.Controls;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseElevatorWeightCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        #endregion

        #region Constructors

        public BaseElevatorWeightCheckViewModel(
            IEventAggregator eventAggregator)
            : base(Services.PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.InitializeNavigationMenu();
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        protected abstract void RaiseCanExecuteChanged();

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Elevator.WeightCheck.STEP1,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step1,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Elevator.WeightCheck.STEP2,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step2,
                    trackCurrentView: false));
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Events;

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

        //public bool IsExecutingProcedure
        //{
        //    get => this.isExecutingProcedure;
        //    protected set
        //    {
        //        if (this.SetProperty(ref this.isExecutingProcedure, value))
        //        {
        //            this.RaiseCanExecuteChanged();
        //        }
        //    }
        //}

        #region Properties

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;
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

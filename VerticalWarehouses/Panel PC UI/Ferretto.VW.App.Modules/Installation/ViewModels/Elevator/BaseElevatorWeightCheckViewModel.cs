﻿using System.Collections.Generic;
using System.ComponentModel;
using Ferretto.VW.App.Controls;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal abstract class BaseElevatorWeightCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        #endregion

        #region Constructors

        public BaseElevatorWeightCheckViewModel()
            : base(Services.PresentationMode.Installer)
        {
            this.InitializeNavigationMenu();
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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Elevator.WeightCheck.STEP1,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.Localized.Get("InstallationApp.Step1"),
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Elevator.WeightCheck.STEP2,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.Localized.Get("InstallationApp.Step2"),
                    trackCurrentView: false));
        }

        #endregion
    }
}

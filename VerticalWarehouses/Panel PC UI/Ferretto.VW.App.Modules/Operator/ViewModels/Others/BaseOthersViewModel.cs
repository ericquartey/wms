﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class BaseOthersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        #endregion

        #region Constructors

        public BaseOthersViewModel()
            : base(PresentationMode.Operator)
        {
            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.IMMEDIATEDRAWERCALL,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.OtherNavigationImmediateDrawerCall,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.DRAWERCOMPACTING,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.OtherNavigationCompaction,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.NAVIGATION,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.OtherNavigationStatistics,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.MAINTENANCE,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.OtherNavigationMaintenance,
                    trackCurrentView: false));
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class BaseOthersViewModel : BaseOperatorViewModel
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

        public override EnableMask EnableMask => EnableMask.Any;

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
                    Utils.Modules.Operator.Others.IMMEDIATELOADINGUNITCALL,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.Localized.Get("OperatorApp.OtherNavigationImmediateDrawerCall"),
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.DrawerCompacting.MAIN,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.Localized.Get("OperatorApp.OtherNavigationCompaction"),
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.NAVIGATION,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.Localized.Get("OperatorApp.OtherNavigationStatistics"),
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Maintenance.MAIN,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.Localized.Get("OperatorApp.OtherNavigationMaintenance"),
                    trackCurrentView: true));
        }

        #endregion
    }
}

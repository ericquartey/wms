using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Resources;

namespace Ferretto.VW.App.Installation.Models
{
    public class MainNavigationMenuItem : NavigationMenuItem
    {
        #region Fields

        private InstallationMenus menuItemType;

        #endregion

        #region Constructors

        public MainNavigationMenuItem(
            InstallationMenus menuItemType,
            string viewModelName,
            string moduleName,
            string description,
            bool trackCurrentView)
            : base(viewModelName, moduleName, description, trackCurrentView)
        {
            this.MenuItemType = menuItemType;
        }

        #endregion

        #region Properties

        public InstallationMenus MenuItemType
        {
            get => this.menuItemType;
            set => this.SetProperty(ref this.menuItemType, value);
        }

        #endregion
    }
}

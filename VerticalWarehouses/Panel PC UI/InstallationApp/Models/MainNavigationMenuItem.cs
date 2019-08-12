using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Resources;

namespace Ferretto.VW.App.Installation.Models
{
    public class MainNavigationMenuItem : NavigationMenuItem
    {
        #region Fields

        private InstallatorMenus menuItemType;

        #endregion

        #region Constructors

        public MainNavigationMenuItem(InstallatorMenus menuItemType, string viewModelName, string moduleName, string description, bool isTrackable)
            : base(viewModelName, moduleName, description, isTrackable)
        {
            this.MenuItemType = menuItemType;
        }

        #endregion

        #region Properties

        public InstallatorMenus MenuItemType
        {
            get => this.menuItemType;
            set => this.SetProperty(ref this.menuItemType, value);
        }

        #endregion
    }
}

using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Resources;

namespace Ferretto.VW.App.Installation.Models
{
    public class MainNavigationMenuItem : NavigationMenuItem
    {
        #region Fields

        private CellsLoadingUnitsMenus cellsLoadingUnitsItemType;

        private InstallationMenus menuItemType;

        #endregion

        #region Constructors

        public MainNavigationMenuItem(
            CellsLoadingUnitsMenus menuItemType,
            string viewModelName,
            string moduleName,
            string description,
            bool trackCurrentView)
            : base(viewModelName, moduleName, description, trackCurrentView)
        {
            this.CellsLoadingUnitsItemType = menuItemType;
        }

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

        public CellsLoadingUnitsMenus CellsLoadingUnitsItemType
        {
            get => this.cellsLoadingUnitsItemType;
            set => this.SetProperty(ref this.cellsLoadingUnitsItemType, value);
        }

        public InstallationMenus MenuItemType
        {
            get => this.menuItemType;
            set => this.SetProperty(ref this.menuItemType, value);
        }

        #endregion
    }
}

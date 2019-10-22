using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Operator.Resources;

namespace Ferretto.VW.App.Operator.Models
{
    public class MainNavigationMenuItem : NavigationMenuItem
    {
        #region Fields

        private OperatorMenus menuItemType;

        #endregion

        #region Constructors

        public MainNavigationMenuItem(
            OperatorMenus menuItemType,
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

        public OperatorMenus MenuItemType
        {
            get => this.menuItemType;
            set => this.SetProperty(ref this.menuItemType, value);
        }

        #endregion
    }
}

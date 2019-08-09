using System.ComponentModel;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Models;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel()
            : base(Services.PresentationMode.Installator)
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Resources.InstallatorMenus.None,
                    Utils.Modules.Installation.ManualMovements.VERTICALENGINE,
                    nameof(Utils.Modules.Installation),
                    Ferretto.VW.App.Resources.InstallationApp.VerticalAxis));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Resources.InstallatorMenus.None,
                    Utils.Modules.Installation.ManualMovements.HORIZONTALENGINE,
                    nameof(Utils.Modules.Installation),
                    Ferretto.VW.App.Resources.InstallationApp.HorizontalAxis));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Resources.InstallatorMenus.None,
                    Utils.Modules.Installation.ManualMovements.SHUTTER,
                    nameof(Utils.Modules.Installation),
                    Ferretto.VW.App.Resources.InstallationApp.Shutter));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Resources.InstallatorMenus.None,
                    Utils.Modules.Installation.ManualMovements.CAROUSEL,
                    nameof(Utils.Modules.Installation),
                    Ferretto.VW.App.Resources.InstallationApp.Carousel));
        }

        #endregion

        #region Properties

        public BindingList<NavigationMenuItem> MenuItems
        {
            get => this.menuItems;
            set => this.SetProperty(ref this.menuItems, value);
        }

        #endregion
    }
}

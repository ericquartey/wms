using System.Collections.ObjectModel;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils.Menu;
using Ferretto.Common.Utils.Modules;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Layout
{
    public class MenuViewModel : BaseNavigationViewModel
    {
        public MenuViewModel()
        {
            this.Inizialize();
        }

        private void Inizialize()
        {
            this.Items = new ObservableCollection<NavMenuItem>();
            var menu = new AppMenu();
            foreach (var item in menu.Menu.Items)
            {
                this.Items.Add(new NavMenuItem(item, string.Empty));
            }

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Appear(nameof(Catalog), Catalog.ITEMSANDDETAILS);
        }

        public ObservableCollection<NavMenuItem> Items { get; set; }
    }
}

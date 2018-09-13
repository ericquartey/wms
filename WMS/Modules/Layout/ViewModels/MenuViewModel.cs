using System.Collections.ObjectModel;
using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils.Menu;
using Prism.Commands;

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
        }

        public ObservableCollection<NavMenuItem> Items { get; set; }
    }
}

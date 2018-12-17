using System.Collections.ObjectModel;
using Ferretto.Common.Controls;
using Ferretto.Common.Utils.Menu;

namespace Ferretto.WMS.Modules.Layout
{
    public class MenuViewModel : BaseNavigationViewModel
    {
        #region Constructors

        public MenuViewModel()
        {
            this.Inizialize();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<NavMenuItem> Items { get; set; }

        #endregion Properties

        #region Methods

        private void Inizialize()
        {
            this.Items = new ObservableCollection<NavMenuItem>();
            var menu = new AppMenu();
            foreach (var item in menu.Menu.Items)
            {
                this.Items.Add(new NavMenuItem(item, string.Empty));
            }
        }

        #endregion Methods
    }
}

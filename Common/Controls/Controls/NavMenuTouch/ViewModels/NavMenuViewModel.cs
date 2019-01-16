using System;
using System.Collections.ObjectModel;
using Ferretto.Common.Utils.Menu;

namespace Ferretto.Common.Controls
{
    public class NavMenuViewModel
    {
        #region Constructors

        public NavMenuViewModel()
        {
            this.Inizialize();
        }

        public NavMenuViewModel(ObservableCollection<NavMenuItem> items)
        {
            this.Items = items;
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

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

        #endregion

        #region Properties

        public ObservableCollection<NavMenuItem> Items { get; set; }

        #endregion

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

        #endregion
    }
}

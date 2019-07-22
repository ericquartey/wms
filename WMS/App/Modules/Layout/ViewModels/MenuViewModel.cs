﻿using System.Collections.ObjectModel;
using Ferretto.WMS.App.Controls;

namespace Ferretto.WMS.App.Modules.Layout
{
    public class MenuViewModel : BaseNavigationViewModel
    {
        private readonly ObservableCollection<NavMenuItem> menuItems = new ObservableCollection<NavMenuItem>();

        #region Constructors

        public MenuViewModel()
        {
            this.Inizialize();
        }

        #endregion

        #region Properties

        public ObservableCollection<NavMenuItem> Items { get => this.menuItems; }

        #endregion

        #region Methods

        private void Inizialize()
        {
            var menu = new AppMenu();
            foreach (var item in menu.Menu.Items)
            {
                this.Items.Add(new NavMenuItem(item, string.Empty));
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils.Menu;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public class NavMenuItem : IMenuItemViewModel
    {
        #region Constructors

        public NavMenuItem()
        {
            this.Command = new DelegateCommand(this.CommandAction);
        }

        public NavMenuItem(MainMenuItem item, string currBreadCrumb) : this()
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.DisplayName = item.Name;
            this.BackColor = ( (SolidColorBrush) System.Windows.Application.Current.Resources[item.BackGroundColor] )
                .Color.ToString();
            this.Image = item.Image;
            this.ModuleName = item.ModuleName;
            this.ViewName = item.ViewName;
            this.IsRootLevel = string.IsNullOrEmpty(currBreadCrumb);
            this.AddChild(item.Children, currBreadCrumb);
        }

        #endregion Constructors

        #region Properties

        public string BackColor { get; set; }

        public TileNavMenuChildItem Child { get; set; }

        public ICommand Command { get; set; }

        public string DisplayName { get; set; }

        public bool HasChildren => ( this.Child != null &&
                                     this.Child.Children != null &&
                                     this.Child.Children.Count > 0 );

        public string Image { get; set; }

        public bool IsRootLevel { get; set; }

        public string ModuleName { get; set; }

        public string ViewName { get; set; }

        #endregion Properties

        #region Methods

        public void CommandAction()
        {
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Appear(this.ModuleName, this.ViewName);
        }

        private void AddChild(ICollection<MainMenuItem> children, string currBreadCrumb)
        {
            if (children == null ||
                children.Count == 0)
            {
                return;
            }

            var breadCrumb = $"{currBreadCrumb}\\{this.DisplayName}";
            this.Child = new TileNavMenuChildItem(breadCrumb);
            foreach (var child in children)
            {
                this.Child.Children.Add(new NavMenuItem(child, breadCrumb));
            }
        }

        #endregion Methods
    }
}

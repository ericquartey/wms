using System;
using System.Collections.Generic;
using System.Windows.Input;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Ferretto.Common.Utils.Menu;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
  public class NavMenuItem : IMenuItemViewModel
  {    
    #region Ctor
    public NavMenuItem()
    {
      this.Command = new DelegateCommand(this.CommandAction);     
    }

    public NavMenuItem(MainMenuItem item, string currBreadCrumb) : this()
    {
      this.DisplayName = item.Name;
      this.BackColor = item.BackGroundColor;
      this.Image = item.Image;
      this.ModuleName = item.ModuleName;
      this.ViewName = item.ViewName;
      this.IsRootLevel = string.IsNullOrEmpty(currBreadCrumb);
      this.AddChild(item.Children, currBreadCrumb);
    }

    private void AddChild(List<MainMenuItem> children, string currBreadCrumb)
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
    #endregion

    #region Properties
    public string DisplayName { get; set; }

    public string BackColor { get; set; }

    public string Image { get; set; }

    public string ModuleName { get; set; }

    public string ViewName { get; set; }

    public bool   IsRootLevel { get; set; }    

    public ICommand Command { get; set; }

    public TileNavMenuChildItem Child { get; set; }

    public bool HasChildren
    {
      get
      {
        return (this.Child != null &&
                this.Child.Children != null &&
                this.Child.Children.Count > 0);
      }
    }
    #endregion

    #region Methods
    public void CommandAction()
    {
      var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
      navigationService.Appear(this.ModuleName, this.ViewName);
    }
    #endregion
  }
}

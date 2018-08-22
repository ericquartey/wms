using System.Collections.ObjectModel;
using System.Windows.Input;
using Ferretto.Common.Controls.Interfaces;
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

    public NavMenuItem(System.String displayName, System.String backColor, System.String image, System.String moduleName, System.String viewName, ICommand command, ObservableCollection<NavMenuItem> children)
    {
      this.DisplayName = displayName;
      this.BackColor = backColor;
      this.Image = image;
      this.ModuleName = moduleName;
      this.ViewName = viewName;
      this.Command = command;
      this.Children = children;
    }
    #endregion

    #region Properties
    public string DisplayName { get; set; }

    public string BackColor { get; set; }

    public string Image { get; set; }

    public string ModuleName { get; set; }

    public string ViewName { get; set; }

    public ICommand Command { get; set; }

    public ObservableCollection<NavMenuItem> Children
    {
      get;
      set;
    }

    public bool HasChildren
    {
      get
      {
        return this.Children != null && this.Children.Count > 0;
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

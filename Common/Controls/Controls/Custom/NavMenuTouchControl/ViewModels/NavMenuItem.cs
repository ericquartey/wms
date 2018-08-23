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

    public NavMenuItem(System.String displayName, System.String backColor, System.String image, System.String moduleName, System.String viewName, ICommand command) : this()
    {
      this.DisplayName = displayName;
      this.BackColor = backColor;
      this.Image = image;
      this.ModuleName = moduleName;
      this.ViewName = viewName;
      this.Command = command;     
    }
    #endregion

    #region Properties
    public string DisplayName { get; set; }

    public string BackColor { get; set; }

    public string Image { get; set; }

    public string ModuleName { get; set; }

    public string ViewName { get; set; }

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

using System.Linq;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
  public partial class WMSView : UserControl, INavigableView
  {
    #region Fields
    private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
    #endregion

    #region Properties
    public string Token { get; set; }
    public string MapId { get; set; }
    public string Title { get; set; }
    #endregion

    #region Ctor
    public WMSView()
    {      
      this.Loaded += WMSView_Loaded;
    }
    #endregion

    #region Event
    private void WMSView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
      if (this.IsWrongDataContext() == false)
      {
        return;
      }

      if (string.IsNullOrEmpty(this.MapId) == false)
      {
        // Is Main WMSView registered
        this.DataContext = navigationService.GetViewModelByMapId(this.MapId);
      }
      
      else if (this.IsChildOfMainView())
      {
        // Is children of WMSView       
        this.DataContext = navigationService.RegisterAndGetViewModel(this.GetType().ToString(), this.GetMainViewToken());        
      }
      else
      {
        // Stand alone case
        this.DataContext = navigationService.GetViewModelByName(this.GetAttachedViewModel());
      }

      ((INavigableViewModel)this.DataContext)?.OnAppear();
     
    }
    #endregion

    #region Methods
    private bool IsChildOfMainView()
    {      
      return (LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WMSView) != null);     
    }
    private string GetMainViewToken()
    {
      string token = null;
      var parentMainView = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WMSView);
      if (parentMainView != null)
      {
         token = ((WMSView)parentMainView).Token;
      }
      return token;
    }
    
    private bool IsWrongDataContext()
    {
      if (this.DataContext == null)
      {
        return true;
      }
      var dataContextName = this.DataContext.GetType().ToString();      
      return (GetAttachedViewModel().Equals(dataContextName) == false);
    }

    private string GetAttachedViewModel()
    {
      return $"{this.GetType().ToString()}{Configuration.Common.MODEL_SUFIX}";
    }
    #endregion
  }
}
